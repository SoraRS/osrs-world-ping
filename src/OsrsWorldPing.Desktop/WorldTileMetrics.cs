using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace OsrsWorldPing.Desktop;

// Measure text for the complete virtualized catalogue without constructing offscreen card controls.
internal static class WorldTileMetrics
{
    private static readonly Typeface Regular = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
    private static readonly Typeface Semibold = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

    public static double Width(WorldCardModel world, double maximum, double pixelsPerDip,
        Dictionary<(string Text, double Size, bool Bold), double> cache)
    {
        double TextWidth(string text, double size, bool bold = false)
        {
            var key = (text, size, bold);
            if (!cache.TryGetValue(key, out double width))
                cache[key] = width = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    bold ? Semibold : Regular, size, Brushes.White, pixelsPerDip).WidthIncludingTrailingWhitespace;
            return width;
        }
        double reading = 64;
        double firstLine = 24 + TextWidth(world.Id.ToString(CultureInfo.CurrentCulture), 16, true) + 6 + reading;
        double secondLine = 23 + TextWidth(world.ActivityLabel, 12);
        return Math.Min(maximum, 16 + Math.Max(firstLine, secondLine));
    }
}
