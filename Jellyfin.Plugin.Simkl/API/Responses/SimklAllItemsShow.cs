using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Jellyfin.Plugin.Simkl.API.Objects;

namespace Jellyfin.Plugin.Simkl.API.Responses
{
    /// <summary>
    /// Watchlist entry for shows or anime.
    /// </summary>
    public sealed class SimklAllItemsShow : SimklAllItemsEntry
    {
        /// <summary>
        /// Gets or sets the show payload.
        /// </summary>
        [JsonPropertyName("show")]
        public SimklShow? Show { get; set; }

        /// <summary>
        /// Gets or sets the anime subtype (if applicable).
        /// </summary>
        [JsonPropertyName("anime_type")]
        public string? AnimeType { get; set; }

        /// <summary>
        /// Gets or sets watched seasons and episodes.
        /// </summary>
        [JsonPropertyName("seasons")]
        public IReadOnlyList<Season> Seasons { get; set; } = Array.Empty<Season>();
    }
}
