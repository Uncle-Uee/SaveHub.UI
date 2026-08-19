namespace SaveHub.WinForms.Common;

/// <summary>Shared default values and file names used across the app.</summary>
internal static class CommonSettings
{
    /// <summary>Default GitHub repository name suggested in Settings.</summary>
    public static readonly string DefaultGitHubRepository = "Emu-Saves-Backup";

    /// <summary>Default Google Drive root folder name suggested in Settings.</summary>
    public static readonly string DefaultGoogleFolder = "EmuSavesBackup";

    /// <summary>File name of the on-disk library cache (under the per-user config folder).</summary>
    public static readonly string LibraryCacheFileName = "library-cache.json";
}
