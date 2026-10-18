using OsrsWorldPing.Core;

namespace OsrsWorldPing.Core.Tests;

public sealed class FixedWindowLayoutTests
{
    [Theory]
    [InlineData(1920, 1040, 1)]
    [InlineData(768, 930, 1)]
    [InlineData(1366, 728, 728d / 930)]
    [InlineData(512, 688, 512d / 768)]
    [InlineData(960, 500, 500d / 930)]
    public void FitsProportionallyWithoutEnlargingTheDesign(double width, double height, double scale)
    {
        var fit = FixedWindowLayout.Fit(width, height);
        Assert.Equal(scale, fit.Scale, 8);
        Assert.Equal(768d / 930, fit.Width / fit.Height, 8);
        Assert.True(fit.Width <= width && fit.Height <= height);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    [InlineData(2)]
    public void WindowsScalingKeepsTheSameAspectAndFitsThePhysicalWorkArea(double dpiScale)
    {
        var fit = FixedWindowLayout.Fit(1920 / dpiScale, 1040 / dpiScale);
        Assert.InRange(fit.Width * dpiScale, 1, 1920);
        Assert.InRange(fit.Height * dpiScale, 1, 1040);
        Assert.Equal(768d / 930, fit.Width / fit.Height, 8);
    }

    [Theory]
    [InlineData(0, 930)]
    [InlineData(768, -1)]
    [InlineData(double.NaN, 930)]
    [InlineData(768, double.PositiveInfinity)]
    public void RejectsInvalidWorkAreas(double width, double height) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => FixedWindowLayout.Fit(width, height));
}
