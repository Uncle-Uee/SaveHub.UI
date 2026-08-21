using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace SaveHub.Avalonia.Common;

/// <summary>Provides a generated "No Cover" placeholder image for the Upload cover preview.</summary>
internal static class CoverImages
{
    private static Bitmap? _placeholder;

    /// <summary>A reusable placeholder shown when no cover art is available.</summary>
    public static Bitmap Placeholder()
    {
        if (_placeholder is not null)
        {
            return _placeholder;
        }
        RenderTargetBitmap bitmap = new RenderTargetBitmap(new PixelSize(256, 256), new Vector(96, 96));
        using (DrawingContext context = bitmap.CreateDrawingContext())
        {
            SolidColorBrush glyph = new SolidColorBrush(Color.FromRgb(198, 198, 208));
            SolidColorBrush textBrush = new SolidColorBrush(Color.FromRgb(130, 130, 142));
            Pen border = new Pen(new SolidColorBrush(Color.FromRgb(206, 206, 214)), 4);

            context.FillRectangle(new SolidColorBrush(Color.FromRgb(245, 245, 248)), new Rect(0, 0, 256, 256));
            context.DrawRectangle(null, border, new Rect(6, 6, 244, 244));
            context.DrawEllipse(glyph, null, new Point(90, 94), 20, 20);

            StreamGeometry mountains = new StreamGeometry();
            using (StreamGeometryContext geometry = mountains.Open())
            {
                geometry.BeginFigure(new Point(48, 190), true);
                geometry.LineTo(new Point(112, 118));
                geometry.LineTo(new Point(150, 162));
                geometry.LineTo(new Point(182, 128));
                geometry.LineTo(new Point(212, 190));
                geometry.EndFigure(true);
            }
            context.DrawGeometry(glyph, null, mountains);

            FormattedText text = new FormattedText("No Cover", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, Typeface.Default, 22, textBrush);
            context.DrawText(text, new Point((256 - text.Width) / 2, 196));
        }
        _placeholder = bitmap;
        return _placeholder;
    }

    /// <summary>Loads a bitmap from a file, or null on failure.</summary>
    public static Bitmap? TryLoad(string path)
    {
        try
        {
            return new Bitmap(path);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Loads a bitmap from image bytes, or null on failure.</summary>
    public static Bitmap? TryLoad(byte[] bytes)
    {
        try
        {
            using MemoryStream stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch
        {
            return null;
        }
    }
}
