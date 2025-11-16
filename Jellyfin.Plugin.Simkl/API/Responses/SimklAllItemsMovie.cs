using System.Text.Json.Serialization;
using Jellyfin.Plugin.Simkl.API.Objects;

namespace Jellyfin.Plugin.Simkl.API.Responses
{
    /// <summary>
    /// Watchlist entry for movies.
    /// </summary>
    public sealed class SimklAllItemsMovie : SimklAllItemsEntry
    {
        /// <summary>
        /// Gets or sets the Simkl movie payload.
        /// </summary>
        [JsonPropertyName("movie")]
        public SimklMovie? Movie { get; set; }
    }
}
