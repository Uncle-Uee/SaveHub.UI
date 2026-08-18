using SaveHub.Core;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>Manage tab: browse and delete published saves (supports multi-select).</summary>
internal sealed partial class ManageTab : UserControl, ITabView
{
    private MainFormController _controller = null!;
    private IShellContext _shell = null!;

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
            _mgGame.Items.Clear();
            _mgList.Items.Clear();
            _mgIcon.Image = null;
            _mgName.Text = "";
            foreach (string game in await _controller.ListGamesAsync(client, system))
            {
                _mgGame.Items.Add(game);
            }
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
            _mgList.Items.Clear();
            foreach (SaveEntry s in await _controller.ListSavesAsync(client, system, game))
            {
                _mgList.Items.Add(new ListViewItem([s.ArchiveName, MainFormController.Label(s.SaveType), s.Description ?? ""]) { Tag = s });
            }

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
}
