using SaveHub.Core;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>Manage tab: browse and delete published saves (supports multi-select).</summary>
internal sealed partial class ManageTab : UserControl, ITabView
{
    private MainFormController _controller = null!;
    private IShellContext _shell = null!;
    private readonly List<string> _allGames = new();
    private readonly List<SaveEntry> _allSaves = new();
    private bool _suppressGameChange;

    public ManageTab()
    {
        InitializeComponent();
        UiHelpers.ConfigureListView(_mgList, ("Archive", 130), ("Type", 110), ("Description", 340));
        _mgList.MultiSelect = true;
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    public async Task OnActivatedAsync()
    {
        if (_mgSystem.Items.Count == 0)
        {
            await LoadSystems();
        }
    }

    private async void Manage_SystemChanged(object? sender, EventArgs e)
    {
        await LoadManageGames();
    }

    private async void Manage_GameChanged(object? sender, EventArgs e)
    {
        if (_suppressGameChange)
        {
            return;
        }
        await LoadManageSaves();
    }

    private async void Manage_RefreshSystems(object? sender, EventArgs e)
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
            string? current = _mgSystem.SelectedItem as string;
            _mgSystem.Items.Clear();
            foreach (string p in await _controller.ListPlatformsAsync(client))
            {
                _mgSystem.Items.Add(p);
            }
            if (current is not null && _mgSystem.Items.Contains(current))
            {
                _mgSystem.SelectedItem = current;
            }
        });
    }

    private async Task LoadManageGames()
    {
        SaveHubClient? client = _shell.RequireClient();
        if (client is null || _mgSystem.SelectedItem is not string system)
        {
            return;
        }
        await _shell.RunBusy($"Loading {system} games...", async () =>
        {
            _allGames.Clear();
            _allSaves.Clear();
            _mgList.Items.Clear();
            _mgIcon.Image = null;
            _mgName.Text = "";
            _lblDetails.Text = "";
            foreach (string game in await _controller.ListGamesAsync(client, system))
            {
                _allGames.Add(game);
            }
            ApplyGameFilter();
        });
    }

    private async Task LoadManageSaves()
    {
        SaveHubClient? client = _shell.RequireClient();
        if (client is null || _mgSystem.SelectedItem is not string system || _mgGame.SelectedItem is not string game)
        {
            return;
        }
        await _shell.RunBusy("Loading saves...", async () =>
        {
            _allSaves.Clear();
            foreach (SaveEntry s in await _controller.ListSavesAsync(client, system, game))
            {
                _allSaves.Add(s);
            }
            ApplySaveFilter();

            IReadOnlyDictionary<string, string> names = await _controller.GetGameNamesAsync(client, system);
            _mgName.Text = names.TryGetValue(game, out string? n) ? $"{n}\n{game}" : game;
            try
            {
                byte[]? icon = await _controller.GetGameIconAsync(client, system, game);
                _mgIcon.Image = icon is null ? null : Image.FromStream(new MemoryStream(icon));
            }
            catch
            {
                _mgIcon.Image = null;
            }
        });
    }

    private async void Manage_DeleteSelected(object? sender, EventArgs e)
    {
        if (_mgList.SelectedItems.Count == 0)
        {
            _shell.Warn("Select one or more saves to delete.");
            return;
        }
        List<SaveEntry> entries = _mgList.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag).OfType<SaveEntry>().ToList();
        if (entries.Count == 0)
        {
            return;
        }

        DialogResult confirm = MessageBox.Show(this,
            $"Delete {entries.Count} save(s)? This cannot be undone.",
            "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }

        await _shell.RunBusy($"Deleting {entries.Count} save(s)...", async () =>
        {
            foreach (SaveEntry entry in entries)
            {
                await _controller.DeleteSaveAsync(client, entry.Platform, entry.GameId, entry.ArchiveName);
            }
            _shell.SetStatus($"Deleted {entries.Count} save(s).");
            await LoadManageSaves();
        });
    }

    private async void Manage_DownloadSelected(object? sender, EventArgs e)
    {
        if (_mgList.SelectedItems.Count == 0)
        {
            _shell.Warn("Select one or more saves to download.");
            return;
        }
        List<SaveEntry> entries = _mgList.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag).OfType<SaveEntry>().ToList();
        if (entries.Count == 0)
        {
            return;
        }

        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }

        using FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Choose a folder for the downloaded saves" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }
        string folder = dialog.SelectedPath;

        await _shell.RunBusy($"Downloading {entries.Count} save(s)...", async () =>
        {
            int ok = 0;
            foreach (SaveEntry entry in entries)
            {
                string destination = Path.Combine(folder, entry.ArchiveName);
                if (await _controller.DownloadArchiveToFileAsync(client, entry.Platform, entry.GameId, entry.ArchiveName, destination))
                {
                    ok++;
                }
            }
            _shell.SetStatus($"Downloaded {ok} of {entries.Count} save(s) to {folder}.");
        });
    }

    private async void Manage_DeleteAll(object? sender, EventArgs e)
    {
        if (_allSaves.Count == 0 || _mgGame.SelectedItem is not string game)
        {
            _shell.Warn("Select a game with saves first.");
            return;
        }
        List<SaveEntry> entries = _allSaves.ToList();

        DialogResult confirm = MessageBox.Show(this,
            $"Delete ALL {entries.Count} save(s) for {game}? This cannot be undone.",
            "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes)
        {
            return;
        }

        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }

        await _shell.RunBusy($"Deleting {entries.Count} save(s)...", async () =>
        {
            foreach (SaveEntry entry in entries)
            {
                await _controller.DeleteSaveAsync(client, entry.Platform, entry.GameId, entry.ArchiveName);
            }
            _shell.SetStatus($"Deleted all {entries.Count} save(s) for {game}.");
            await LoadManageSaves();
        });
    }

    private void Manage_SelectionChanged(object? sender, EventArgs e)
    {
        if (_mgList.SelectedItems.Count > 0 && _mgList.SelectedItems[0].Tag is SaveEntry s)
        {
            string description = string.IsNullOrWhiteSpace(s.Description) ? "(none)" : s.Description;
            _lblDetails.Text = $"Archive: {s.ArchiveName}\nType: {MainFormController.Label(s.SaveType)}\nIndex: {s.Index}\nDescription: {description}";
        }
        else
        {
            _lblDetails.Text = "";
        }
    }

    private void GameFilter_Changed(object? sender, EventArgs e)
    {
        ApplyGameFilter();
    }

    private void SaveFilter_Changed(object? sender, EventArgs e)
    {
        ApplySaveFilter();
    }

    private void ApplyGameFilter()
    {
        string filter = _txtGameFilter.Text.Trim();
        string? current = _mgGame.SelectedItem as string;
        _suppressGameChange = true;
        _mgGame.Items.Clear();
        foreach (string game in _allGames)
        {
            if (filter.Length == 0 || game.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                _mgGame.Items.Add(game);
            }
        }
        if (current is not null && _mgGame.Items.Contains(current))
        {
            _mgGame.SelectedItem = current;
        }
        _suppressGameChange = false;
    }

    private void ApplySaveFilter()
    {
        string filter = _txtSaveFilter.Text.Trim();
        _mgList.Items.Clear();
        foreach (SaveEntry s in _allSaves)
        {
            string type = MainFormController.Label(s.SaveType);
            string description = s.Description ?? "";
            if (filter.Length == 0 ||
                s.ArchiveName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                type.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                description.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                _mgList.Items.Add(new ListViewItem([s.ArchiveName, type, description]) { Tag = s });
            }
        }
    }
}
