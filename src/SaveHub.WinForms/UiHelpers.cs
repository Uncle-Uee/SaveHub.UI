using System.Drawing.Drawing2D;

namespace SaveHub.WinForms;

/// <summary>Shared UI helpers used across the tab user controls.</summary>
internal static class UiHelpers
{
    private static Image? _coverPlaceholder;

    /// <summary>A reusable "no cover" placeholder shown when no cover art is available.</summary>
    public static Image CoverPlaceholder()
    {
        if (_coverPlaceholder is not null)
        {
            return _coverPlaceholder;
        }
        Bitmap bitmap = new Bitmap(256, 256);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.FromArgb(245, 245, 248));
            using Pen border = new Pen(Color.FromArgb(206, 206, 214), 4f);
            graphics.DrawRectangle(border, 8, 8, 239, 239);
            using SolidBrush glyph = new SolidBrush(Color.FromArgb(198, 198, 208));
            graphics.FillEllipse(glyph, 70, 74, 40, 40);
            Point[] mountains = [new Point(48, 190), new Point(112, 118), new Point(150, 162), new Point(182, 128), new Point(212, 190)];
            graphics.FillPolygon(glyph, mountains);
            using Font font = new Font("Segoe UI", 18f, FontStyle.Bold);
            using SolidBrush textBrush = new SolidBrush(Color.FromArgb(130, 130, 142));
            using StringFormat format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            graphics.DrawString("No Cover", font, textBrush, new RectangleF(0, 198, 256, 46), format);
        }
        _coverPlaceholder = bitmap;
        return _coverPlaceholder;
    }

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
