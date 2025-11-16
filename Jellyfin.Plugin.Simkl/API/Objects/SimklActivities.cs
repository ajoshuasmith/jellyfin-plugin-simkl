using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Simkl.API.Objects
{
    /// <summary>
    /// Represents the payload returned by /sync/activities.
    /// </summary>
    public class SimklActivities
    {
        /// <summary>
        /// Gets or sets the last update timestamp across any watchlist.
        /// </summary>
        [JsonPropertyName("all")]
        public DateTime? All { get; set; }

        /// <summary>
        /// Gets or sets settings activity metadata.
        /// </summary>
        [JsonPropertyName("settings")]
        public SimklSettingsActivity? Settings { get; set; }

        /// <summary>
        /// Gets or sets tv show activity metadata.
        /// </summary>
        [JsonPropertyName("tv_shows")]
        public SimklActivitySection? TvShows { get; set; }

        /// <summary>
        /// Gets or sets anime activity metadata.
        /// </summary>
        [JsonPropertyName("anime")]
        public SimklActivitySection? Anime { get; set; }

        /// <summary>
        /// Gets or sets movie activity metadata.
        /// </summary>
        [JsonPropertyName("movies")]
        public SimklActivitySection? Movies { get; set; }
    }
}