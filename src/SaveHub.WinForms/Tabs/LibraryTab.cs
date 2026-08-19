using SaveHub.Core;
using SaveHub.Core.Models;

namespace SaveHub.WinForms.Tabs;

/// <summary>Library tab: a manufacturer → platform → game tree, backed by the root library index.</summary>
internal sealed partial class LibraryTab : UserControl, ITabView
{
    private MainFormController _controller = null!;
    private IShellContext _shell = null!;
    private bool _loaded;

    public LibraryTab()
    {
        InitializeComponent();
    }

    public void Initialize(MainFormController controller, IShellContext shell)
    {
        _controller = controller;
        _shell = shell;
    }

    public async Task OnActivatedAsync()
    {
        if (!_loaded)
        {
            _loaded = true;
            Populate(_controller.LoadLocalLibrary());
            await RefreshFromBackend(false);
        }
    }

    private async void Library_Refresh(object? sender, EventArgs e)
    {
        await RefreshFromBackend(false);
    }

    private async void Library_Rebuild(object? sender, EventArgs e)
    {
        await RefreshFromBackend(true);
    }

    /// <summary>Rebuilds the backend index and local cache (used by the menu bar).</summary>
    public Task RebuildAsync()
    {
        _loaded = true;
        return RefreshFromBackend(true);
    }

    private async Task RefreshFromBackend(bool rebuild)
    {
        SaveHubClient? client = rebuild ? _shell.RequireClient() : _controller.TryCreateClient(out _);
        if (client is null)
        {
            if (!rebuild)
            {
                _shell.SetStatus("Showing cached library — open Settings to connect for a fresh copy.");
            }
            return;
        }
        await _shell.RunBusy(rebuild ? "Rebuilding library index..." : "Loading library...", async () =>
        {
            LibraryIndex index = rebuild
                ? await _controller.RebuildLibraryIndexAsync(client)
                : await _controller.GetLibraryIndexAsync(client);
            _controller.SaveLocalLibrary(index);
            Populate(index);
        });
    }

    private void Populate(LibraryIndex index)
    {
        _tree.BeginUpdate();
        _tree.Nodes.Clear();

        Dictionary<string, TreeNode> byManufacturer = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, Dictionary<string, string>> platform in index.Platforms)
        {
            string manufacturer = Devices.ManufacturerFor(platform.Key);
            if (!byManufacturer.TryGetValue(manufacturer, out TreeNode? manufacturerNode))
            {
                manufacturerNode = new TreeNode(manufacturer);
                byManufacturer[manufacturer] = manufacturerNode;
            }

            TreeNode platformNode = new TreeNode($"{platform.Key} ({platform.Value.Count})");
            foreach (KeyValuePair<string, string> game in platform.Value.OrderBy(g => g.Value, StringComparer.OrdinalIgnoreCase))
            {
                string label = string.Equals(game.Value, game.Key, StringComparison.OrdinalIgnoreCase)
                    ? game.Key
                    : $"{game.Value} ({game.Key})";
                platformNode.Nodes.Add(label);
            }
            manufacturerNode.Nodes.Add(platformNode);
        }

        foreach (string manufacturer in ManufacturerOrder())
        {
            if (byManufacturer.TryGetValue(manufacturer, out TreeNode? node))
            {
                _tree.Nodes.Add(node);
                byManufacturer.Remove(manufacturer);
            }
        }
        foreach (TreeNode node in byManufacturer.Values)
        {
            _tree.Nodes.Add(node);
        }

        int totalGames = index.Platforms.Values.Sum(g => g.Count);
        _lblSummary.Text = $"{index.Platforms.Count} platform(s), {totalGames} game(s).";
        _tree.EndUpdate();
    }

    private static IEnumerable<string> ManufacturerOrder()
    {
        foreach (DeviceGroup group in Devices.Groups)
        {
            yield return group.Manufacturer;
        }
        yield return "Other";
    }
}
