using System;

namespace Jellyfin.Plugin.Simkl.Configuration
{
    /// <summary>
    /// User config.
    /// </summary>
    public class UserConfig
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfig"/> class.
        /// </summary>
        public UserConfig()
        {
            ScrobbleMovies = true;
            ScrobbleShows = true;
            ScrobblePercentage = 70;
            ScrobbleNowWatchingPercentage = 5;
            MinLength = 5;
            UserToken = string.Empty; // Todo: check if token is still valid
            ScrobbleTimeout = 30;
            SyncFromSimkl = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether scrobble movies.
        /// </summary>
        public bool ScrobbleMovies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether scrobble shows.
        /// </summary>
        public bool ScrobbleShows { get; set; }

        /// <summary>
        /// Gets or sets scrobble percentage.
        /// </summary>
        public int ScrobblePercentage { get; set; }

        /// <summary>
        /// Gets or sets scrobble now watching percentage.
        /// </summary>
        public int ScrobbleNowWatchingPercentage { get; set; }

        /// <summary>
        /// Gets or sets min length.
        /// </summary>
        /// <remarks>
        /// Minimum length for scrobbling (in minutes).
        /// </remarks>
        public int MinLength { get; set; }

        /// <summary>
        /// Gets or sets user token.
        /// </summary>
        public string UserToken { get; set; } // Is the user logged in

        /// <summary>
        /// Gets or sets scrobble timeout.
        /// </summary>
        /// <remarks>
        /// Time between scrobbling tries.
        /// </remarks>
        public int ScrobbleTimeout { get; set; }

        /// <summary>
        /// Gets or sets user id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether watched-state import is enabled.
        /// </summary>
        public bool SyncFromSimkl { get; set; }

        /// <summary>
        /// Gets or sets the last /sync/activities timestamp.
        /// </summary>
        public DateTime? LastActivitiesAll { get; set; }

        /// <summary>
        /// Gets or sets the last movie activity timestamp.
        /// </summary>
        public DateTime? LastActivitiesMovies { get; set; }

        /// <summary>
        /// Gets or sets the last show activity timestamp.
        /// </summary>
        public DateTime? LastActivitiesShows { get; set; }

        /// <summary>
        /// Gets or sets the last anime activity timestamp.
        /// </summary>
        public DateTime? LastActivitiesAnime { get; set; }

        /// <summary>
        /// Gets or sets the last time an import completed.
        /// </summary>
        public DateTime? LastImportUtc { get; set; }
    }
}