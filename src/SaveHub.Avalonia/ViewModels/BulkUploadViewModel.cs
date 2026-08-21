using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SaveHub.Avalonia.Common;
using SaveHub.Avalonia.Models;
using SaveHub.Avalonia.Services;
using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Archiving;
using SaveHub.Core.Models;

namespace SaveHub.Avalonia.ViewModels;

/// <summary>
/// Bulk Upload tab: add a parent folder (each sub-folder = a game) or files, curate the tree, and
/// upload each item. Folders upload as save folders; files as memory cards (which also update the
/// platform's <c>!index</c> catalog).
/// </summary>
public sealed partial class BulkUploadViewModel : ViewModelBase
{
    private readonly AppController _controller;
    private readonly IShellContext _shell;

    [ObservableProperty]
    private BulkNode? _selectedBulkNode;

    [ObservableProperty]
    private bool _bulkExpanded;

    [ObservableProperty]
    private Bitmap? _bulkCoverPreview;

    [ObservableProperty]
    private bool _hasBulkSelection;

    public ObservableCollection<BulkNode> Roots { get; } = [];

    internal BulkUploadViewModel(AppController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    [RelayCommand]
    private async Task AddFolder()
    {
        string? root = await _shell.PickFolderAsync("Select a game save folder (or a folder containing several)");
        if (root is null)
        {
            return;
        }
        string[] subDirs = Directory.GetDirectories(root);
        IReadOnlyList<string> gameFolders = subDirs.Length > 0 ? subDirs : [root];
        foreach (string folder in gameFolders)
        {
            AddGameFolderNode(folder);
        }
    }

    [RelayCommand]
    private async Task AddFiles()
    {
        IReadOnlyList<string> files = await _shell.PickFilesAsync("Select memory-card file(s)", true, null, null);
        foreach (string file in files)
        {
            AddCardNode(file);
        }
    }

    [RelayCommand]
    private void EditName()
    {
        if (SelectedBulkNode is { } node)
        {
            node.IsEditing = !node.IsEditing;
        }
        else
        {
            _shell.SetStatus("Select an item to rename.");
        }
    }

    [RelayCommand]
    private void Remove()
    {
        if (SelectedBulkNode is not { } node)
        {
            _shell.SetStatus("Select an item to remove.");
            return;
        }
        RemoveNode(Roots, node);
        SelectedBulkNode = null;
    }

    [RelayCommand]
    private async Task SetIcon()
    {
        if (SelectedBulkNode is not { IsUploadUnit: true } node)
        {
            _shell.SetStatus("Select a top-level folder or file, then Set icon.");
            return;
        }
        IReadOnlyList<string> files = await _shell.PickFilesAsync(
            "Select an icon", false, "Images", ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.gif", "*.bmp"]);
        if (files.Count > 0)
        {
            node.IconPath = files[0];
            UpdateBulkCover();
            _shell.SetStatus($"Icon set for '{node.Name}'.");
        }
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (Roots.Count == 0)
        {
            await _shell.WarnAsync("Add folders or files to upload first.");
            return;
        }
        foreach (BulkNode node in Roots)
        {
            if (!node.IsIncluded)
            {
                continue;
            }
            if (node.IsCardGroup)
            {
                foreach (BulkNode child in node.Children)
                {
                    if (child.IsIncluded && string.IsNullOrWhiteSpace(child.Platform))
                    {
                        await _shell.WarnAsync($"Select a platform for '{child.Name}'.");
                        return;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(node.Platform))
            {
                await _shell.WarnAsync($"Select a platform for '{node.Name}'.");
                return;
            }
        }

        SaveHubClient? client = await _shell.RequireClientAsync();
        if (client is null)
        {
            return;
        }

        int uploaded = 0;
        Dictionary<string, List<MemoryCardIndexEntry>> cardIndex = new(StringComparer.OrdinalIgnoreCase);
        await _shell.RunBusy("Uploading...", async () =>
        {
            foreach (BulkNode node in Roots)
            {
                if (!node.IsIncluded)
                {
                    continue;
                }
                if (node.IsCardGroup)
                {
                    foreach (BulkNode child in node.Children)
                    {
                        if (child.IsIncluded && await UploadNode(client, child, cardIndex))
                        {
                            uploaded++;
                        }
                    }
                }
                else if (await UploadNode(client, node, cardIndex))
                {
                    uploaded++;
                }
            }

            foreach (KeyValuePair<string, List<MemoryCardIndexEntry>> pair in cardIndex)
            {
                await _controller.UpdateMemoryCardIndexAsync(client, pair.Key, pair.Value);
            }
        });
        _shell.SetStatus($"Bulk upload complete: {uploaded} item(s) uploaded.");
    }

    private async Task<bool> UploadNode(SaveHubClient client, BulkNode node, Dictionary<string, List<MemoryCardIndexEntry>> cardIndex)
    {
        string name = node.Name.Trim();
        SaveType saveType = node.IsFolder ? SaveType.SaveFolder : SaveType.MemoryCard;
        List<string> files = new List<string>();
        if (node.IsFolder)
        {
            CollectIncludedFiles(node, files);
        }
        else
        {
            files.Add(node.Path);
        }
        if (files.Count == 0)
        {
            return false;
        }
        string? titleId = node.TitleId.Trim().Length > 0
            ? node.TitleId.Trim()
            : (node.IsFolder ? null : _controller.DetectTitleId(node.Platform, SaveType.MemoryCard, files));
        GameIdResolution resolution = _controller.Resolve(node.Platform, saveType, files, titleId, name.Length > 0 ? name : null);
        string displayName = name.Length > 0 ? name : resolution.GameId;
        string description = node.Description.Trim().Length > 0 ? node.Description.Trim() : displayName;
        SaveUploadRequest request = new SaveUploadRequest
        {
            Platform = node.Platform,
            GameId = resolution.GameId,
            SaveType = saveType,
            Files = files,
            RootDirectory = node.IsFolder ? node.Path : null,
            Description = description,
            GameTitle = name.Length > 0 ? name : null,
            IconPath = node.IconPath,
            AutoFetchCoverArt = node.IconPath is null,
        };
        await _controller.UploadAsync(client, request, new UploadOptions());

        if (!node.IsFolder)
        {
            if (!cardIndex.TryGetValue(node.Platform, out List<MemoryCardIndexEntry>? list))
            {
                list = new List<MemoryCardIndexEntry>();
                cardIndex[node.Platform] = list;
            }
            list.Add(new MemoryCardIndexEntry(resolution.GameId, displayName));
        }
        return true;
    }

    partial void OnBulkExpandedChanged(bool value)
    {
        SetExpanded(Roots, value);
    }

    partial void OnSelectedBulkNodeChanged(BulkNode? value)
    {
        HasBulkSelection = value is { IsUploadUnit: true };
        UpdateBulkCover();
    }

    [RelayCommand]
    private void DetectBulkTitleId()
    {
        if (SelectedBulkNode is not { IsUploadUnit: true } node)
        {
            _shell.SetStatus("Select a card or folder first.");
            return;
        }
        if (string.IsNullOrWhiteSpace(node.Platform))
        {
            _shell.SetStatus("Set the platform first.");
            return;
        }
        List<string> files = NodeFiles(node);
        string? id = _controller.DetectTitleId(node.Platform, node.IsFolder ? SaveType.SaveFolder : SaveType.MemoryCard, files);
        if (!string.IsNullOrWhiteSpace(id))
        {
            node.TitleId = id;
            _shell.SetStatus($"Detected Title ID: {id}");
        }
        else
        {
            _shell.SetStatus("No Title ID found on this item.");
        }
        UpdateBulkCover();
    }

    private void UpdateBulkCover()
    {
        BulkCoverPreview = ResolveBulkCover(SelectedBulkNode);
    }

    private Bitmap ResolveBulkCover(BulkNode? node)
    {
        if (node is { IsUploadUnit: true })
        {
            if (node.IconPath is { } icon && File.Exists(icon) && CoverImages.TryLoad(icon) is { } user)
            {
                return user;
            }
            if (!string.IsNullOrEmpty(node.Platform) && node.TitleId.Trim().Length > 0 &&
                _controller.TryGetCachedCover(node.Platform, node.TitleId.Trim()) is { } cached &&
                CoverImages.TryLoad(cached) is { } cover)
            {
                return cover;
            }
        }
        return CoverImages.Placeholder();
    }

    private static List<string> NodeFiles(BulkNode node)
    {
        List<string> files = new List<string>();
        if (node.IsFolder)
        {
            files.AddRange(Directory.GetFiles(node.Path, "*", SearchOption.AllDirectories));
        }
        else
        {
            files.Add(node.Path);
        }
        return files;
    }

    private void AddGameFolderNode(string folder)
    {
        // A folder of PS1/PS2 memory cards stays as a parent node; each card is an indexed child.
        List<string> cardFiles = MemoryCardFilesIn(folder);
        if (cardFiles.Count > 0)
        {
            string groupName = Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar));
            BulkNode group = new BulkNode(folder, groupName, true, true, string.Empty, isUploadUnit: false, isCardGroup: true) { IsExpanded = true };
            foreach (string card in cardFiles)
            {
                group.Children.Add(CreateCardNode(card, false));
            }
            Roots.Add(group);
            _shell.SetStatus($"Added {cardFiles.Count} memory card(s) under '{groupName}'.");
            return;
        }

        string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
        string platform = _controller.DetectFolderPlatform(files) ?? string.Empty;
        BulkNode node = new BulkNode(folder, Path.GetFileName(folder.TrimEnd(Path.DirectorySeparatorChar)), true, true, platform, isUploadUnit: true);
        AddFolderChildren(node, folder);
        Roots.Add(node);
        _shell.SetStatus($"Added '{node.Name}'. Set its platform on the right if it isn't detected.");
    }

    private BulkNode CreateCardNode(string file, bool topLevel)
    {
        string platform = _controller.DetectMemoryCardPlatform(file) ?? string.Empty;
        string gameName = _controller.DetectSaveName(platform, [file]) ?? Path.GetFileName(file);
        string titleId = _controller.DetectTitleId(platform, SaveType.MemoryCard, [file]) ?? string.Empty;
        return new BulkNode(file, gameName, false, topLevel, platform, isUploadUnit: true) { TitleId = titleId };
    }

    private void AddCardNode(string file)
    {
        Roots.Add(CreateCardNode(file, true));
    }

    private List<string> MemoryCardFilesIn(string folder)
    {
        List<string> cards = new List<string>();
        foreach (string file in Directory.GetFiles(folder, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            if (_controller.DetectMemoryCardPlatform(file) is not null)
            {
                cards.Add(file);
            }
        }
        return cards;
    }

    private static void AddFolderChildren(BulkNode parent, string dir)
    {
        foreach (string sub in Directory.GetDirectories(dir).OrderBy(d => d, StringComparer.OrdinalIgnoreCase))
        {
            BulkNode dirNode = new BulkNode(sub, Path.GetFileName(sub), true, false, string.Empty);
            parent.Children.Add(dirNode);
            AddFolderChildren(dirNode, sub);
        }
        foreach (string file in Directory.GetFiles(dir).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
        {
            parent.Children.Add(new BulkNode(file, Path.GetFileName(file), false, false, string.Empty));
        }
    }

    private static bool RemoveNode(ObservableCollection<BulkNode> nodes, BulkNode target)
    {
        if (nodes.Remove(target))
        {
            return true;
        }
        foreach (BulkNode node in nodes)
        {
            if (RemoveNode(node.Children, target))
            {
                return true;
            }
        }
        return false;
    }

    private static void SetExpanded(IReadOnlyList<BulkNode> nodes, bool value)
    {
        foreach (BulkNode node in nodes)
        {
            node.IsExpanded = value;
            SetExpanded(node.Children, value);
        }
    }

    private static void CollectIncludedFiles(BulkNode node, List<string> files)
    {
        foreach (BulkNode child in node.Children)
        {
            if (!child.IsIncluded)
            {
                continue;
            }
            if (child.IsFolder)
            {
                CollectIncludedFiles(child, files);
            }
            else
            {
                files.Add(child.Path);
            }
        }
    }
}
