using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Simkl.API.Responses
{
    /// <summary>
    /// Base class for watchlist entries.
    /// </summary>
    public abstract class SimklAllItemsEntry
    {
        /// <summary>
        /// Gets or sets the watchlist status.
        /// </summary>
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the last watched date.
        /// </summary>
        [JsonPropertyName("last_watched_at")]
        public DateTime? LastWatchedAt { get; set; }

        /// <summary>
        /// Gets or sets when the item was added to the list.
        /// </summary>
        [JsonPropertyName("added_to_watchlist_at")]
        public DateTime? AddedToWatchlistAt { get; set; }
    }
}
