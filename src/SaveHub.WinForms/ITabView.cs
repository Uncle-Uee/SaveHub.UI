namespace SaveHub.WinForms;

/// <summary>A tab hosted by <see cref="MainForm"/> that shares the controller and shell.</summary>
internal interface ITabView
{
    /// <summary>Injects the shared logic controller and shell services after construction.</summary>
    void Initialize(MainFormController controller, IShellContext shell);

    /// <summary>Called when the tab becomes the active tab (e.g. to lazily load systems).</summary>
    Task OnActivatedAsync();
}
