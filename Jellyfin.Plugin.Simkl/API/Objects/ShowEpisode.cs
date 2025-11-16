using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Simkl.API.Objects
{
    /// <summary>
    /// Show episode.
    /// </summary>
    public class ShowEpisode
    {
        /// <summary>
        /// Gets or sets episode number.
        /// </summary>
        [JsonPropertyName("number")]
        public int? Number { get; set; }

        /// <summary>
        /// Gets or sets last watched timestamp.
        /// </summary>
        [JsonPropertyName("watched_at")]
        public DateTime? WatchedAt { get; set; }
    }
}