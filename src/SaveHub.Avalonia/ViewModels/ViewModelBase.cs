using CommunityToolkit.Mvvm.ComponentModel;

namespace SaveHub.Avalonia.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    /// <summary>Called when the owning tab becomes active (e.g. to lazily load systems).</summary>
    public virtual Task OnActivatedAsync()
    {
        return Task.CompletedTask;
    }
}
