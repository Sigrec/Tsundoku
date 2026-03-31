using System.Reflection;
using Avalonia.Media;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ThemeResourceKeysTests
{
    private static readonly FieldInfo[] ConstantFields =
        typeof(ThemeResourceKeys)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .ToArray();

    [Test]
    public void PropertyMap_ContainsAllExpectedKeys()
    {
        string[] expectedKeys = ConstantFields
            .Select(f => (string)f.GetRawConstantValue()!)
            .ToArray();

        foreach (string key in expectedKeys)
        {
            Assert.That(
                ThemeResourceKeys.PropertyMap.ContainsKey(key),
                Is.True,
                $"PropertyMap should contain key '{key}'"
            );
        }
    }

    [Test]
    public void PropertyMap_KeyCount_MatchesConstantCount()
    {
        Assert.That(
            ThemeResourceKeys.PropertyMap.Count,
            Is.EqualTo(ConstantFields.Length),
            "PropertyMap count should match the number of string constants defined in ThemeResourceKeys"
        );
    }

    [Test]
    public void PropertyMap_HasNoDuplicateKeys()
    {
        string[] keys = ThemeResourceKeys.PropertyMap.Keys.ToArray();
        string[] distinctKeys = keys.Distinct().ToArray();

        Assert.That(keys.Length, Is.EqualTo(distinctKeys.Length), "PropertyMap should have no duplicate keys");
    }

    [Test]
    public void PropertyMap_GettersReturnCorrectProperties()
    {
        TsundokuTheme theme = TsundokuTheme.DEFAULT_THEME;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuBGColor](theme), Is.SameAs(theme.MenuBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.UsernameColor](theme), Is.SameAs(theme.UsernameColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuTextColor](theme), Is.SameAs(theme.MenuTextColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SearchBarBGColor](theme), Is.SameAs(theme.SearchBarBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SearchBarBorderColor](theme), Is.SameAs(theme.SearchBarBorderColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SearchBarTextColor](theme), Is.SameAs(theme.SearchBarTextColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.DividerColor](theme), Is.SameAs(theme.DividerColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuButtonBGColor](theme), Is.SameAs(theme.MenuButtonBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuButtonBGHoverColor](theme), Is.SameAs(theme.MenuButtonBGHoverColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuButtonBorderColor](theme), Is.SameAs(theme.MenuButtonBorderColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuButtonBorderHoverColor](theme), Is.SameAs(theme.MenuButtonBorderHoverColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuButtonTextAndIconColor](theme), Is.SameAs(theme.MenuButtonTextAndIconColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.MenuButtonTextAndIconHoverColor](theme), Is.SameAs(theme.MenuButtonTextAndIconHoverColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.CollectionBGColor](theme), Is.SameAs(theme.CollectionBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.StatusAndBookTypeBGColor](theme), Is.SameAs(theme.StatusAndBookTypeBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.StatusAndBookTypeTextColor](theme), Is.SameAs(theme.StatusAndBookTypeTextColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardBGColor](theme), Is.SameAs(theme.SeriesCardBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardTitleColor](theme), Is.SameAs(theme.SeriesCardTitleColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardPublisherColor](theme), Is.SameAs(theme.SeriesCardPublisherColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardStaffColor](theme), Is.SameAs(theme.SeriesCardStaffColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardDescColor](theme), Is.SameAs(theme.SeriesCardDescColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardBorderColor](theme), Is.SameAs(theme.SeriesCardBorderColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesProgressBarColor](theme), Is.SameAs(theme.SeriesProgressBarColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesProgressBarBGColor](theme), Is.SameAs(theme.SeriesProgressBarBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesProgressBarBorderColor](theme), Is.SameAs(theme.SeriesProgressBarBorderColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesProgressTextColor](theme), Is.SameAs(theme.SeriesProgressTextColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesButtonIconColor](theme), Is.SameAs(theme.SeriesButtonIconColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesButtonIconHoverColor](theme), Is.SameAs(theme.SeriesButtonIconHoverColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.StatusAndBookTypeBorderColor](theme), Is.SameAs(theme.StatusAndBookTypeBorderColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardDividerColor](theme), Is.SameAs(theme.SeriesCardDividerColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCoverBGColor](theme), Is.SameAs(theme.SeriesCoverBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardButtonBGColor](theme), Is.SameAs(theme.SeriesCardButtonBGColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardButtonBGHoverColor](theme), Is.SameAs(theme.SeriesCardButtonBGHoverColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardButtonBorderColor](theme), Is.SameAs(theme.SeriesCardButtonBorderColor));
            Assert.That(ThemeResourceKeys.PropertyMap[ThemeResourceKeys.SeriesCardButtonBorderHoverColor](theme), Is.SameAs(theme.SeriesCardButtonBorderHoverColor));
        }
    }

    [Test]
    public void PropertyMap_AllGettersReturnNonNull_ForDefaultTheme()
    {
        TsundokuTheme theme = TsundokuTheme.DEFAULT_THEME;

        foreach (var kvp in ThemeResourceKeys.PropertyMap)
        {
            SolidColorBrush result = kvp.Value(theme);
            Assert.That(result, Is.Not.Null, $"Getter for key '{kvp.Key}' returned null for DEFAULT_THEME");
        }
    }

    [Test]
    public void AllConstants_StartWithTsundokuPrefix()
    {
        foreach (FieldInfo field in ConstantFields)
        {
            string value = (string)field.GetRawConstantValue()!;
            Assert.That(
                value.StartsWith("Tsundoku", StringComparison.Ordinal),
                Is.True,
                $"Constant '{field.Name}' value '{value}' should start with 'Tsundoku' prefix"
            );
        }
    }
}
