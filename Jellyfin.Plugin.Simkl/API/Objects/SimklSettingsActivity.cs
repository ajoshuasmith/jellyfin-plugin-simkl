using System;
using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.Simkl.API.Objects
{
    /// <summary>
    /// Settings-level activity timestamps.
    /// </summary>
    public class SimklSettingsActivity
    {
        /// <summary>
        /// Gets or sets the settings timestamp.
        /// </summary>
        [JsonPropertyName("all")]
        public DateTime? All { get; set; }
    }
}
