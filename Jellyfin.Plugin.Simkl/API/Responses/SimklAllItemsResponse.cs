using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Jellyfin.Plugin.Simkl.API.Objects;

namespace Jellyfin.Plugin.Simkl.API.Responses
{
    /// <summary>
    /// Represents the payload returned by /sync/all-items.
    /// </summary>
    public class SimklAllItemsResponse
    {
        /// <summary>
        /// Gets or sets completed movie entries.
        /// </summary>
        [JsonPropertyName("movies")]
        public IReadOnlyList<SimklAllItemsMovie> Movies { get; set; } = Array.Empty<SimklAllItemsMovie>();

        /// <summary>
        /// Gets or sets show entries.
        /// </summary>
        [JsonPropertyName("shows")]
        public IReadOnlyList<SimklAllItemsShow> Shows { get; set; } = Array.Empty<SimklAllItemsShow>();

        /// <summary>
        /// Gets or sets anime entries (same schema as shows).
        /// </summary>
        [JsonPropertyName("anime")]
        public IReadOnlyList<SimklAllItemsShow> Anime { get; set; } = Array.Empty<SimklAllItemsShow>();
    }
}