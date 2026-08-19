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

    /// <summary>Shows a modal single-line text prompt; returns null when cancelled.</summary>
    public static string? Prompt(IWin32Window owner, string title, string message, string defaultValue)
    {
        using Form form = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(400, 116),
        };
        Label label = new Label { Text = message, Left = 12, Top = 12, Width = 376, Height = 20, AutoSize = false };
        TextBox textBox = new TextBox { Text = defaultValue, Left = 12, Top = 38, Width = 376 };
        Button ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 232, Top = 72, Width = 75 };
        Button cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 313, Top = 72, Width = 75 };
        form.Controls.Add(label);
        form.Controls.Add(textBox);
        form.Controls.Add(ok);
        form.Controls.Add(cancel);
        form.AcceptButton = ok;
        form.CancelButton = cancel;
        return form.ShowDialog(owner) == DialogResult.OK ? textBox.Text : null;
    }
}
