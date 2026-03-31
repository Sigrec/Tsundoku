using System.Globalization;
using Avalonia.Media;
using Tsundoku.Converters;

namespace Tsundoku.Tests.Converters;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class BrushToColorConverterTests
{
    private static readonly BrushToColorConverter Converter = BrushToColorConverter.Instance;

    [Test]
    public void Convert_SolidColorBrush_ReturnsItsColor()
    {
        SolidColorBrush brush = new(Colors.Red);

        object result = Converter.Convert(brush, typeof(Color), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(Colors.Red));
    }

    [Test]
    public void Convert_Color_ReturnsSameColor()
    {
        Color color = Colors.Blue;

        object result = Converter.Convert(color, typeof(Color), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(Colors.Blue));
    }

    [Test]
    public void Convert_NullValue_ReturnsTransparent()
    {
        object result = Converter.Convert(null!, typeof(Color), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(Colors.Transparent));
    }

    [Test]
    public void Convert_UnsupportedType_ReturnsTransparent()
    {
        object result = Converter.Convert("not a brush", typeof(Color), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(Colors.Transparent));
    }

    [Test]
    public void ConvertBack_SolidColorBrush_ReturnsSameBrush()
    {
        SolidColorBrush brush = new(Colors.Green);

        object result = Converter.ConvertBack(brush, typeof(ISolidColorBrush), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.SameAs(brush));
    }

    [Test]
    public void ConvertBack_Color_ReturnsNewSolidColorBrush()
    {
        Color color = Colors.Purple;

        object result = Converter.ConvertBack(color, typeof(ISolidColorBrush), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.TypeOf<SolidColorBrush>());
        Assert.That(((SolidColorBrush)result).Color, Is.EqualTo(Colors.Purple));
    }

    [Test]
    public void ConvertBack_NullValue_ReturnsTransparentBrush()
    {
        object result = Converter.ConvertBack(null!, typeof(ISolidColorBrush), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.TypeOf<SolidColorBrush>());
        Assert.That(((SolidColorBrush)result).Color, Is.EqualTo(Colors.Transparent));
    }

    [Test]
    public void ConvertBack_UnsupportedType_ReturnsTransparentBrush()
    {
        object result = Converter.ConvertBack(42, typeof(ISolidColorBrush), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.TypeOf<SolidColorBrush>());
        Assert.That(((SolidColorBrush)result).Color, Is.EqualTo(Colors.Transparent));
    }

    [Test]
    public void Convert_CustomColor_ReturnsCorrectColor()
    {
        Color custom = Color.FromArgb(128, 64, 32, 16);
        SolidColorBrush brush = new(custom);

        object result = Converter.Convert(brush, typeof(Color), null!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(custom));
    }
}
