using Tsundoku.ViewModels;

namespace Tsundoku.Tests.ViewModels;

[TestFixture]
public class FilterChipViewModelTests
{

    [AvaloniaTest]
    public void Constructor_DefaultValues_SetsRatingGreaterThanOrEqualZero()
    {
        FilterChipViewModel chip = new();

        Assert.Multiple(() =>
        {
            Assert.That(chip.SelectedField, Is.EqualTo("Rating"));
            Assert.That(chip.SelectedOperator, Is.EqualTo(">="));
            Assert.That(chip.Value, Is.EqualTo("0"));
            Assert.That(chip.ShowConnector, Is.True);
        });
    }

    [AvaloniaTest]
    public void Constructor_CustomValues_SetsPropertiesCorrectly()
    {
        FilterChipViewModel chip = new("Value", "<=", "50");

        Assert.Multiple(() =>
        {
            Assert.That(chip.SelectedField, Is.EqualTo("Value"));
            Assert.That(chip.SelectedOperator, Is.EqualTo("<="));
            Assert.That(chip.Value, Is.EqualTo("50"));
        });
    }

    [AvaloniaTest]
    public void Constructor_NumericField_SetsIsValueFreeTextTrue()
    {
        FilterChipViewModel chip = new("Rating");

        Assert.That(chip.IsValueFreeText, Is.True);
    }

    [AvaloniaTest]
    public void Constructor_NumericFields_HaveAllNumericOperators()
    {
        string[] numericFields = ["Rating", "Value", "Read", "CurVolumes", "MaxVolumes"];
        string[] expectedOps = ["==", ">=", "<=", ">", "<"];

        foreach (string field in numericFields)
        {
            FilterChipViewModel chip = new(field);
            Assert.That(chip.AvailableOperators, Is.EqualTo(expectedOps), $"Failed for field: {field}");
        }
    }

    [AvaloniaTest]
    public void Constructor_EnumFields_HaveEqualityOnlyOperator()
    {
        string[] enumFields = ["Format", "Status", "Demographic", "Genre", "Series", "Favorite"];

        foreach (string field in enumFields)
        {
            FilterChipViewModel chip = new(field, "==", string.Empty);
            Assert.That(chip.AvailableOperators, Is.EqualTo(new[] { "==" }), $"Failed for field: {field}");
        }
    }

    [AvaloniaTest]
    public void Constructor_EnumFields_SetIsValueFreeTextFalse()
    {
        string[] enumFields = ["Format", "Status", "Demographic", "Genre", "Series", "Favorite"];

        foreach (string field in enumFields)
        {
            FilterChipViewModel chip = new(field, "==", string.Empty);
            Assert.That(chip.IsValueFreeText, Is.False, $"Failed for field: {field}");
        }
    }

    [AvaloniaTest]
    public void Constructor_TextFields_SetIsValueFreeTextTrue()
    {
        string[] textFields = ["Notes", "Publisher"];

        foreach (string field in textFields)
        {
            FilterChipViewModel chip = new(field, "==", string.Empty);
            Assert.That(chip.IsValueFreeText, Is.True, $"Failed for field: {field}");
        }
    }

    [AvaloniaTest]
    public void Constructor_EnumField_SetsAvailableValues()
    {
        FilterChipViewModel chip = new("Favorite", "==", "True");

        Assert.That(chip.AvailableValues, Is.EqualTo(new[] { "True", "False" }));
    }

    [AvaloniaTest]
    public void Constructor_SeriesField_HasCompleteAndInComplete()
    {
        FilterChipViewModel chip = new("Series", "==", "Complete");

        Assert.That(chip.AvailableValues, Is.EqualTo(new[] { "Complete", "InComplete" }));
    }

    [AvaloniaTest]
    public void ToQuerySegment_NumericField_ReturnsFieldOperatorValue()
    {
        FilterChipViewModel chip = new("Rating", ">=", "7");

        string result = chip.ToQuerySegment();

        Assert.That(result, Is.EqualTo("Rating>=7"));
    }

    [AvaloniaTest]
    public void ToQuerySegment_EnumField_ReturnsFieldEqualsValue()
    {
        FilterChipViewModel chip = new("Format", "==", "Manga");

        string result = chip.ToQuerySegment();

        Assert.That(result, Is.EqualTo("Format==Manga"));
    }

    [AvaloniaTest]
    public void ToQuerySegment_FreeTextWithSpace_WrapsValueInQuotes()
    {
        FilterChipViewModel chip = new("Notes", "==");
        chip.Value = "some notes here";

        string result = chip.ToQuerySegment();

        Assert.That(result, Is.EqualTo("Notes==\"some notes here\""));
    }

    [AvaloniaTest]
    public void ToQuerySegment_FreeTextWithoutSpace_DoesNotWrapInQuotes()
    {
        FilterChipViewModel chip = new("Notes", "==");
        chip.Value = "important";

        string result = chip.ToQuerySegment();

        Assert.That(result, Is.EqualTo("Notes==important"));
    }

    [AvaloniaTest]
    public void ToQuerySegment_PublisherWithSpace_WrapsValueInQuotes()
    {
        FilterChipViewModel chip = new("Publisher", "==");
        chip.Value = "Yen Press";

        string result = chip.ToQuerySegment();

        Assert.That(result, Is.EqualTo("Publisher==\"Yen Press\""));
    }

    [AvaloniaTest]
    public void ToQuerySegment_NumericFieldWithSpace_DoesNotWrapInQuotes()
    {
        // Numeric fields should not quote even if value somehow has a space
        FilterChipViewModel chip = new("Rating", ">=", "7");

        string result = chip.ToQuerySegment();

        Assert.That(result, Is.EqualTo("Rating>=7"));
    }

    [AvaloniaTest]
    public void ToggleConnector_InitiallyFalse_BecomesTrue()
    {
        FilterChipViewModel chip = new();

        Assert.That(chip.IsConnectorOr, Is.False);

        chip.ToggleConnector();

        Assert.That(chip.IsConnectorOr, Is.True);
    }

    [AvaloniaTest]
    public void ToggleConnector_CalledTwice_ReturnsFalse()
    {
        FilterChipViewModel chip = new();

        chip.ToggleConnector();
        chip.ToggleConnector();

        Assert.That(chip.IsConnectorOr, Is.False);
    }

    [AvaloniaTest]
    public void Constructor_InvalidOperatorForEnumField_ResetsToEquals()
    {
        // Passing ">=" which is only valid for numeric fields
        FilterChipViewModel chip = new("Format", ">=", "Manga");

        Assert.That(chip.SelectedOperator, Is.EqualTo("=="));
    }

    [AvaloniaTest]
    public void Constructor_EnumFieldWithInvalidValue_ResetsToFirstAvailableValue()
    {
        FilterChipViewModel chip = new("Favorite", "==", "InvalidValue");

        Assert.That(chip.AvailableValues, Does.Contain(chip.Value));
    }

    [AvaloniaTest]
    public void AllFields_ContainsExpectedFields()
    {
        string[] expected = ["Rating", "Value", "Read", "CurVolumes", "MaxVolumes",
            "Format", "Status", "Demographic", "Series", "Favorite", "Genre",
            "Notes", "Publisher"];

        Assert.That(FilterChipViewModel.AllFields, Is.EqualTo(expected));
    }
}
