using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.Simkl.Configuration;
using Jellyfin.Plugin.Simkl.Services;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.Simkl.ScheduledTasks;

/// <summary>
/// Scheduled task that imports watched state from Simkl into Jellyfin.
/// </summary>
    public class SimklImportWatchedTask : IScheduledTask
{
    private readonly ILogger<SimklImportWatchedTask> _logger;
    private readonly SimklImportService _importService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimklImportWatchedTask"/> class.
        /// </summary>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="importService">Import service dependency.</param>
    public SimklImportWatchedTask(ILoggerFactory loggerFactory, SimklImportService importService)
    {
        _logger = loggerFactory.CreateLogger<SimklImportWatchedTask>();
        _importService = importService;
    }

        /// <inheritdoc />
    public string Key => "SimklImportWatchedTask";

        /// <inheritdoc />
    public string Name => "Import watched states from Simkl";

        /// <inheritdoc />
    public string Description => "Imports watched/unwatched states from Simkl for any users that enabled Sync From Simkl.";

        /// <inheritdoc />
    public string Category => "Simkl";

        /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        // Default to disabled so admins can opt-in via the scheduled task UI.
        return Enumerable.Empty<TaskTriggerInfo>();
    }

        /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running Simkl watched import scheduled task.");
        await _importService.ImportEnabledUsersAsync(progress, cancellationToken).ConfigureAwait(false);
    }
}
