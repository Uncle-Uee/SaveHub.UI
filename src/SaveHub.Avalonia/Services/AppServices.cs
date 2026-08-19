using SaveHub.Core;
using SaveHub.Core.Configuration;
using SaveHub.Hosting;

namespace SaveHub.Avalonia.Services;

/// <summary>Loads configuration and builds a provider-agnostic <see cref="SaveHubClient"/>.</summary>
internal static class AppServices
{
    /// <summary>The app uses the per-user config location (shared with the CLI).</summary>
    public static SaveHubConfigStore Store { get; } = new(SaveHubConfigStore.DefaultPath);

    public static SaveHubConfig LoadConfig()
    {
        return Store.Load();
    }

    public static void SaveConfig(SaveHubConfig config)
    {
        Store.Save(config);
    }

    /// <summary>Builds a client for the active provider, or returns null with a reason.</summary>
    public static SaveHubClient? TryCreateClient(out string error)
    {
        return SaveHubHost.TryCreateClient(Store.Load(), out error);
    }
}
