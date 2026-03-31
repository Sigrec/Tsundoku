namespace Tsundoku.Tests.Extensions;

[TestFixture]
public class ExtensionMethodTests
{
    #region NormalizeQuotes

    [Test]
    public void NormalizeQuotes_NullInput_ReturnsNull()
    {
        string? result = ((string?)null)!.NormalizeQuotes();
        Assert.That(result, Is.Null);
    }

    [Test]
    public void NormalizeQuotes_EmptyInput_ReturnsEmpty()
    {
        string result = string.Empty.NormalizeQuotes();
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void NormalizeQuotes_NoSmartQuotes_ReturnsSameString()
    {
        const string input = "Hello \"World\" it's fine";
        string result = input.NormalizeQuotes();
        Assert.That(result, Is.SameAs(input));
    }

    [Test]
    public void NormalizeQuotes_LeftDoubleQuote_ReplacedWithAscii()
    {
        string result = "\u201CHello\u201D".NormalizeQuotes();
        Assert.That(result, Is.EqualTo("\"Hello\""));
    }

    [Test]
    public void NormalizeQuotes_DoubleLow9QuotationMark_ReplacedWithAscii()
    {
        string result = "\u201EHello\u201F".NormalizeQuotes();
        Assert.That(result, Is.EqualTo("\"Hello\""));
    }

    [Test]
    public void NormalizeQuotes_LeftSingleQuote_ReplacedWithAscii()
    {
        string result = "\u2018it\u2019s".NormalizeQuotes();
        Assert.That(result, Is.EqualTo("'it's"));
    }

    [Test]
    public void NormalizeQuotes_SingleLow9AndHighReversed9_ReplacedWithAscii()
    {
        string result = "\u201Aword\u201B".NormalizeQuotes();
        Assert.That(result, Is.EqualTo("'word'"));
    }

    [Test]
    public void NormalizeQuotes_MixedSmartQuotes_AllReplaced()
    {
        string result = "\u201CHe said \u2018hello\u2019\u201D".NormalizeQuotes();
        Assert.That(result, Is.EqualTo("\"He said 'hello'\""));
    }

    [Test]
    public void NormalizeQuotes_PreservesNonQuoteCharacters()
    {
        string result = "abc\u201Cdef\u201Dghi".NormalizeQuotes();
        Assert.That(result, Is.EqualTo("abc\"def\"ghi"));
    }

    #endregion

    #region RemoveInPlaceCharArray

    [Test]
    public void RemoveInPlaceCharArray_EmptyString_ReturnsEmpty()
    {
        string result = ExtensionMethods.RemoveInPlaceCharArray(string.Empty);
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void RemoveInPlaceCharArray_NoWhitespace_ReturnsSameContent()
    {
        string result = ExtensionMethods.RemoveInPlaceCharArray("Hello");
        Assert.That(result, Is.EqualTo("Hello"));
    }

    [Test]
    public void RemoveInPlaceCharArray_AsciiSpaces_Removed()
    {
        string result = ExtensionMethods.RemoveInPlaceCharArray("Hello World");
        Assert.That(result, Is.EqualTo("HelloWorld"));
    }

    [Test]
    public void RemoveInPlaceCharArray_TabsAndNewlines_Removed()
    {
        string result = ExtensionMethods.RemoveInPlaceCharArray("A\tB\nC\rD");
        Assert.That(result, Is.EqualTo("ABCD"));
    }

    [Test]
    public void RemoveInPlaceCharArray_UnicodeWhitespace_Removed()
    {
        // \u00A0 = non-breaking space, \u3000 = ideographic space, \u2003 = em space
        string result = ExtensionMethods.RemoveInPlaceCharArray("A\u00A0B\u3000C\u2003D");
        Assert.That(result, Is.EqualTo("ABCD"));
    }

    [Test]
    public void RemoveInPlaceCharArray_OnlyWhitespace_ReturnsEmpty()
    {
        string result = ExtensionMethods.RemoveInPlaceCharArray(" \t\n\r");
        Assert.That(result, Is.EqualTo(string.Empty));
    }

    #endregion

    #region SimilarThreshold

    [Test]
    public void SimilarThreshold_InputLongerThanCompareTo_ReturnsCompareToLengthDividedBySix()
    {
        int result = ExtensionMethods.SimilarThreshold("LongerString", "Short");
        Assert.That(result, Is.EqualTo(5 / 6));
    }

    [Test]
    public void SimilarThreshold_InputShorterThanCompareTo_ReturnsInputLengthDividedBySix()
    {
        int result = ExtensionMethods.SimilarThreshold("Hi", "LongerString");
        Assert.That(result, Is.EqualTo(2 / 6));
    }

    [Test]
    public void SimilarThreshold_CompareToIsNull_ReturnsInputLengthDividedBySix()
    {
        int result = ExtensionMethods.SimilarThreshold("Twelve Chars", null!);
        Assert.That(result, Is.EqualTo(12 / 6));
    }

    [Test]
    public void SimilarThreshold_CompareToIsEmpty_ReturnsInputLengthDividedBySix()
    {
        int result = ExtensionMethods.SimilarThreshold("Twelve Chars", string.Empty);
        Assert.That(result, Is.EqualTo(12 / 6));
    }

    #endregion

    #region DictionariesEqual

    [Test]
    public void DictionariesEqual_BothNull_ReturnsTrue()
    {
        bool result = ExtensionMethods.DictionariesEqual<string, int>(null, null);
        Assert.That(result, Is.True);
    }

    [Test]
    public void DictionariesEqual_SameReference_ReturnsTrue()
    {
        var dict = new Dictionary<string, int> { { "a", 1 } };
        bool result = ExtensionMethods.DictionariesEqual(dict, dict);
        Assert.That(result, Is.True);
    }

    [Test]
    public void DictionariesEqual_OneNull_ReturnsFalse()
    {
        var dict = new Dictionary<string, int> { { "a", 1 } };
        Assert.That(ExtensionMethods.DictionariesEqual(dict, null), Is.False);
        Assert.That(ExtensionMethods.DictionariesEqual(null, dict), Is.False);
    }

    [Test]
    public void DictionariesEqual_DifferentCounts_ReturnsFalse()
    {
        var a = new Dictionary<string, int> { { "a", 1 } };
        var b = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        Assert.That(ExtensionMethods.DictionariesEqual(a, b), Is.False);
    }

    [Test]
    public void DictionariesEqual_SameKeysAndValues_ReturnsTrue()
    {
        var a = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        var b = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
        Assert.That(ExtensionMethods.DictionariesEqual(a, b), Is.True);
    }

    [Test]
    public void DictionariesEqual_SameKeysDifferentValues_ReturnsFalse()
    {
        var a = new Dictionary<string, int> { { "a", 1 } };
        var b = new Dictionary<string, int> { { "a", 99 } };
        Assert.That(ExtensionMethods.DictionariesEqual(a, b), Is.False);
    }

    [Test]
    public void DictionariesEqual_DifferentKeys_ReturnsFalse()
    {
        var a = new Dictionary<string, int> { { "a", 1 } };
        var b = new Dictionary<string, int> { { "z", 1 } };
        Assert.That(ExtensionMethods.DictionariesEqual(a, b), Is.False);
    }

    #endregion

    #region HashSetsEqual

    [Test]
    public void HashSetsEqual_BothNull_ReturnsTrue()
    {
        bool result = ExtensionMethods.HashSetsEqual<string>(null, null);
        Assert.That(result, Is.True);
    }

    [Test]
    public void HashSetsEqual_SameReference_ReturnsTrue()
    {
        var set = new HashSet<int> { 1, 2, 3 };
        Assert.That(ExtensionMethods.HashSetsEqual(set, set), Is.True);
    }

    [Test]
    public void HashSetsEqual_OneNull_ReturnsFalse()
    {
        var set = new HashSet<int> { 1 };
        Assert.That(ExtensionMethods.HashSetsEqual(set, null), Is.False);
        Assert.That(ExtensionMethods.HashSetsEqual(null, set), Is.False);
    }

    [Test]
    public void HashSetsEqual_SameElements_ReturnsTrue()
    {
        var a = new HashSet<int> { 1, 2, 3 };
        var b = new HashSet<int> { 3, 2, 1 };
        Assert.That(ExtensionMethods.HashSetsEqual(a, b), Is.True);
    }

    [Test]
    public void HashSetsEqual_DifferentElements_ReturnsFalse()
    {
        var a = new HashSet<int> { 1, 2 };
        var b = new HashSet<int> { 3, 4 };
        Assert.That(ExtensionMethods.HashSetsEqual(a, b), Is.False);
    }

    #endregion
}
