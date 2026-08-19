using SaveHub.Core;
using SaveHub.Core.Abstractions;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>Edit tab: replace an existing save's contents/description in place.</summary>
internal sealed partial class EditTab : UserControl, ITabView
{
    private const int MaxDescription = 256;

    private MainFormController _controller = null!;
    private IShellContext _shell = null!;

    private readonly List<string> _edFilesList = [];
    private string? _edRoot;
    private SaveEntry? _edSelected;

    public EditTab()
    {
        InitializeComponent();
        UiHelpers.ConfigureListView(_edList, ("Archive", 130), ("Type", 120), ("Description", 340));
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    public async Task OnActivatedAsync()
    {
        if (_edSystem.Items.Count == 0)
        {
            await LoadSystems();
        }
    }

    private async void Edit_SystemChanged(object? sender, EventArgs e)
    {
        await LoadEditGames();
    }

    private async void Edit_GameChanged(object? sender, EventArgs e)
    {
        await LoadEditSaves();
    }

    private async void Edit_RefreshSystems(object? sender, EventArgs e)
    {
        await LoadSystems();
    }

    private async Task LoadSystems()
    {
        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }
        await _shell.RunBusy("Loading systems...", async () =>
        {
            string? current = _edSystem.SelectedItem as string;
            _edSystem.Items.Clear();
            foreach (string p in await _controller.ListPlatformsAsync(client))
            {
                _edSystem.Items.Add(p);
            }
            if (current is not null && _edSystem.Items.Contains(current))
            {
                _edSystem.SelectedItem = current;
            }
        });
    }

    private async Task LoadEditGames()
    {
        SaveHubClient? client = _shell.RequireClient();
        if (client is null || _edSystem.SelectedItem is not string system)
        {
            return;
        }
        await _shell.RunBusy($"Loading {system} games...", async () =>
        {
            _edGame.Items.Clear();
            _edList.Items.Clear();
            IReadOnlyDictionary<string, string> names = await _controller.GetPlatformNamesAsync(client, system);
            foreach (string game in await _controller.ListGamesAsync(client, system))
            {
                _edGame.Items.Add(new GameOption(game, MainFormController.GameDisplay(game, names)));
            }
        });
    }

    private async Task LoadEditSaves()
    {
        SaveHubClient? client = _shell.RequireClient();
        if (client is null || _edSystem.SelectedItem is not string system || _edGame.SelectedItem is not GameOption game)
        {
            return;
        }
        await _shell.RunBusy("Loading saves...", async () =>
        {
            _edList.Items.Clear();
            foreach (SaveEntry s in await _controller.ListSavesAsync(client, system, game.Id))
            {
                _edList.Items.Add(new ListViewItem([s.ArchiveName, MainFormController.Label(s.SaveType), s.Description ?? ""]) { Tag = s });
            }

            IReadOnlyDictionary<string, string> names = await _controller.GetPlatformNamesAsync(client, system);
            _edName.Text = names.TryGetValue(game.Id, out string? n) ? $"{n}\n{game.Id}" : game.Id;
            try
            {
                byte[]? icon = await _controller.GetGameIconAsync(client, system, game.Id);
                _edIcon.Image = icon is null ? null : Image.FromStream(new MemoryStream(icon));
            }
            catch
            {
                _edIcon.Image = null;
            }
        });
    }

    private void Edit_SelectionChanged(object? sender, EventArgs e)
    {
        _edSelected = _edList.SelectedItems.Count > 0 ? _edList.SelectedItems[0].Tag as SaveEntry : null;
        if (_edSelected is not null)
        {
            _edDescription.Text = _edSelected.Description ?? "";
        }
    }

    private void Edit_Browse(object? sender, EventArgs e)
    {
        if (_edSelected is null)
        {
            _shell.Warn("Select a save to replace first.");
            return;
        }
        _edFilesList.Clear();
        _edRoot = null;

        if (_edSelected.SaveType == SaveType.SaveFolder)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _edRoot = dialog.SelectedPath;
            _edFilesList.AddRange(Directory.GetFiles(_edRoot, "*", SearchOption.AllDirectories));
            _edPath.Text = _edRoot;
        }
        else
        {
            using OpenFileDialog dialog = new OpenFileDialog { Multiselect = _edSelected.SaveType != SaveType.MemoryCard };
            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }
            _edFilesList.AddRange(dialog.FileNames);
            _edPath.Text = _edFilesList.Count == 1 ? _edFilesList[0] : $"{_edFilesList.Count} files selected";
        }
    }

    private async void Edit_Update(object? sender, EventArgs e)
    {
        if (_edSelected is null)
        {
            _shell.Warn("Select a save to replace.");
            return;
        }
        if (_edFilesList.Count == 0)
        {
            _shell.Warn("Browse the replacement file(s)/folder.");
            return;
        }
        string description = _edDescription.Text.Trim();
        if (description.Length == 0)
        {
            _shell.Warn("Enter a description.");
            return;
        }
        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }

        SaveEntry entry = _edSelected;
        SaveUploadRequest request = new SaveUploadRequest
        {
            Platform = entry.Platform,
            GameId = entry.GameId,
            SaveType = entry.SaveType,
            Files = _edFilesList.ToList(),
            RootDirectory = entry.SaveType == SaveType.SaveFolder ? _edRoot : null,
            Description = description,
            AutoFetchCoverArt = false,
        };

        await _shell.RunBusy($"Updating {entry.ArchiveName}...", async () =>
        {
            SaveUploadResult result = await _controller.UploadAsync(client, request, new UploadOptions { TargetIndex = entry.Index });
            _shell.ShowResult(result);
            await LoadEditSaves();
        });
    }
}
