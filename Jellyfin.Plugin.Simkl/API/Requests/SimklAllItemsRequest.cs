namespace Jellyfin.Plugin.Simkl.API.Requests
{
    /// <summary>
    /// Encapsulates query string options for /sync/all-items.
    /// </summary>
    public class SimklAllItemsRequest
    {
        /// <summary>
        /// Gets or sets the media type (movies, shows, anime).
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the watchlist status filter.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the ISO8601 timestamp used for incremental sync.
        /// </summary>
        public string? DateFrom { get; set; }

        /// <summary>
        /// Gets or sets the extended mode.
        /// </summary>
        public string? Extended { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether watched_at timestamps should be included for episodes.
        /// </summary>
        public bool IncludeEpisodeWatchedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether memos should be included.
        /// </summary>
        public bool IncludeMemos { get; set; }
    }
}
