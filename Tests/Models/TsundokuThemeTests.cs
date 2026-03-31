using System.Reflection;
using System.Text.Json;
using Avalonia.Media;
using static Tsundoku.Models.TsundokuTheme;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class TsundokuThemeTests
{
    private static readonly PropertyInfo[] BrushProperties =
        typeof(TsundokuTheme)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(SolidColorBrush))
            .ToArray();

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new SolidColorBrushJsonConverter() },
        PropertyNameCaseInsensitive = true,
    };

    [Test]
    public void DefaultTheme_HasNoNullProperties()
    {
        TsundokuTheme theme = TsundokuTheme.DEFAULT_THEME;

        Assert.That(theme.ThemeName, Is.Not.Null.And.Not.Empty, "ThemeName should not be null or empty");

        foreach (PropertyInfo prop in BrushProperties)
        {
            object? value = prop.GetValue(theme);
            Assert.That(value, Is.Not.Null, $"{prop.Name} should not be null on DEFAULT_THEME");
        }
    }

    [Test]
    public void Cloning_ProducesEqualButSeparateInstance()
    {
        TsundokuTheme original = TsundokuTheme.DEFAULT_THEME;
        TsundokuTheme clone = original.Cloning();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(clone, Is.EqualTo(original), "Clone should be equal to the original");
            Assert.That(clone, Is.Not.SameAs(original), "Clone should not be the same reference as the original");
        }

        // Verify brush instances are different objects
        foreach (PropertyInfo prop in BrushProperties)
        {
            SolidColorBrush? originalBrush = (SolidColorBrush?)prop.GetValue(original);
            SolidColorBrush? cloneBrush = (SolidColorBrush?)prop.GetValue(clone);

            Assert.That(cloneBrush, Is.Not.SameAs(originalBrush), $"{prop.Name} brush should be a separate instance");
        }
    }

    [Test]
    public void Equals_SameValues_ReturnsTrue()
    {
        TsundokuTheme a = TsundokuTheme.DEFAULT_THEME.Cloning();
        TsundokuTheme b = TsundokuTheme.DEFAULT_THEME.Cloning();

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_DifferentThemeName_ReturnsFalse()
    {
        TsundokuTheme a = TsundokuTheme.DEFAULT_THEME.Cloning();
        TsundokuTheme b = TsundokuTheme.DEFAULT_THEME.Cloning();
        b.ThemeName = "Different";

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Equals_DifferentColor_ReturnsFalse()
    {
        TsundokuTheme a = TsundokuTheme.DEFAULT_THEME.Cloning();
        TsundokuTheme b = TsundokuTheme.DEFAULT_THEME.Cloning();
        b.MenuBGColor = new SolidColorBrush(Colors.Red);

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Equals_Null_ReturnsFalse()
    {
        TsundokuTheme theme = TsundokuTheme.DEFAULT_THEME;

        Assert.That(theme.Equals(null), Is.False);
    }

    [Test]
    public void Equals_SameReference_ReturnsTrue()
    {
        TsundokuTheme theme = TsundokuTheme.DEFAULT_THEME;

        Assert.That(theme.Equals(theme), Is.True);
    }

    [Test]
    public void Equals_DifferentType_ReturnsFalse()
    {
        TsundokuTheme theme = TsundokuTheme.DEFAULT_THEME;

        Assert.That(theme.Equals("not a theme"), Is.False);
    }

    [Test]
    public void GetHashCode_EqualObjects_ReturnSameHash()
    {
        TsundokuTheme a = TsundokuTheme.DEFAULT_THEME.Cloning();
        TsundokuTheme b = TsundokuTheme.DEFAULT_THEME.Cloning();

        Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void GetHashCode_IsConsistent_AcrossMultipleCalls()
    {
        TsundokuTheme theme = TsundokuTheme.DEFAULT_THEME;

        int hash1 = theme.GetHashCode();
        int hash2 = theme.GetHashCode();

        Assert.That(hash1, Is.EqualTo(hash2));
    }

    [Test]
    public void GetHashCode_DifferentObjects_ReturnDifferentHash()
    {
        TsundokuTheme a = TsundokuTheme.DEFAULT_THEME.Cloning();
        TsundokuTheme b = TsundokuTheme.DEFAULT_THEME.Cloning();
        b.MenuBGColor = new SolidColorBrush(Colors.Red);

        Assert.That(a.GetHashCode(), Is.Not.EqualTo(b.GetHashCode()));
    }

    [Test]
    public void JsonRoundTrip_PreservesAllProperties()
    {
        TsundokuTheme original = TsundokuTheme.DEFAULT_THEME.Cloning();

        string json = JsonSerializer.Serialize(original, SerializerOptions);
        TsundokuTheme? deserialized = JsonSerializer.Deserialize<TsundokuTheme>(json, SerializerOptions);

        Assert.That(deserialized, Is.Not.Null, "Deserialized theme should not be null");
        Assert.That(deserialized!.ThemeName, Is.EqualTo(original.ThemeName), "ThemeName should survive round-trip");

        foreach (PropertyInfo prop in BrushProperties)
        {
            SolidColorBrush? originalBrush = (SolidColorBrush?)prop.GetValue(original);
            SolidColorBrush? deserializedBrush = (SolidColorBrush?)prop.GetValue(deserialized);

            Assert.That(
                deserializedBrush?.Color,
                Is.EqualTo(originalBrush?.Color),
                $"{prop.Name} color should survive round-trip"
            );
        }
    }

    [Test]
    public void SolidColorBrushJsonConverter_NullHex_ReturnsNull()
    {
        string json = "null";
        SolidColorBrushJsonConverter converter = new();

        Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        SolidColorBrush? result = converter.Read(ref reader, typeof(SolidColorBrush), new JsonSerializerOptions());

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SolidColorBrushJsonConverter_EmptyString_ReturnsNull()
    {
        string json = "\"\"";
        SolidColorBrushJsonConverter converter = new();

        Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        SolidColorBrush? result = converter.Read(ref reader, typeof(SolidColorBrush), new JsonSerializerOptions());

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SolidColorBrushJsonConverter_WhitespaceString_ReturnsNull()
    {
        string json = "\"   \"";
        SolidColorBrushJsonConverter converter = new();

        Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        SolidColorBrush? result = converter.Read(ref reader, typeof(SolidColorBrush), new JsonSerializerOptions());

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SolidColorBrushJsonConverter_InvalidHex_ReturnsNull()
    {
        string json = "\"not-a-color\"";
        SolidColorBrushJsonConverter converter = new();

        Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        SolidColorBrush? result = converter.Read(ref reader, typeof(SolidColorBrush), new JsonSerializerOptions());

        Assert.That(result, Is.Null);
    }

    [Test]
    public void SolidColorBrushJsonConverter_ValidHex_ReturnsBrush()
    {
        string json = "\"#ff20232d\"";
        SolidColorBrushJsonConverter converter = new();

        Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(json));
        reader.Read();

        SolidColorBrush? result = converter.Read(ref reader, typeof(SolidColorBrush), new JsonSerializerOptions());

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Color, Is.EqualTo(Color.Parse("#ff20232d")));
    }

    [Test]
    public void SolidColorBrushJsonConverter_WriteNull_WritesNullValue()
    {
        SolidColorBrushJsonConverter converter = new();
        using System.IO.MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);

        converter.Write(writer, null!, new JsonSerializerOptions());
        writer.Flush();

        string json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.That(json, Is.EqualTo("null"));
    }

    [Test]
    public void SolidColorBrushJsonConverter_WriteValidBrush_WritesColorString()
    {
        SolidColorBrushJsonConverter converter = new();
        SolidColorBrush brush = new(Color.Parse("#ff20232d"));
        using System.IO.MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream);

        converter.Write(writer, brush, new JsonSerializerOptions());
        writer.Flush();

        string json = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.That(json, Does.Contain("20232D").IgnoreCase);
    }

    [Test]
    public void CompareTo_OrdersByThemeName()
    {
        TsundokuTheme a = new("Alpha");
        TsundokuTheme b = new("Beta");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a.CompareTo(b), Is.LessThan(0));
            Assert.That(b.CompareTo(a), Is.GreaterThan(0));
            Assert.That(a.CompareTo(a), Is.EqualTo(0));
        }
    }

    [Test]
    public void CompareTo_NonThemeObject_ReturnsOne()
    {
        TsundokuTheme theme = new("Test");

        Assert.That(theme.CompareTo("not a theme"), Is.EqualTo(1));
    }
}
