using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SaveHub.Avalonia.Views;

public partial class UploadView : UserControl
{
    public UploadView()
    {
        InitializeComponent();
    }

    private void EditName_Click(object? sender, RoutedEventArgs e)
    {
        GameNameBox.Focus();
    }
}
