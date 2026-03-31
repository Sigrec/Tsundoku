using System.Runtime.Serialization;
using static Tsundoku.Models.Enums.TsundokuSortModel;

namespace Tsundoku.Tests.Helpers;

[TestFixture]
public class EnumExtensionsTests
{
    #region GetEnumMemberValue

    [Test]
    public void GetEnumMemberValue_EnumWithMatchingAttribute_ReturnsAttributeValue()
    {
        string result = TsundokuSort.TitleAZ.GetEnumMemberValue();
        Assert.That(result, Is.EqualTo("Title A-Z"));
    }

    [Test]
    public void GetEnumMemberValue_EnumWithSpacesInValue_ReturnsAttributeValue()
    {
        string result = TsundokuSort.VolumeCount.GetEnumMemberValue();
        Assert.That(result, Is.EqualTo("Volume Count"));
    }

    [Test]
    public void GetEnumMemberValue_EnumWhereValueMatchesName_ReturnsValue()
    {
        string result = TsundokuSort.Rating.GetEnumMemberValue();
        Assert.That(result, Is.EqualTo("Rating"));
    }

    [Test]
    public void GetEnumMemberValue_AllSortValues_NoneAreEmpty()
    {
        foreach (TsundokuSort sort in Enum.GetValues<TsundokuSort>())
        {
            string result = sort.GetEnumMemberValue();
            Assert.That(result, Is.Not.Empty, $"EnumMemberValue for {sort} should not be empty");
        }
    }

    #endregion

    #region GetEnumValueFromMemberValue (throwing overload)

    [Test]
    public void GetEnumValueFromMemberValue_ExactMatch_ReturnsCorrectEnum()
    {
        TsundokuSort result = "Title A-Z".GetEnumValueFromMemberValue<TsundokuSort>();
        Assert.That(result, Is.EqualTo(TsundokuSort.TitleAZ));
    }

    [Test]
    public void GetEnumValueFromMemberValue_CaseInsensitiveMatch_ReturnsCorrectEnum()
    {
        TsundokuSort result = "title a-z".GetEnumValueFromMemberValue<TsundokuSort>();
        Assert.That(result, Is.EqualTo(TsundokuSort.TitleAZ));
    }

    [Test]
    public void GetEnumValueFromMemberValue_CaseSensitiveExactMatch_ReturnsCorrectEnum()
    {
        TsundokuSort result = "Volume Count".GetEnumValueFromMemberValue<TsundokuSort>(ignoreCase: false);
        Assert.That(result, Is.EqualTo(TsundokuSort.VolumeCount));
    }

    [Test]
    public void GetEnumValueFromMemberValue_CaseSensitiveMismatch_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            "volume count".GetEnumValueFromMemberValue<TsundokuSort>(ignoreCase: false));
    }

    [Test]
    public void GetEnumValueFromMemberValue_NoMatch_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            "NonExistentValue".GetEnumValueFromMemberValue<TsundokuSort>());
    }

    [Test]
    public void GetEnumValueFromMemberValue_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            string.Empty.GetEnumValueFromMemberValue<TsundokuSort>());
    }

    [Test]
    public void GetEnumValueFromMemberValue_SimpleNameMatch_ReturnsCorrectEnum()
    {
        TsundokuSort result = "Rating".GetEnumValueFromMemberValue<TsundokuSort>();
        Assert.That(result, Is.EqualTo(TsundokuSort.Rating));
    }

    #endregion

    #region GetEnumValueFromMemberValue (default value overload)

    [Test]
    public void GetEnumValueFromMemberValueWithDefault_ExactMatch_ReturnsCorrectEnum()
    {
        TsundokuSort result = "Title Z-A".GetEnumValueFromMemberValue(TsundokuSort.TitleAZ);
        Assert.That(result, Is.EqualTo(TsundokuSort.TitleZA));
    }

    [Test]
    public void GetEnumValueFromMemberValueWithDefault_NoMatch_ReturnsDefaultValue()
    {
        TsundokuSort result = "NonExistent".GetEnumValueFromMemberValue(TsundokuSort.Rating);
        Assert.That(result, Is.EqualTo(TsundokuSort.Rating));
    }

    [Test]
    public void GetEnumValueFromMemberValueWithDefault_CaseInsensitiveMatch_ReturnsCorrectEnum()
    {
        TsundokuSort result = "UNREAD".GetEnumValueFromMemberValue(TsundokuSort.TitleAZ);
        Assert.That(result, Is.EqualTo(TsundokuSort.Unread));
    }

    [Test]
    public void GetEnumValueFromMemberValueWithDefault_CaseSensitiveMismatch_ReturnsDefault()
    {
        TsundokuSort result = "unread".GetEnumValueFromMemberValue(TsundokuSort.TitleAZ, ignoreCase: false);
        Assert.That(result, Is.EqualTo(TsundokuSort.TitleAZ));
    }

    [Test]
    public void GetEnumValueFromMemberValueWithDefault_EmptyString_ReturnsDefault()
    {
        TsundokuSort result = string.Empty.GetEnumValueFromMemberValue(TsundokuSort.Read);
        Assert.That(result, Is.EqualTo(TsundokuSort.Read));
    }

    #endregion

    #region TryGetEnumValueFromMemberValue

    [Test]
    public void TryGetEnumValueFromMemberValue_ExactMatch_ReturnsTrueAndCorrectValue()
    {
        bool success = "Volume Count".TryGetEnumValueFromMemberValue<TsundokuSort>(out TsundokuSort result);
        Assert.That(success, Is.True);
        Assert.That(result, Is.EqualTo(TsundokuSort.VolumeCount));
    }

    [Test]
    public void TryGetEnumValueFromMemberValue_CaseInsensitiveMatch_ReturnsTrueAndCorrectValue()
    {
        bool success = "value".TryGetEnumValueFromMemberValue<TsundokuSort>(out TsundokuSort result);
        Assert.That(success, Is.True);
        Assert.That(result, Is.EqualTo(TsundokuSort.Value));
    }

    [Test]
    public void TryGetEnumValueFromMemberValue_NoMatch_ReturnsFalseAndDefault()
    {
        bool success = "DoesNotExist".TryGetEnumValueFromMemberValue<TsundokuSort>(out TsundokuSort result);
        Assert.That(success, Is.False);
        Assert.That(result, Is.EqualTo(default(TsundokuSort)));
    }

    [Test]
    public void TryGetEnumValueFromMemberValue_CaseSensitiveExactMatch_ReturnsTrueAndCorrectValue()
    {
        bool success = "Read".TryGetEnumValueFromMemberValue<TsundokuSort>(out TsundokuSort result, ignoreCase: false);
        Assert.That(success, Is.True);
        Assert.That(result, Is.EqualTo(TsundokuSort.Read));
    }

    [Test]
    public void TryGetEnumValueFromMemberValue_CaseSensitiveMismatch_ReturnsFalse()
    {
        bool success = "read".TryGetEnumValueFromMemberValue<TsundokuSort>(out TsundokuSort _, ignoreCase: false);
        Assert.That(success, Is.False);
    }

    [Test]
    public void TryGetEnumValueFromMemberValue_EmptyString_ReturnsFalse()
    {
        bool success = string.Empty.TryGetEnumValueFromMemberValue<TsundokuSort>(out TsundokuSort _);
        Assert.That(success, Is.False);
    }

    #endregion

    #region Roundtrip

    [Test]
    public void Roundtrip_AllSortValues_ConvertToStringAndBack()
    {
        foreach (TsundokuSort original in Enum.GetValues<TsundokuSort>())
        {
            string memberValue = original.GetEnumMemberValue();
            TsundokuSort roundtripped = memberValue.GetEnumValueFromMemberValue<TsundokuSort>();
            Assert.That(roundtripped, Is.EqualTo(original), $"Roundtrip failed for {original} (member value: '{memberValue}')");
        }
    }

    #endregion
}
