using System;

namespace GitHubUpdater
{
    public class VersionEventArgs : EventArgs
    {
        /// <summary>
        /// The current application version
        /// </summary>
        public Version CurrentVersion { get; private set; }
        /// <summary>
        /// The latest version on GitHub
        /// </summary>
        public Version LatestVersion { get; private set; }
        /// <summary>
        /// If the latest version is currently downloaded
        /// </summary>
        public bool UpdateDownloaded { get; private set; }
        /// <summary>
        /// The changelog from GitHub
        /// </summary>
        public string Changelog { get; private set; }

        /// <summary>
        /// The constructor for the event
        /// </summary>
        /// <param name="currentVersion">The current application version</param>
        /// <param name="latestVersion">The latest version on GitHub</param>
        public VersionEventArgs(Version currentVersion, Version latestVersion)
        {
            CurrentVersion = currentVersion;
            LatestVersion = latestVersion;
        }

        /// <summary>
        /// The constructor for the event
        /// </summary>
        /// <param name="currentVersion">The current application version</param>
        /// <param name="latestVersion">The latest version on GitHub</param>
        /// <param name="updateDownloaded">If the latest version is current downloaded</param>
        public VersionEventArgs(Version currentVersion, Version latestVersion, bool updateDownloaded) : this(currentVersion, latestVersion)
        {
            UpdateDownloaded = updateDownloaded;
        }

        /// <summary>
        /// The constructor for the event
        /// </summary>
        /// <param name="currentVersion">The current application version</param>
        /// <param name="latestVersion">The latest version on GitHub</param>
        /// <param name="updateDownloaded">If the latest version is current downloaded</param>
        /// <param name="changelog">The changelog from GitHub</param>
        public VersionEventArgs(Version currentVersion, Version latestVersion, bool updateDownloaded, string changelog) : this(currentVersion, latestVersion, updateDownloaded)
        {
            Changelog = changelog;
        }
    }
}
