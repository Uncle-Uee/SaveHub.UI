using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Configuration;
using SaveHub.Core.Models;
using SaveHub.Bitbucket;
using SaveHub.GitHub;
using SaveHub.GitLab;
using SaveHub.GoogleDrive;
using SaveHub.Hosting;
using SaveHub.Supabase;
using SaveHub.WinForms.Common;

namespace SaveHub.WinForms;

/// <summary>
/// Non-UI application logic for <see cref="MainForm"/>: client creation, backend
/// operations, save detection, and settings persistence. The form is responsible
/// only for presentation and delegates all of this work here (Single Responsibility).
/// </summary>
internal sealed class MainFormController
{
    // ---------------------------------------------------------------- Providers / config

    public IReadOnlyList<ProviderDescriptor> Providers => SaveHubHost.Providers;

    public string ConfigPath => AppServices.Store.Path;

    /// <summary>Whether a Google Drive sign-in session is currently active in memory.</summary>
    public bool GoogleHasActiveSession => GoogleDriveSession.HasActiveSession;

    /// <summary>Builds a client for the active provider, or returns null with a reason.</summary>
    public SaveHubClient? TryCreateClient(out string error)
    {
        return AppServices.TryCreateClient(out error);
    }

    public bool IsNintendo(string device)
    {
        return KnownPlatforms.IsNintendo(device);
    }

    // ---------------------------------------------------------------- Cover art cache

    /// <summary>Directory holding the shared SaveHub data (config + cover cache).</summary>
    public string DataFolderPath => Path.GetDirectoryName(AppServices.Store.Path)!;

    /// <summary>Directory holding cached cover art.</summary>
    public string CoverCachePath => AppServices.CoverCache.RootDirectory;

    /// <summary>Reads a cached cover for a platform/serial, or null when none is cached.</summary>
    public byte[]? TryGetCachedCover(string platform, string serial)
    {
        return AppServices.CoverCache.TryRead(platform, serial);
    }

    /// <summary>Caches a user-supplied cover so it is reused for later uploads and previews.</summary>
    public void CacheUserCover(string platform, string serial, string iconPath)
    {
        if (string.IsNullOrWhiteSpace(serial) || string.IsNullOrWhiteSpace(iconPath) || !File.Exists(iconPath))
        {
            return;
        }
        try
        {
            byte[] bytes = File.ReadAllBytes(iconPath);
            string extension = CoverArtSource.Resolve(platform, serial) is { } source
                ? source.Extension
                : Path.GetExtension(iconPath);
            AppServices.CoverCache.Store(platform, serial, bytes, extension);
        }
        catch (IOException)
        {
            // Best-effort.
        }
    }


    // ---------------------------------------------------------------- Backend operations

    public Task<IReadOnlyList<string>> ListPlatformsAsync(SaveHubClient client)
    {
        return client.ListPlatformsAsync();
    }

    public Task<IReadOnlyList<string>> ListGamesAsync(SaveHubClient client, string system)
    {
        return client.ListGamesAsync(system);
    }

    public Task<IReadOnlyList<SaveEntry>> ListSavesAsync(SaveHubClient client, string system, string game)
    {
        return client.ListSavesAsync(system, game);
    }

    public Task<IReadOnlyDictionary<string, string>> GetGameNamesAsync(SaveHubClient client, string system)
    {
        return client.GetGameNamesAsync(system);
    }

    /// <summary>
    /// Game names for a platform, read from the on-disk library cache when present; otherwise fetched
    /// once from the backend and written to the cache. Avoids repeated per-platform network reads.
    /// </summary>
    public async Task<IReadOnlyDictionary<string, string>> GetPlatformNamesAsync(SaveHubClient client, string platform)
    {
        LibraryIndex cache = LoadLocalLibrary();
        IReadOnlyDictionary<string, string> cached = cache.ForPlatform(platform);
        if (cached.Count > 0)
        {
            return cached;
        }

        IReadOnlyDictionary<string, string> fresh = await client.GetGameNamesAsync(platform);
        foreach (KeyValuePair<string, string> entry in fresh)
        {
            cache.Set(platform, entry.Key, entry.Value);
        }
        SaveLocalLibrary(cache);
        return fresh;
    }

    /// <summary>Updates a single game's name in the on-disk library cache (best-effort).</summary>
    public void CacheGameName(string platform, string gameId, string? name)
    {
        LibraryIndex cache = LoadLocalLibrary();
        cache.Set(platform, gameId, string.IsNullOrWhiteSpace(name) ? gameId : name.Trim());
        SaveLocalLibrary(cache);
    }

    public Task<LibraryIndex> GetLibraryIndexAsync(SaveHubClient client)
    {
        return client.GetLibraryIndexAsync();
    }

    public Task<LibraryIndex> RebuildLibraryIndexAsync(SaveHubClient client)
    {
        return client.RebuildLibraryIndexAsync();
    }

    public async Task SetGameNameAsync(SaveHubClient client, string platform, string gameId, string name)
    {
        await client.SetGameNameAsync(platform, gameId, name);
        CacheGameName(platform, gameId, name);
    }

    public Task<byte[]?> GetGameIconAsync(SaveHubClient client, string system, string game)
    {
        return client.GetGameIconAsync(system, game);
    }

    public Task<bool> DownloadArchiveToFileAsync(SaveHubClient client, string system, string game, string archive, string destination)
    {
        return client.DownloadArchiveToFileAsync(system, game, archive, destination);
    }

    public Task<bool> DeleteSaveAsync(SaveHubClient client, string platform, string gameId, string archive)
    {
        return client.DeleteSaveAsync(platform, gameId, archive);
    }

    public Task<ConnectionTestResult> TestConnectionAsync(SaveHubClient client)
    {
        return client.TestConnectionAsync();
    }

    public Task<SaveUploadResult> UploadAsync(SaveHubClient client, SaveUploadRequest request, UploadOptions options)
    {
        return client.UploadAsync(request, options);
    }

    /// <summary>Adds or updates rows in the platform's bulk memory-card index.</summary>
    public Task UpdateMemoryCardIndexAsync(SaveHubClient client, string platform, IReadOnlyList<MemoryCardIndexEntry> entries)
    {
        return client.UpdateMemoryCardIndexAsync(platform, entries);
    }

    // ---------------------------------------------------------------- Save detection

    public string? DetectTitleId(string device, SaveType saveType, IReadOnlyList<string> files)
    {
        return GameIdResolver.DetectTitleId(device, saveType, files);
    }

    public string? DetectSaveName(string device, IReadOnlyList<string> files)
    {
        return SaveNameExtractor.Read(device, files);
    }

    public string? DetectMemoryCardPlatform(string file)
    {
        return MemoryCardReader.DetectPlatformFromFile(file);
    }

    public string? DetectFolderPlatform(IReadOnlyList<string> files)
    {
        return PlaystationDetector.DetectFolderPlatform(files);
    }

    public GameIdResolution Resolve(string device, SaveType saveType, IReadOnlyList<string> files, string? titleId, string? gameName)
    {
        return GameIdResolver.Resolve(device, saveType, files, titleId, gameName);
    }

    /// <summary>Looks up a stored game name for an existing game; best-effort (null on failure).</summary>
    public async Task<string?> LookupExistingGameNameAsync(SaveHubClient client, string device, string titleId)
    {
        try
        {
            IReadOnlyDictionary<string, string> names = await client.GetGameNamesAsync(device);
            if (names.TryGetValue(titleId, out string? existingName) && !string.Equals(existingName, titleId, StringComparison.OrdinalIgnoreCase))
            {
                return existingName;
            }
        }
        catch
        {
            // Best-effort: ignore lookup failures.
        }
        return null;
    }

    // ---------------------------------------------------------------- Settings

    public int ActiveProviderIndex(string activeProvider)
    {
        int index = SaveHubHost.Providers.ToList().FindIndex(p => p.Name == activeProvider);
        return index >= 0 ? index : 0;
    }

    public string ProviderCodeAt(int index)
    {
        return index >= 0 ? SaveHubHost.Providers[index].Name : GitHubProviderFactory.ProviderName;
    }

    public SettingsSnapshot LoadSettings()
    {
        SaveHubConfig config = AppServices.LoadConfig();
        GitHubProviderSettings gh = GitHubProviderFactory.ReadSettings(config) ?? new GitHubProviderSettings();
        GitLabProviderSettings gl = GitLabProviderFactory.ReadSettings(config) ?? new GitLabProviderSettings();
        BitbucketProviderSettings bb = BitbucketProviderFactory.ReadSettings(config) ?? new BitbucketProviderSettings();
        SupabaseProviderSettings sb = SupabaseProviderFactory.ReadSettings(config) ?? new SupabaseProviderSettings();
        GoogleDriveProviderSettings gd = GoogleDriveProviderFactory.ReadSettings(config) ?? new GoogleDriveProviderSettings();

        // The repository name is intentionally left blank for new users; the UI shows the default
        // only as a placeholder hint. The Google folder default is functional and kept.
        if (string.IsNullOrWhiteSpace(gd.RootFolderName))
        {
            gd.RootFolderName = CommonSettings.DefaultGoogleFolder;
        }

        return new SettingsSnapshot(gh, gl, bb, sb, gd, ActiveProviderIndex(config.ActiveProvider));
    }

    public void SaveGitHubSettings(string owner, string repository, string branch, bool autoMerge, string? token)
    {
        SaveHubConfig config = AppServices.LoadConfig();
        GitHubProviderSettings gh = GitHubProviderFactory.ReadSettings(config) ?? new GitHubProviderSettings();

        gh.Owner = owner;
        gh.Repository = repository;
        gh.Branch = branch;
        gh.AutoMerge = autoMerge;

        if (!string.IsNullOrEmpty(token))
        {
            gh.Token = token;
        }

        GitHubProviderFactory.WriteSettings(config, gh);
        AppServices.SaveConfig(config);
    }

    public void SaveGitLabSettings(string baseUrl, string owner, string repository, string branch, bool autoMerge, string? token)
    {
        SaveHubConfig config = AppServices.LoadConfig();
        GitLabProviderSettings gl = GitLabProviderFactory.ReadSettings(config) ?? new GitLabProviderSettings();

        gl.BaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? "https://gitlab.com" : baseUrl;
        gl.Owner = owner;
        gl.Repository = repository;
        gl.Branch = branch;
        gl.AutoMerge = autoMerge;

        if (!string.IsNullOrEmpty(token))
        {
            gl.Token = token;
        }

        GitLabProviderFactory.WriteSettings(config, gl);
        AppServices.SaveConfig(config);
    }

    public void SaveBitbucketSettings(string workspace, string repository, string branch, string username, bool autoMerge, string? appPassword)
    {
        SaveHubConfig config = AppServices.LoadConfig();
        BitbucketProviderSettings bb = BitbucketProviderFactory.ReadSettings(config) ?? new BitbucketProviderSettings();

        bb.Workspace = workspace;
        bb.Repository = repository;
        bb.Branch = branch;
        bb.Username = username;
        bb.AutoMerge = autoMerge;

        if (!string.IsNullOrEmpty(appPassword))
        {
            bb.AppPassword = appPassword;
        }

        BitbucketProviderFactory.WriteSettings(config, bb);
        AppServices.SaveConfig(config);
    }

    public void SaveSupabaseSettings(string url, string bucket, bool isOwner, string? apiKey)
    {
        SaveHubConfig config = AppServices.LoadConfig();
        SupabaseProviderSettings sb = SupabaseProviderFactory.ReadSettings(config) ?? new SupabaseProviderSettings();
        sb.Url = url;
        sb.Bucket = bucket;
        sb.IsOwner = isOwner;

        if (!string.IsNullOrEmpty(apiKey))
        {
            sb.ApiKey = apiKey;
        }

        SupabaseProviderFactory.WriteSettings(config, sb);
        AppServices.SaveConfig(config);
    }

    public void SaveGoogleSettings(string rootFolderName, string clientId, bool isOwner, string? clientSecret)
    {
        SaveHubConfig config = AppServices.LoadConfig();
        GoogleDriveProviderSettings gd = GoogleDriveProviderFactory.ReadSettings(config) ?? new GoogleDriveProviderSettings();
        gd.RootFolderName = string.IsNullOrWhiteSpace(rootFolderName) ? CommonSettings.DefaultGoogleFolder : rootFolderName;
        gd.ClientId = clientId;
        gd.IsOwner = isOwner;

        if (!string.IsNullOrEmpty(clientSecret))
        {
            gd.ClientSecret = clientSecret;
        }

        GoogleDriveProviderFactory.WriteSettings(config, gd);
        AppServices.SaveConfig(config);
    }

    public Task<GoogleDriveSession> SignInGoogleAsync()
    {
        GoogleDriveProviderSettings settings = GoogleDriveProviderFactory.ReadSettings(AppServices.LoadConfig()) ?? new GoogleDriveProviderSettings();
        return GoogleDriveAuthenticator.SignInAsync(settings, new GoogleDriveAuthenticator.MemoryTokenStore());
    }

    // ---------------------------------------------------------------- Pure helpers

    public static string Label(SaveType saveType)
    {
        return SaveNaming.Label(saveType);
    }

    /// <summary>Formats a game as "Name (id)" when a distinct name is known, else just the id.</summary>
    public static string GameDisplay(string gameId, IReadOnlyDictionary<string, string> names)
    {
        return names.TryGetValue(gameId, out string? name) && !string.Equals(name, gameId, StringComparison.OrdinalIgnoreCase)
            ? $"{name} ({gameId})"
            : gameId;
    }

    private static string LocalLibraryPath => Path.Combine(
        Path.GetDirectoryName(AppServices.Store.Path) ?? ".", CommonSettings.LibraryCacheFileName);

    /// <summary>Loads the locally cached library index (empty when absent or unreadable).</summary>
    public LibraryIndex LoadLocalLibrary()
    {
        try
        {
            return File.Exists(LocalLibraryPath)
                ? LibraryIndex.Deserialize(File.ReadAllBytes(LocalLibraryPath))
                : new LibraryIndex();
        }
        catch
        {
            return new LibraryIndex();
        }
    }

    /// <summary>Writes the library index to the local cache (best-effort).</summary>
    public void SaveLocalLibrary(LibraryIndex index)
    {
        try
        {
            string? dir = Path.GetDirectoryName(LocalLibraryPath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(LocalLibraryPath, index.Serialize());
        }
        catch
        {
            // Best-effort local cache.
        }
    }

    public static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double value = bytes;
        int unit = 0;

        while (value >= 1024 && unit < units.Length - 1)
        {
            value /= 1024;
            unit++;
        }

        return $"{value:0.#} {units[unit]}";
    }
}

/// <summary>Immutable view of the persisted provider settings used to populate the Settings tab.</summary>
internal sealed record SettingsSnapshot(
    GitHubProviderSettings GitHub,
    GitLabProviderSettings GitLab,
    BitbucketProviderSettings Bitbucket,
    SupabaseProviderSettings Supabase,
    GoogleDriveProviderSettings Google,
    int ActiveProviderIndex);
