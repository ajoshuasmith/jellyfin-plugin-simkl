using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Plugin.Simkl.API;
using Jellyfin.Plugin.Simkl.API.Objects;
using Jellyfin.Plugin.Simkl.API.Requests;
using Jellyfin.Plugin.Simkl.API.Responses;
using Jellyfin.Plugin.Simkl.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;
using LibraryUser = Jellyfin.Database.Implementations.Entities.User;

namespace Jellyfin.Plugin.Simkl.Services;

/// <summary>
/// Handles importing watched state from Simkl into Jellyfin.
/// </summary>
public class SimklImportService
{
    private readonly ILogger<SimklImportService> _logger;
    private readonly SimklApi _simklApi;
    private readonly IUserManager _userManager;
    private readonly IUserDataManager _userDataManager;
    private readonly ILibraryManager _libraryManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimklImportService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    /// <param name="simklApi">Simkl API abstraction.</param>
    /// <param name="userManager">Jellyfin user manager.</param>
    /// <param name="userDataManager">User data manager.</param>
    /// <param name="libraryManager">Library manager.</param>
    public SimklImportService(
        ILogger<SimklImportService> logger,
        SimklApi simklApi,
        IUserManager userManager,
        IUserDataManager userDataManager,
        ILibraryManager libraryManager)
    {
        _logger = logger;
        _simklApi = simklApi;
        _userManager = userManager;
        _userDataManager = userDataManager;
        _libraryManager = libraryManager;
    }

    /// <summary>
    /// Imports watched state for all users that enabled the feature.
    /// </summary>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token propagated from the scheduler.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ImportEnabledUsersAsync(IProgress<double>? progress, CancellationToken cancellationToken)
    {
        var configs = SimklPlugin.Instance?.Configuration.UserConfigs
            .Where(c => c.SyncFromSimkl)
            .ToList() ?? new List<UserConfig>();

        if (configs.Count == 0)
        {
            _logger.LogInformation("No users enabled Simkl import, skipping task.");
            return;
        }

        var percentStep = 100d / configs.Count;
        var completed = 0;

        foreach (var config in configs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ImportUserAsync(config.Id, dryRun: false, cancellationToken).ConfigureAwait(false);
            completed++;
            progress?.Report(percentStep * completed);
        }
    }

    /// <summary>
    /// Imports watched state for a specific user.
    /// </summary>
    /// <param name="userId">The Jellyfin user identifier to process.</param>
    /// <param name="dryRun">Whether to log actions without persisting changes.</param>
    /// <param name="cancellationToken">Cancellation token from the caller.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ImportUserAsync(Guid userId, bool dryRun, CancellationToken cancellationToken)
    {
        var config = SimklPlugin.Instance?.Configuration.GetByGuid(userId);
        if (config == null || string.IsNullOrEmpty(config.UserToken))
        {
            _logger.LogWarning("Cannot import for user {UserId}. Missing configuration or token.", userId);
            return;
        }

        var user = _userManager.GetUserById(userId);
        if (user == null)
        {
            _logger.LogWarning("Cannot import for user {UserId}. Jellyfin user not found.", userId);
            return;
        }

        try
        {
            _logger.LogInformation("Starting watched import from Simkl for user {User} (dry-run: {DryRun})", user.Username, dryRun);

            var previousActivities = new ActivitySnapshot(config);
            var activities = await _simklApi.GetActivitiesAsync(config.UserToken).ConfigureAwait(false);
            if (activities == null)
            {
                _logger.LogWarning("Simkl activities response was null for user {User}", user.Username);
                return;
            }

            var movieLookup = BuildMovieLookup(user);
            var episodeLookup = BuildEpisodeLookup(user);

            var summary = new ImportSummary();

            await ImportMoviesAsync(config, user, previousActivities, movieLookup, summary, dryRun, cancellationToken).ConfigureAwait(false);
            await ImportShowsAsync(config, user, previousActivities, episodeLookup, summary, dryRun, cancellationToken).ConfigureAwait(false);
            await ImportAnimeAsync(config, user, previousActivities, episodeLookup, summary, dryRun, cancellationToken).ConfigureAwait(false);

            LogSummary(user.Username, summary, dryRun);

            if (!dryRun)
            {
                config.LastActivitiesAll = activities.All;
                config.LastActivitiesMovies = activities.Movies?.All;
                config.LastActivitiesShows = activities.TvShows?.All;
                config.LastActivitiesAnime = activities.Anime?.All;
                config.LastImportUtc = DateTime.UtcNow;
                SimklPlugin.Instance?.SaveConfiguration();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while importing Simkl data for user {UserId}", userId);
            throw;
        }
    }

    private async Task ImportMoviesAsync(
        UserConfig config,
        LibraryUser user,
        ActivitySnapshot previousActivities,
        Dictionary<string, Movie> movieLookup,
        ImportSummary summary,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var request = new SimklAllItemsRequest
        {
            Type = "movies",
            Status = "completed",
            DateFrom = previousActivities.Movies?.ToString("o")
        };

        var response = await _simklApi.GetAllItemsAsync(config.UserToken, request).ConfigureAwait(false);
        if (response?.Movies == null || response.Movies.Count == 0)
        {
            _logger.LogDebug("No movie entries returned for user {User}", user.Username);
            return;
        }

        foreach (var entry in response.Movies)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var match = FindMovie(entry.Movie, movieLookup);
            if (match == null)
            {
                summary.MoviesNotFound++;
                _logger.LogDebug("No movie match found locally for Simkl entry {Title}", entry.Movie?.Title);
                continue;
            }

            var watchedAt = entry.Movie?.WatchedAt ?? entry.LastWatchedAt ?? DateTime.UtcNow;
            await ApplyUserDataAsync(user, match, watchedAt, dryRun, summary, isEpisode: false, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task ImportShowsAsync(
        UserConfig config,
        LibraryUser user,
        ActivitySnapshot previousActivities,
        Dictionary<string, Episode> episodeLookup,
        ImportSummary summary,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var request = new SimklAllItemsRequest
        {
            Type = "shows",
            Status = "completed",
            DateFrom = previousActivities.Tv?.ToString("o"),
            Extended = "full",
            IncludeEpisodeWatchedAt = true
        };

        var response = await _simklApi.GetAllItemsAsync(config.UserToken, request).ConfigureAwait(false);
        if (response?.Shows == null || response.Shows.Count == 0)
        {
            _logger.LogDebug("No show entries returned for user {User}", user.Username);
            return;
        }

        await ImportEpisodesAsync(user, response.Shows, episodeLookup, summary, dryRun, cancellationToken).ConfigureAwait(false);
    }

    private async Task ImportAnimeAsync(
        UserConfig config,
        LibraryUser user,
        ActivitySnapshot previousActivities,
        Dictionary<string, Episode> episodeLookup,
        ImportSummary summary,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var request = new SimklAllItemsRequest
        {
            Type = "anime",
            Status = "completed",
            DateFrom = previousActivities.Anime?.ToString("o"),
            Extended = "full",
            IncludeEpisodeWatchedAt = true
        };

        var response = await _simklApi.GetAllItemsAsync(config.UserToken, request).ConfigureAwait(false);
        if (response?.Anime == null || response.Anime.Count == 0)
        {
            _logger.LogDebug("No anime entries returned for user {User}", user.Username);
            return;
        }

        await ImportEpisodesAsync(user, response.Anime, episodeLookup, summary, dryRun, cancellationToken).ConfigureAwait(false);
    }

    private async Task ImportEpisodesAsync(
        LibraryUser user,
        IReadOnlyList<SimklAllItemsShow> showEntries,
        Dictionary<string, Episode> episodeLookup,
        ImportSummary summary,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        foreach (var entry in showEntries)
        {
            if (entry.Seasons == null || entry.Seasons.Count == 0)
            {
                continue;
            }

            foreach (var season in entry.Seasons)
            {
                if (!season.Number.HasValue || season.Episodes == null)
                {
                    continue;
                }

                foreach (var episodeInfo in season.Episodes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!episodeInfo.Number.HasValue)
                    {
                        continue;
                    }

                    var match = FindEpisode(entry.Show, season.Number.Value, episodeInfo.Number.Value, episodeLookup);
                    if (match == null)
                    {
                        summary.EpisodesNotFound++;
                        _logger.LogDebug("Unable to match episode {Show} S{Season}E{Episode} locally", entry.Show?.Title, season.Number, episodeInfo.Number);
                        continue;
                    }

                    var watchedAt = episodeInfo.WatchedAt ?? entry.LastWatchedAt ?? DateTime.UtcNow;
                    await ApplyUserDataAsync(user, match, watchedAt, dryRun, summary, isEpisode: true, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task ApplyUserDataAsync(
        LibraryUser user,
        BaseItem item,
        DateTime watchedAt,
        bool dryRun,
        ImportSummary summary,
        bool isEpisode,
        CancellationToken cancellationToken)
    {
        var userData = _userDataManager.GetUserData(user, item);
        if (userData == null)
        {
            _logger.LogWarning("Unable to load user data for {Item} and user {User}", item.Name, user.Username);
            return;
        }

        var needsUpdate = !userData.Played || (userData.LastPlayedDate == null || userData.LastPlayedDate < watchedAt);

        if (!needsUpdate)
        {
            return;
        }

        if (isEpisode)
        {
            summary.EpisodesImported++;
        }
        else
        {
            summary.MoviesImported++;
        }

        if (dryRun)
        {
            _logger.LogInformation("[Dry-Run] Would mark {Type} '{Name}' as watched for {User} at {Date}", isEpisode ? "episode" : "movie", item.Name, user.Username, watchedAt);
            return;
        }

        userData.Played = true;
        userData.LastPlayedDate = watchedAt;
        if (userData.PlayCount < 1)
        {
            userData.PlayCount = 1;
        }

        _userDataManager.SaveUserData(user, item, userData, UserDataSaveReason.Import, cancellationToken);
    }

    private static Movie? FindMovie(SimklMovie? source, Dictionary<string, Movie> lookup)
    {
        if (source == null)
        {
            return null;
        }

        foreach (var key in BuildMovieKeys(source))
        {
            if (lookup.TryGetValue(key, out var movie))
            {
                return movie;
            }
        }

        return null;
    }

    private static Episode? FindEpisode(SimklShow? show, int seasonNumber, int episodeNumber, Dictionary<string, Episode> lookup)
    {
        if (show?.Ids == null)
        {
            return null;
        }

        foreach (var key in BuildEpisodeKeys(show.Ids, seasonNumber, episodeNumber))
        {
            if (lookup.TryGetValue(key, out var episode))
            {
                return episode;
            }
        }

        return null;
    }

    private Dictionary<string, Movie> BuildMovieLookup(LibraryUser user)
    {
        var query = new InternalItemsQuery(user)
        {
            IncludeItemTypes = new[] { BaseItemKind.Movie },
            IsVirtualItem = false,
            Recursive = true
        };

        var movies = _libraryManager.GetItemList(query).OfType<Movie>();
        var lookup = new Dictionary<string, Movie>(StringComparer.OrdinalIgnoreCase);

        foreach (var movie in movies)
        {
            foreach (var key in BuildMovieKeys(movie))
            {
                if (!lookup.ContainsKey(key))
                {
                    lookup.Add(key, movie);
                }
            }
        }

        return lookup;
    }

    private Dictionary<string, Episode> BuildEpisodeLookup(LibraryUser user)
    {
        var query = new InternalItemsQuery(user)
        {
            IncludeItemTypes = new[] { BaseItemKind.Episode },
            IsVirtualItem = false,
            Recursive = true
        };

        var episodes = _libraryManager.GetItemList(query).OfType<Episode>();
        var lookup = new Dictionary<string, Episode>(StringComparer.OrdinalIgnoreCase);

        foreach (var episode in episodes)
        {
            if (!episode.ParentIndexNumber.HasValue || !episode.IndexNumber.HasValue)
            {
                continue;
            }

            foreach (var key in BuildEpisodeKeys(episode))
            {
                if (!lookup.ContainsKey(key))
                {
                    lookup.Add(key, episode);
                }
            }
        }

        return lookup;
    }

    private static IEnumerable<string> BuildMovieKeys(Movie movie)
    {
        var imdb = movie.GetProviderId(MetadataProvider.Imdb);
        if (!string.IsNullOrEmpty(imdb))
        {
            yield return $"imdb:{imdb}";
        }

        var tmdb = movie.GetProviderId(MetadataProvider.Tmdb);
        if (!string.IsNullOrEmpty(tmdb))
        {
            yield return $"tmdb:{tmdb}";
        }

        var slug = movie.GetProviderId("simkl");
        if (!string.IsNullOrEmpty(slug))
        {
            yield return $"simkl:{slug}";
        }

        var simklId = movie.GetProviderId("simklid");
        if (!string.IsNullOrEmpty(simklId))
        {
            yield return $"simklid:{simklId}";
        }

        if (!string.IsNullOrEmpty(movie.OriginalTitle) && movie.ProductionYear.HasValue)
        {
            yield return $"title:{movie.OriginalTitle}:{movie.ProductionYear.Value}";
        }

        if (!string.IsNullOrEmpty(movie.Name) && movie.ProductionYear.HasValue)
        {
            yield return $"title:{movie.Name}:{movie.ProductionYear.Value}";
        }
    }

    private static IEnumerable<string> BuildMovieKeys(SimklMovie movie)
    {
        if (movie.Ids != null)
        {
            if (!string.IsNullOrEmpty(movie.Ids.Imdb))
            {
                yield return $"imdb:{movie.Ids.Imdb}";
            }

            if (!string.IsNullOrEmpty(movie.Ids.Tmdb))
            {
                yield return $"tmdb:{movie.Ids.Tmdb}";
            }

            if (movie.Ids.Simkl.HasValue)
            {
                yield return $"simklid:{movie.Ids.Simkl.Value}";
            }

            if (!string.IsNullOrEmpty(movie.Ids.Slug))
            {
                yield return $"simkl:{movie.Ids.Slug}";
            }
        }

        if (!string.IsNullOrEmpty(movie.Title) && movie.Year.HasValue)
        {
            yield return $"title:{movie.Title}:{movie.Year.Value}";
        }
    }

    private static IEnumerable<string> BuildEpisodeKeys(Episode episode)
    {
        var season = episode.ParentIndexNumber;
        if (!season.HasValue || !episode.IndexNumber.HasValue)
        {
            yield break;
        }

        var showIds = episode.Series?.ProviderIds ?? new Dictionary<string, string>();
        foreach (var key in BuildEpisodeKeys(showIds, season.Value, episode.IndexNumber.Value))
        {
            yield return key;
        }
    }

    private static IEnumerable<string> BuildEpisodeKeys(IReadOnlyDictionary<string, string> ids, int season, int episode)
    {
        if (ids.TryGetValue(MetadataProvider.Imdb.ToString(), out var imdb) && !string.IsNullOrEmpty(imdb))
        {
            yield return $"imdb:{imdb}:S{season}E{episode}";
        }

        if (ids.TryGetValue(MetadataProvider.Tvdb.ToString(), out var tvdb) && !string.IsNullOrEmpty(tvdb))
        {
            yield return $"tvdb:{tvdb}:S{season}E{episode}";
        }

        if (ids.TryGetValue(MetadataProvider.Tmdb.ToString(), out var tmdb) && !string.IsNullOrEmpty(tmdb))
        {
            yield return $"tmdb:{tmdb}:S{season}E{episode}";
        }

        if (ids.TryGetValue("simkl", out var simkl) && !string.IsNullOrEmpty(simkl))
        {
            yield return $"simkl:{simkl}:S{season}E{episode}";
        }

        if (ids.TryGetValue("simklid", out var simklId) && !string.IsNullOrEmpty(simklId))
        {
            yield return $"simklid:{simklId}:S{season}E{episode}";
        }
    }

    private static IEnumerable<string> BuildEpisodeKeys(SimklIds ids, int season, int episode)
    {
        if (!string.IsNullOrEmpty(ids.Imdb))
        {
            yield return $"imdb:{ids.Imdb}:S{season}E{episode}";
        }

        if (!string.IsNullOrEmpty(ids.Tvdb))
        {
            yield return $"tvdb:{ids.Tvdb}:S{season}E{episode}";
        }

        if (!string.IsNullOrEmpty(ids.Tmdb))
        {
            yield return $"tmdb:{ids.Tmdb}:S{season}E{episode}";
        }

        if (!string.IsNullOrEmpty(ids.Slug))
        {
            yield return $"simkl:{ids.Slug}:S{season}E{episode}";
        }

        if (ids.Simkl.HasValue)
        {
            yield return $"simklid:{ids.Simkl.Value}:S{season}E{episode}";
        }
    }

    private void LogSummary(string username, ImportSummary summary, bool dryRun)
    {
        var mode = dryRun ? "Dry-run" : "Import";
        var message = $"{mode} completed for {username}: " +
            $"{summary.MoviesImported} movies, {summary.EpisodesImported} episodes updated. " +
            $"{summary.MoviesNotFound} movies and {summary.EpisodesNotFound} episodes not matched.";
        _logger.LogInformation("{Message}", message);
    }

    private sealed class ImportSummary
    {
        public int MoviesImported { get; set; }

        public int EpisodesImported { get; set; }

        public int MoviesNotFound { get; set; }

        public int EpisodesNotFound { get; set; }
    }

    private sealed class ActivitySnapshot
    {
        public ActivitySnapshot(UserConfig config)
        {
            All = config.LastActivitiesAll;
            Movies = config.LastActivitiesMovies;
            Tv = config.LastActivitiesShows;
            Anime = config.LastActivitiesAnime;
        }

        public DateTime? All { get; }

        public DateTime? Movies { get; }

        public DateTime? Tv { get; }

        public DateTime? Anime { get; }
    }
}
