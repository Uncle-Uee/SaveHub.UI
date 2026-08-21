using SaveHub.Core.Models;

namespace SaveHub.Avalonia.Common;

/// <summary>One staged save in the Upload tab: its files plus the metadata entered for it.</summary>
internal sealed class PendingSave
{
    public string Key { get; set; } = string.Empty;
    public SaveType SaveType { get; set; } = SaveType.MemoryCard;
    public List<string> Files { get; set; } = [];
    public string? RootDirectory { get; set; }
    public string? Device { get; set; }
    public string TitleId { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconPath { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
