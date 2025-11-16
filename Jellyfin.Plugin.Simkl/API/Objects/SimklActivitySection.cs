using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Simkl.API.Objects
{
    /// <summary>
    /// Watchlist activity timestamps for a specific media category.
    /// </summary>
    public class SimklActivitySection
    {
        /// <summary>
        /// Gets or sets latest change timestamp.
        /// </summary>
        [JsonPropertyName("all")]
        public DateTime? All { get; set; }

        /// <summary>
        /// Gets or sets last rating change.
        /// </summary>
        [JsonPropertyName("rated_at")]
        public DateTime? RatedAt { get; set; }

        /// <summary>
        /// Gets or sets last playback change.
        /// </summary>
        [JsonPropertyName("playback")]
        public DateTime? Playback { get; set; }

        /// <summary>
        /// Gets or sets last plan-to-watch update.
        /// </summary>
        [JsonPropertyName("plantowatch")]
        public DateTime? PlanToWatch { get; set; }

        /// <summary>
        /// Gets or sets last watching update.
        /// </summary>
        [JsonPropertyName("watching")]
        public DateTime? Watching { get; set; }

        /// <summary>
        /// Gets or sets last completed update.
        /// </summary>
        [JsonPropertyName("completed")]
        public DateTime? Completed { get; set; }

        /// <summary>
        /// Gets or sets last hold update.
        /// </summary>
        [JsonPropertyName("hold")]
        public DateTime? Hold { get; set; }

        /// <summary>
        /// Gets or sets last dropped update.
        /// </summary>
        [JsonPropertyName("dropped")]
        public DateTime? Dropped { get; set; }

        /// <summary>
        /// Gets or sets last removed-from-list update.
        /// </summary>
        [JsonPropertyName("removed_from_list")]
        public DateTime? RemovedFromList { get; set; }
    }
}
