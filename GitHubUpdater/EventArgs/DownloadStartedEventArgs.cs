using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubUpdater
{
    public class DownloadStartedEventArgs : EventArgs
    {
        /// <summary>
        /// The version that started downloading
        /// </summary>
        public Version Version { get; private set; }

        /// <summary>
        /// The constructor for the event
        /// </summary>
        /// <param name="version">The version that started downloading</param>
        public DownloadStartedEventArgs(Version version)
        {
            Version = version;
        }
    }
}
