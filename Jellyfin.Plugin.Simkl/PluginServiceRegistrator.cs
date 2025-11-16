using Jellyfin.Plugin.Simkl.API;
using Jellyfin.Plugin.Simkl.ScheduledTasks;
using Jellyfin.Plugin.Simkl.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.Simkl
{
    /// <inheritdoc />
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc />
        public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
        {
            serviceCollection.AddSingleton<SimklApi>();
            serviceCollection.AddSingleton<SimklImportService>();
            serviceCollection.AddHostedService<PlaybackScrobbler>();
            serviceCollection.AddSingleton<IScheduledTask, SimklImportWatchedTask>();
        }
    }
}