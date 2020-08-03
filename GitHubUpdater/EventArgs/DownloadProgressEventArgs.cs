using System;
using System.Collections.Generic;
using System.Text;

namespace GitHubUpdater
{
    public class DownloadProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The current amount of received bytes
        /// </summary>
        public long BytesReceived { get; private set; }
        /// <summary>
        /// The total amount of bytes to receive
        /// </summary>
        public long TotalBytesToReceive { get; private set; }
        /// <summary>
        /// The current downloading progress in percent
        /// </summary>
        public int ProgressPercent { get; private set; }
        /// <summary>
        /// The estimated time left until completed
        /// </summary>
        public string TimeLeft { get; private set; }
        /// <summary>
        /// The time spent downloading
        /// </summary>
        public string TimeSpent { get; private set; }
        /// <summary>
        /// The current amount downloaded
        /// </summary>
        public string Downloaded { get; private set; }
        /// <summary>
        /// The current amount to download
        /// </summary>
        public string ToDownload { get; private set; }

        /// <summary>
        /// The constructor for the event
        /// </summary>
        /// <param name="bytesReceived">The total amount of bytes received</param>
        /// <param name="totalBytesToReceive">The total amount of bytes to receive</param>
        /// <param name="progressPercent">The download progress percent</param>
        /// <param name="timeLeft">The estimated time left until completed</param>
        /// <param name="timeSpent">The time spent downloading</param>
        /// <param name="downloaded">The current amount downloaded</param>
        /// <param name="toDownload">The current amount to download</param>
        public DownloadProgressEventArgs(long bytesReceived, long totalBytesToReceive, int progressPercent, string timeLeft, string timeSpent, string downloaded, string toDownload)
        {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
            ProgressPercent = progressPercent;
            TimeLeft = timeLeft;
            TimeSpent = timeSpent;
            Downloaded = downloaded;
            ToDownload = toDownload;
        }
    }
}
