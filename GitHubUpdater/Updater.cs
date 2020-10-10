using GitHubUpdater.Properties;
using Ionic.Zip;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace GitHubUpdater
{
    public class Updater : IDisposable
    {
        /// <summary>
        /// Fires if an update is available
        /// </summary>
        public event EventHandler<VersionEventArgs> UpdateAvailable;
        /// <summary>
        /// Fires when an update starts downloading
        /// </summary>
        public event EventHandler<DownloadStartedEventArgs> DownloadingStarted;
        /// <summary>
        /// Fires when an update has progressed
        /// </summary>
        public event EventHandler<DownloadProgressEventArgs> DownloadingProgressed;
        /// <summary>
        /// Fires when an update has completed downloading
        /// </summary>
        public event EventHandler<VersionEventArgs> DownloadingCompleted;
        /// <summary>
        /// Fires if downloading an update failed
        /// </summary>
        public event EventHandler<ExceptionEventArgs<Exception>> DownloadingFailed;
        /// <summary>
        /// Fires when an update started installing
        /// </summary>
        public event EventHandler<VersionEventArgs> InstallationStarted;
        /// <summary>
        /// Fires if an update failed installing
        /// </summary>
        public event EventHandler<ExceptionEventArgs<Exception>> InstallationFailed;

        private string gitHubUsername;
        private string gitHubRepositoryName;
        private readonly WebClient webClient;
        private readonly GitHubClient gitHubClient;
        private readonly string downloadPath;
        private string downloadFilePath;
        private readonly string updatePath;
        private readonly string originalFilePath;
        private readonly string changelogFilePath;
        private readonly string versionFilePath;
        private readonly string batFilePath;
        private Release latestRelease;
        private readonly Version currentVersion;
        private Version latestVersion;
        private string changelog;
        private DateTime updateStartTime;

        /// <summary>
        /// The GitHub repository name
        /// </summary>
        public string GitHubRepositoryName
        {
            get { return gitHubRepositoryName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new NullReferenceException("The repository name can't be null or whitespace");

                gitHubRepositoryName = value;
            }
        }

        /// <summary>
        /// The GitHub username
        /// </summary>
        public string GitHubUsername
        {
            get { return gitHubUsername; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new NullReferenceException("The github username can't be null or whitespace");

                gitHubUsername = value;
            }
        }

        /// <summary>
        /// The current state of the updater
        /// </summary>
        public UpdaterState State { get; private set; }

        /// <summary>
        /// Initializes a new instance of the updater
        /// </summary>
        /// <param name="gitHubUsername">The GitHub username</param>
        /// <param name="gitHubRepositoryName">The GitHub repository name</param>
        /// <param name="token">The GitHub personal access token</param>
        public Updater(string gitHubUsername, string gitHubRepositoryName, string token)
        {
            GitHubUsername = gitHubUsername;
            GitHubRepositoryName = gitHubRepositoryName;

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string mainProjectName = Assembly.GetEntryAssembly().GetName().Name;
            string appDataPath = $@"{appData}\{mainProjectName}";
            downloadPath = $@"{appDataPath}\Download";
            updatePath = $@"{appDataPath}\Update";
            batFilePath = $@"{appDataPath}\InstallUpdate.bat";

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);
            if (!Directory.Exists(updatePath))
                Directory.CreateDirectory(updatePath);

            if (File.Exists(batFilePath) && !string.Equals(File.ReadAllText(batFilePath), Resources.InstallUpdate) || !File.Exists(batFilePath))
                File.WriteAllText(batFilePath, Resources.InstallUpdate);

            try
            {
                gitHubClient = new GitHubClient(new ProductHeaderValue(mainProjectName))
                {
                    Credentials = new Credentials(token)
                };
                webClient = new WebClient();
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            }
            catch (Exception)
            {
                throw;
            }

            originalFilePath = Process.GetCurrentProcess().MainModule.FileName;
            changelogFilePath = $@"{downloadPath}\Update.changelog";
            versionFilePath = $@"{downloadPath}\Update.version";

            currentVersion = Version.ConvertToVersion(Assembly.GetEntryAssembly().GetName().Version.ToString(), true);
        }

        /// <summary>
        /// Initializes a new instance of the updater
        /// </summary>
        /// <param name="gitHubUsername">The GitHub username</param>
        /// <param name="gitHubRepositoryName">The GitHub repository name</param>
        /// <param name="token">The GitHub personal access token</param>
        /// <param name="currentVersion">The current version of the application</param>
        public Updater(string gitHubUsername, string gitHubRepositoryName, string token, string currentVersion) : this(gitHubUsername, gitHubRepositoryName, token)
        {
            this.currentVersion = Version.ConvertToVersion(currentVersion, true);
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string received = string.Format(CultureInfo.InvariantCulture, "{0:n0} kB", e.BytesReceived / 1000);
            string toReceive = string.Format(CultureInfo.InvariantCulture, "{0:n0} kB", e.TotalBytesToReceive / 1000);

            if (e.BytesReceived / 1000000 >= 1)
                received = string.Format("{0:.#0} MB", Math.Round((decimal)e.BytesReceived / 1000000, 2));
            if (e.TotalBytesToReceive / 1000000 >= 1)
                toReceive = string.Format("{0:.#0} MB", Math.Round((decimal)e.TotalBytesToReceive / 1000000, 2));

            TimeSpan timeSpent = DateTime.Now - updateStartTime;
            int secondsRemaining = (int)(timeSpent.TotalSeconds / e.ProgressPercentage * (100 - e.ProgressPercentage));
            TimeSpan timeLeft = new TimeSpan(0, 0, secondsRemaining);
            string timeLeftString = string.Empty;
            string timeSpentString = string.Empty;

            if (timeLeft.Hours > 0)
                timeLeftString += string.Format("{0} hours", timeLeft.Hours);
            if (timeLeft.Minutes > 0)
                timeLeftString += string.IsNullOrWhiteSpace(timeLeftString) ? string.Format("{0} min", timeLeft.Minutes) : string.Format(" {0} min", timeLeft.Minutes);
            if (timeLeft.Seconds >= 0)
                timeLeftString += string.IsNullOrWhiteSpace(timeLeftString) ? string.Format("{0} sec", timeLeft.Seconds) : string.Format(" {0} sec", timeLeft.Seconds);

            if (timeSpent.Hours > 0)
                timeSpentString = string.Format("{0} hours", timeSpent.Hours);
            if (timeSpent.Minutes > 0)
                timeSpentString += string.IsNullOrWhiteSpace(timeSpentString) ? string.Format("{0} min", timeSpent.Minutes) : string.Format(" {0} min", timeSpent.Minutes);
            if (timeSpent.Seconds >= 0)
                timeSpentString += string.IsNullOrWhiteSpace(timeSpentString) ? string.Format("{0} sec", timeSpent.Seconds) : string.Format(" {0} sec", timeSpent.Seconds);

            DownloadingProgressed?.Invoke(this, new DownloadProgressEventArgs(e.BytesReceived, e.TotalBytesToReceive, e.ProgressPercentage, timeLeftString, timeSpentString, received, toReceive));
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            State = UpdaterState.Idle;

            File.WriteAllText(changelogFilePath, changelog);
            File.WriteAllText(versionFilePath, latestRelease.TagName.Replace("v", ""));

            List<string> updateFiles = Directory.GetFiles(updatePath).ToList();
            updateFiles.ForEach(x => File.Delete(x));

            if (ZipFile.IsZipFile(downloadFilePath) && ZipFile.CheckZip(downloadFilePath))
            {
                using (ZipFile zip = new ZipFile(downloadFilePath))
                    zip.ExtractAll(updatePath, ExtractExistingFileAction.OverwriteSilently);

                if (File.Exists(downloadFilePath))
                    File.Delete(downloadFilePath);
            }
            else
            {
                string newFilePath = $@"{updatePath}\{Path.GetFileName(downloadFilePath)}";
                File.Move(downloadFilePath, newFilePath);
            }

            DownloadingCompleted?.Invoke(this, new VersionEventArgs(currentVersion, latestVersion, false, changelog));
        }

        /// <summary>
        /// Checks if an update is available
        /// </summary>
        /// <returns>The latest or current version</returns>
        public async Task<Version> CheckForUpdatesAsync()
        {
            State = UpdaterState.CheckingForUpdates;

            int updateFiles = Directory.GetFiles(downloadPath).Length;
            if (File.Exists(versionFilePath) && updateFiles > 0)
            {
                string versionTxt = await File.ReadAllTextAsync(versionFilePath);
                latestVersion = Version.ConvertToVersion(versionTxt);

                if (latestVersion > currentVersion)
                {
                    if (File.Exists(changelogFilePath))
                        UpdateAvailable?.Invoke(this, new VersionEventArgs(currentVersion, latestVersion, true, File.ReadAllText(changelogFilePath)));
                    else
                        UpdateAvailable?.Invoke(this, new VersionEventArgs(currentVersion, latestVersion, true));

                    State = UpdaterState.Idle;
                    return latestVersion;
                }
            }
            else
            {
                try
                {
                    var releases = await gitHubClient.Repository.Release.GetAll(GitHubUsername, GitHubRepositoryName);
                    Release release = releases.FirstOrDefault(x => Version.ConvertToVersion(x.TagName.Replace("v", "")) > currentVersion);

                    if (release is null)
                        return currentVersion;

                    latestRelease = release;
                    latestVersion = Version.ConvertToVersion(latestRelease.TagName.Replace("v", ""));
                    changelog = latestRelease.Body;
                    UpdateAvailable?.Invoke(this, new VersionEventArgs(currentVersion, latestVersion, false, latestRelease.Body));
                    State = UpdaterState.Idle;
                    return latestVersion;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            State = UpdaterState.Idle;
            return currentVersion;
        }

        /// <summary>
        /// Checks if an update is downloaded
        /// </summary>
        /// <returns>true if an update is downloaded, false otherwise</returns>
        public bool IsUpdateDownloaded()
        {
            int updateFiles = Directory.GetFiles(downloadPath).Length;
            if (File.Exists(versionFilePath) && updateFiles > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Begins to download an update if one is available
        /// </summary>
        public void DownloadUpdate()
        {
            if (latestRelease is null)
            {
                DownloadingFailed?.Invoke(this, new ExceptionEventArgs<Exception>(new FileNotFoundException("There isn't any update available"), "There isn't any update available"));
                return;
            }

            if (File.Exists(downloadPath))
                File.Delete(downloadPath);

            try
            {
                DownloadingStarted?.Invoke(this, new DownloadStartedEventArgs(latestVersion));
                State = UpdaterState.Downloading;

                string fileUrl = latestRelease.Assets[0].BrowserDownloadUrl;
                string filePath = $@"{downloadPath}\{Path.GetFileName(fileUrl)}";
                downloadFilePath = filePath;

                updateStartTime = DateTime.Now;
                webClient.DownloadFileAsync(new Uri(fileUrl), filePath);
            }
            catch (Exception e)
            {
                DownloadingFailed?.Invoke(this, new ExceptionEventArgs<Exception>(e, e.Message));
                return;
            }
        }

        /// <summary>
        /// Installs the downloaded update if it exists
        /// </summary>
        public void InstallUpdate()
        {
            int updateFiles = Directory.GetFiles(downloadPath).Length;
            if (updateFiles == 0)
            {
                InstallationFailed?.Invoke(this, new ExceptionEventArgs<Exception>(new FileNotFoundException("There isn't any downloaded update"), "There isn't any downloaded update"));
                return;
            }

            State = UpdaterState.Installing;
            InstallationStarted?.Invoke(this, new VersionEventArgs(currentVersion, latestVersion, false, changelog));

            try
            {
                Process process = new Process
                {
                    StartInfo =
                    {
                        FileName = batFilePath,
                        Arguments = $"{Process.GetCurrentProcess().Id} \"{updatePath}\" \"{Path.GetDirectoryName(originalFilePath)}\" \"{originalFilePath}\"",
                        CreateNoWindow = true
                    }
                };

                process.Start();
            }
            catch (Exception e)
            {
                InstallationFailed?.Invoke(this, new ExceptionEventArgs<Exception>(e, e.Message));
                return;
            }

            State = UpdaterState.Idle;
        }

        /// <summary>
        /// Deletes the update files if they exist
        /// </summary>
        public void DeleteUpdateFiles()
        {
            if (File.Exists(changelogFilePath))
                File.Delete(changelogFilePath);
            if (File.Exists(versionFilePath))
                File.Delete(versionFilePath);

            List<string> updateFiles = Directory.GetFiles(updatePath).ToList();
            updateFiles.ForEach(x => File.Delete(x));
        }

        /// <summary>
        /// Disposes the updater
        /// </summary>
        public void Dispose()
        {
            webClient.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
