namespace GitHubUpdater
{
    /// <summary>
    /// The available updater states
    /// </summary>
    public enum UpdaterState
    {
        Idle,
        CheckingForUpdates,
        Downloading,
        Installing,
        RollingBack
    }
}
