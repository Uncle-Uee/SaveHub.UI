using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace SaveHub.Avalonia.Services;

/// <summary>Avalonia implementation of <see cref="IDialogService"/> bound to the main window.</summary>
internal sealed class DialogService : IDialogService
{
    private readonly Window _owner;

    public DialogService(Window owner)
    {
        _owner = owner;
    }

    public Task ShowMessageAsync(string title, string message)
    {
        return ShowDialogAsync(title, message, false);
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        return ShowDialogAsync(title, message, true);
    }

    public async Task<string?> PromptAsync(string title, string message, string defaultValue)
    {
        TaskCompletionSource<string?> completion = new TaskCompletionSource<string?>();

        Window dialog = new Window
        {
            Title = title,
            Width = 420,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
        };

        TextBox input = new TextBox { Text = defaultValue };
        Button okButton = new Button { Content = "OK", MinWidth = 88, IsDefault = true };
        Button cancelButton = new Button { Content = "Cancel", MinWidth = 88, IsCancel = true };
        okButton.Click += (_, _) =>
        {
            completion.TrySetResult(input.Text);
            dialog.Close();
        };
        cancelButton.Click += (_, _) =>
        {
            completion.TrySetResult(null);
            dialog.Close();
        };

        StackPanel buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
        };
        buttons.Children.Add(okButton);
        buttons.Children.Add(cancelButton);

        StackPanel root = new StackPanel { Margin = new Thickness(20), Spacing = 12 };
        root.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap });
        root.Children.Add(input);
        root.Children.Add(buttons);
        dialog.Content = root;

        dialog.Closed += (_, _) => completion.TrySetResult(null);

        await dialog.ShowDialog(_owner);
        return await completion.Task;
    }

    public async Task<IReadOnlyList<string>> OpenFilesAsync(string title, bool allowMultiple, string? filterName, IReadOnlyList<string>? patterns)
    {
        FilePickerOpenOptions options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple,
        };
        if (filterName is not null && patterns is not null)
        {
            options.FileTypeFilter = [new FilePickerFileType(filterName) { Patterns = patterns.ToList() }];
        }

        IReadOnlyList<IStorageFile> files = await _owner.StorageProvider.OpenFilePickerAsync(options);
        List<string> paths = new List<string>();
        foreach (IStorageFile file in files)
        {
            string? path = file.TryGetLocalPath();
            if (path is not null)
            {
                paths.Add(path);
            }
        }
        return paths;
    }

    public async Task<string?> OpenFolderAsync(string title)
    {
        IReadOnlyList<IStorageFolder> folders = await _owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
        });
        return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
    }

    public async Task<string?> SaveFileAsync(string title, string suggestedName, string? filterName, IReadOnlyList<string>? patterns)
    {
        FilePickerSaveOptions options = new FilePickerSaveOptions
        {
            Title = title,
            SuggestedFileName = suggestedName,
        };
        if (filterName is not null && patterns is not null)
        {
            options.FileTypeChoices = [new FilePickerFileType(filterName) { Patterns = patterns.ToList() }];
        }

        IStorageFile? file = await _owner.StorageProvider.SaveFilePickerAsync(options);
        return file?.TryGetLocalPath();
    }

    private async Task<bool> ShowDialogAsync(string title, string message, bool confirm)
    {
        TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();

        Window dialog = new Window
        {
            Title = title,
            SizeToContent = SizeToContent.WidthAndHeight,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ShowInTaskbar = false,
            MinWidth = 320,
        };

        StackPanel buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
        };

        Button okButton = new Button { Content = confirm ? "Yes" : "OK", MinWidth = 88, IsDefault = true };
        okButton.Click += (_, _) =>
        {
            completion.TrySetResult(true);
            dialog.Close();
        };
        buttons.Children.Add(okButton);

        if (confirm)
        {
            Button cancelButton = new Button { Content = "No", MinWidth = 88, IsCancel = true };
            cancelButton.Click += (_, _) =>
            {
                completion.TrySetResult(false);
                dialog.Close();
            };
            buttons.Children.Add(cancelButton);
        }

        StackPanel root = new StackPanel { Margin = new Thickness(20), Spacing = 16 };
        root.Children.Add(new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, MaxWidth = 460 });
        root.Children.Add(buttons);
        dialog.Content = root;

        dialog.Closed += (_, _) => completion.TrySetResult(false);

        await dialog.ShowDialog(_owner);
        return await completion.Task;
    }
}
