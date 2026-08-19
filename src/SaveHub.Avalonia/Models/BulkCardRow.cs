using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SaveHub.Avalonia.Models;

/// <summary>
/// One memory card in a bulk upload: its detected/edited game name and id, the chosen action
/// (upload the card and index it, or index only), and the source file it came from.
/// </summary>
public sealed partial class BulkCardRow : ObservableObject
{
    /// <summary>Action that uploads the card archive and adds it to the index.</summary>
    public const string ModeUploadIndex = "Upload + Index";

    /// <summary>Action that adds the card to the index only (no archive upload).</summary>
    public const string ModeIndexOnly = "Index only";

    /// <summary>The two selectable per-card actions.</summary>
    public static IReadOnlyList<string> Modes { get; } = [ModeUploadIndex, ModeIndexOnly];

    private readonly string _path;

    [ObservableProperty]
    private string _game;

    [ObservableProperty]
    private string _titleId;

    [ObservableProperty]
    private string _mode;

    public BulkCardRow(string path, string fileName, string game, string titleId, string mode)
    {
        _path = path;
        FileName = fileName;
        _game = game;
        _titleId = titleId;
        _mode = mode;
    }

    /// <summary>Absolute path of the memory-card file.</summary>
    public string Path => _path;

    /// <summary>Display file name of the memory-card file.</summary>
    public string FileName { get; }
}
