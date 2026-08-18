using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SaveHub.Avalonia.Services;
using SaveHub.Avalonia.ViewModels;
using SaveHub.Avalonia.Views;

namespace SaveHub.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindowViewModel viewModel = new MainWindowViewModel();
            MainWindow window = new MainWindow
            {
                DataContext = viewModel,
            };
            viewModel.AttachDialogs(new DialogService(window));
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}