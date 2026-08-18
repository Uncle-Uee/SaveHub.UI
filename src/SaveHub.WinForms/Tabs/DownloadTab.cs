using SaveHub.Core;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>Download tab: browse published saves by system and download an archive.</summary>
internal sealed partial class DownloadTab : UserControl, ITabView
{
    private MainFormController _controller = null!;
    private IShellContext _shell = null!;

    private Dictionary<string, string> _dlNames = new(StringComparer.OrdinalIgnoreCase);

    public DownloadTab()
    {
        InitializeComponent();
        UiHelpers.ConfigureListView(_dlList,
            ("Name", 150), ("Title ID", 90), ("Archive", 90), ("Type", 90), ("Description", 160));
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    public async Task OnActivatedAsync()
    {
        if (_dlSystem.Items.Count == 0)
        {
            await LoadSystems();
        }
    }

    private async void Download_SystemChanged(object? sender, EventArgs e)
    {
        await LoadDownloadSaves();
    }

    private async void Download_RefreshSystems(object? sender, EventArgs e)
    {
        await LoadSystems();
    }

    private async void Download_SelectionChanged(object? sender, EventArgs e)
    {
        await ShowIcon();
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
            string? current = _dlSystem.SelectedItem as string;
            _dlSystem.Items.Clear();
            foreach (string p in await _controller.ListPlatformsAsync(client))
            {
                _dlSystem.Items.Add(p);
            }
            if (current is not null && _dlSystem.Items.Contains(current))
            {
                _dlSystem.SelectedItem = current;
            }
        });
    }

    private async Task LoadDownloadSaves()
    {
        SaveHubClient? client = _shell.RequireClient();
        if (client is null || _dlSystem.SelectedItem is not string system)
        {
            return;
        }

        await _shell.RunBusy($"Loading {system} saves...", async () =>
        {
            _dlList.Items.Clear();
            _dlIcon.Image = null;
            _dlName.Text = string.Empty;
            _dlNames = new Dictionary<string, string>(await _controller.GetGameNamesAsync(client, system), StringComparer.OrdinalIgnoreCase);
            IReadOnlyList<string> games = await _controller.ListGamesAsync(client, system);
            foreach (string game in games)
            {
                string name = _dlNames.TryGetValue(game, out string? n) ? n : game;
                IReadOnlyList<SaveEntry> saves = await _controller.ListSavesAsync(client, system, game);
                foreach (SaveEntry s in saves)
                {
                    ListViewItem item = new ListViewItem([name, game, s.ArchiveName, MainFormController.Label(s.SaveType), s.Description ?? ""])
                    {
                        Tag = (system, game, s.ArchiveName),
                    };
                    _dlList.Items.Add(item);
                }
            }
        });
    }

    private async Task ShowIcon()
    {
        if (_dlList.SelectedItems.Count == 0 || _dlList.SelectedItems[0].Tag is not (string system, string game, _))
        {
            return;
        }
        _dlName.Text = _dlNames.TryGetValue(game, out string? n) ? $"{n}\n{game}" : game;
        SaveHubClient? client = _controller.TryCreateClient(out _);
        if (client is null)
        {
            return;
        }
        try
        {
            byte[]? bytes = await _controller.GetGameIconAsync(client, system, game);
            _dlIcon.Image = bytes is null ? null : Image.FromStream(new MemoryStream(bytes));
        }
        catch
        {
            _dlIcon.Image = null;
        }
    }

    private async void Download_Selected(object? sender, EventArgs e)
    {
        if (_dlList.SelectedItems.Count == 0)
        {
            _shell.Warn("Select a save to download.");
            return;
        }
        if (_dlList.SelectedItems[0].Tag is not (string system, string game, string archive))
        {
            return;
        }
        SaveHubClient? client = _shell.RequireClient();
        if (client is null)
        {
            return;
        }

        using SaveFileDialog dialog = new SaveFileDialog { FileName = archive, Filter = "Zip archive|*.zip" };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        await _shell.RunBusy("Downloading...", async () =>
        {
            bool ok = await _controller.DownloadArchiveToFileAsync(client, system, game, archive, dialog.FileName);
            if (ok)
            {
                _shell.SetStatus($"Downloaded to {dialog.FileName}");
            }
            else
            {
                _shell.Warn("The archive could not be found.");
            }
        });
    }
}
