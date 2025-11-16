<h1 align="center">Jellyfin SIMKL Plugin</h1>
<h3 align="center">Part of the <a href="https://jellyfin.org/">Jellyfin Project</a></h3>

###

## Current features
- Multi-user support
- Auto scrobble Movies and TV Shows at given percentage to Simkl
- Easy login using pin
- If scrobbling fails, search it using filename using Simkl's API and then scrobble it
- Manually sync your entire watched history with Simkl via a button in the plugin settings
- Restore watched status from Simkl back into Jellyfin, including a dry-run mode to review matches before applying changes

## Future features
- Batch syncing for very big collections of movies, series and shows
- Ratings, lists, and playback progress import/export parity with the Trakt plugin

## Known gaps vs. Trakt plugin
- Ratings sync is one-way (Jellyfin → Simkl) and lacks import/export parity
- Simkl collections/watchlists do not yet map to Jellyfin libraries
- Playback progress (pause/resume) isn't imported from Simkl's playback endpoint
- There are no per-library filters or dry-run previews for outbound sync jobs
- Error reporting/log visibility is limited to server logs; no UI history yet