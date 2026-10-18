namespace OsrsWorldPing.Core;

/// <summary>Provisional design dimensions in Windows device-independent pixels.</summary>
public static class FixedWindowLayout
{
    public const double DesignWidth = 768;
    public const double DesignHeight = 930;

    public static (double Width, double Height, double Scale) Fit(double workWidth, double workHeight)
    {
        if (!double.IsFinite(workWidth) || workWidth <= 0) throw new ArgumentOutOfRangeException(nameof(workWidth));
        if (!double.IsFinite(workHeight) || workHeight <= 0) throw new ArgumentOutOfRangeException(nameof(workHeight));
        double scale = Math.Min(1, Math.Min(workWidth / DesignWidth, workHeight / DesignHeight));
        return (DesignWidth * scale, DesignHeight * scale, scale);
    }
}
