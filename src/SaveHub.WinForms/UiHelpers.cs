namespace SaveHub.WinForms;

/// <summary>Shared UI helpers used across the tab user controls.</summary>
internal static class UiHelpers
{
    /// <summary>Configures a details-view <see cref="ListView"/> with the given columns.</summary>
    public static void ConfigureListView(ListView lv, params (string Header, int Width)[] columns)
    {
        lv.View = View.Details;
        lv.FullRowSelect = true;
        lv.GridLines = true;
        lv.MultiSelect = false;
        lv.Scrollable = true;
        lv.Columns.Clear();
        foreach ((string header, int width) in columns)
        {
            lv.Columns.Add(header, width);
        }
    }
}
