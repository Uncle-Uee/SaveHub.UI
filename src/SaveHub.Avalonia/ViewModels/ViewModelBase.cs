using CommunityToolkit.Mvvm.ComponentModel;

namespace SaveHub.Avalonia.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    /// <summary>Shown in the status bar when no storage provider is ready.</summary>
    protected const string NoProviderMessage = "No storage provider is ready — open Settings to configure or sign in.";

    /// <summary>Called when the owning tab becomes active (e.g. to lazily load systems).</summary>
    public virtual Task OnActivatedAsync()
    {
        return Task.CompletedTask;
    }
}
