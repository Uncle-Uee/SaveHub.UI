using System.IO;
using System.Text.Json;
using SaveHub.Core.Configuration;

namespace SaveHub.Avalonia.Common;

/// <summary>
/// Session-only JSON scratch file that mirrors the Upload tab's staged saves so per-card metadata
/// is never lost while switching between cards. It is not meant to survive across app runs.
/// </summary>
internal sealed class PendingUploadStore
{
    private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };
    private readonly string _path;

    public PendingUploadStore(string path)
    {
        _path = path;
    }

    /// <summary>Default scratch-file location under the shared per-user SaveHub folder.</summary>
    public static string DefaultPath
    {
        get
        {
            string folder = Path.GetDirectoryName(SaveHubConfigStore.DefaultPath)!;
            return Path.Combine(folder, "pending-upload.json");
        }
    }

    public void Save(IReadOnlyList<PendingSave> items)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            File.WriteAllText(_path, JsonSerializer.Serialize(items, Options));
        }
        catch (IOException)
        {
            // Scratch persistence is best-effort.
        }
    }

    public void Clear()
    {
        try
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        catch (IOException)
        {
            // Ignore; the file is only scratch state.
        }
    }
}
