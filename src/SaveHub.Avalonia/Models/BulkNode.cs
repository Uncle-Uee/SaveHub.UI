using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SaveHub.Avalonia.Models;

/// <summary>
/// A node in the bulk-folders upload tree. A top-level node is an upload unit: a folder (one save-folder
/// upload) or a file (one memory card). Child nodes are the folders/files inside a game folder, shown so
/// the user can exclude specific items. Editing/removing here only changes what gets uploaded, never disk.
/// </summary>
public sealed partial class BulkNode : ObservableObject
{
    /// <summary>Platform codes selectable per top-level node.</summary>
    public static IReadOnlyList<string> Platforms { get; } = Devices.All.Select(o => o.Code).ToList();

    private readonly string _path;

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _platform;

    [ObservableProperty]
    private bool _isIncluded = true;

    [ObservableProperty]
    private bool _isExpanded;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIcon))]
    private string? _iconPath;

    [ObservableProperty]
    private string _titleId = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    public BulkNode(string path, string name, bool isFolder, bool isTopLevel, string platform, bool isUploadUnit = false, bool isCardGroup = false)
    {
        _path = path;
        _name = name;
        IsFolder = isFolder;
        IsTopLevel = isTopLevel;
        _platform = platform;
        IsUploadUnit = isUploadUnit;
        IsCardGroup = isCardGroup;
    }

    /// <summary>Absolute path of the source folder or file.</summary>
    public string Path => _path;

    /// <summary>True when this node is a folder (a save-folder upload); false for a memory-card file.</summary>
    public bool IsFolder { get; }

    /// <summary>True for an upload unit (a folder or file the user added), false for an inner item.</summary>
    public bool IsTopLevel { get; }

    /// <summary>True when this node is uploaded on its own (a folder or a memory card), not a container/inner file.</summary>
    public bool IsUploadUnit { get; }

    /// <summary>True for a folder that groups individual memory cards (its children are the uploads).</summary>
    public bool IsCardGroup { get; }

    public ObservableCollection<BulkNode> Children { get; } = [];

    /// <summary>True when this node has a user-picked cover icon.</summary>
    public bool HasIcon => IconPath is not null;

    partial void OnIsIncludedChanged(bool value)
    {
        foreach (BulkNode child in Children)
        {
            child.IsIncluded = value;
        }
    }
}
