using System.Linq;
using Avalonia.Controls;
using SaveHub.Avalonia.Models;
using SaveHub.Avalonia.ViewModels;

namespace SaveHub.Avalonia.Views;

public partial class ManageView : UserControl
{
    public ManageView()
    {
        InitializeComponent();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid grid && DataContext is ManageViewModel viewModel)
        {
            viewModel.SetSelection(grid.SelectedItems.OfType<SaveRow>());
        }
    }
}
