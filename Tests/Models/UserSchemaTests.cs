using System.Text.Json.Nodes;

namespace Tsundoku.Tests.Models;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class UserSchemaTests
{
    private static readonly string[] RemovedKeys =
    [
        "SeriesProgressBGColor", "SeriesEditPaneBGColor",
        "SeriesEditPaneButtonsBGColor", "SeriesEditPaneButtonsBGHoverColor",
        "SeriesEditPaneButtonsBorderColor", "SeriesEditPaneButtonsBorderHoverColor",
        "SeriesEditPaneButtonsIconColor", "SeriesEditPaneButtonsIconHoverColor",
        "UserIconBorderColor", "StatusAndBookTypeBGHoverColor",
        "StatusAndBookTypeTextHoverColor", "SeriesProgressButtonsHoverColor",
        "SeriesNotesBGColor", "SeriesNotesBorderColor", "SeriesNotesTextColor"
    ];

    private static readonly string[] AddedKeys =
    [
        "SeriesCardBorderColor", "StatusAndBookTypeBorderColor",
        "SeriesCoverBGColor", "SeriesCardButtonBGColor",
        "SeriesCardDividerColor", "SeriesCardButtonBGHoverColor",
        "SeriesCardButtonBorderColor", "SeriesCardButtonBorderHoverColor"
    ];

    [Test]
    public void V62Migration_RemovesAll15OldThemeColorKeys()
    {
        JsonNode userData = CreatePreV62UserData();

        User.UpdateSchemaVersion(userData, isImport: true);

        JsonObject theme = userData["SavedThemes"]!.AsArray()[0]!.AsObject();
        foreach (string key in RemovedKeys)
        {
            Assert.That(theme.ContainsKey(key), Is.False, $"Expected key '{key}' to be removed");
        }
    }

    [Test]
    public void V62Migration_AddsAll8NewThemeColorKeys()
    {
        JsonNode userData = CreatePreV62UserData();

        User.UpdateSchemaVersion(userData, isImport: true);

        JsonObject theme = userData["SavedThemes"]!.AsArray()[0]!.AsObject();
        foreach (string key in AddedKeys)
        {
            Assert.That(theme.ContainsKey(key), Is.True, $"Expected key '{key}' to be added");
        }
    }

    [Test]
    public void V62Migration_UsesExistingThemeValuesAsDefaults()
    {
        string customDividerColor = "#ff112233";
        string customMenuButtonBGColor = "#ff445566";
        string customMenuButtonBorderColor = "#ff778899";
        string customMenuButtonBGHoverColor = "#ffaabbcc";
        string customMenuButtonBorderHoverColor = "#ffddeeff";

        JsonNode userData = CreatePreV62UserData(
            dividerColor: customDividerColor,
            menuButtonBGColor: customMenuButtonBGColor,
            menuButtonBorderColor: customMenuButtonBorderColor,
            menuButtonBGHoverColor: customMenuButtonBGHoverColor,
            menuButtonBorderHoverColor: customMenuButtonBorderHoverColor
        );

        User.UpdateSchemaVersion(userData, isImport: true);

        JsonObject theme = userData["SavedThemes"]!.AsArray()[0]!.AsObject();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(theme["SeriesCardBorderColor"]!.GetValue<string>(), Is.EqualTo(customDividerColor));
            Assert.That(theme["StatusAndBookTypeBorderColor"]!.GetValue<string>(), Is.EqualTo(customDividerColor));
            Assert.That(theme["SeriesCoverBGColor"]!.GetValue<string>(), Is.EqualTo(customMenuButtonBGColor));
            Assert.That(theme["SeriesCardButtonBGColor"]!.GetValue<string>(), Is.EqualTo(customMenuButtonBGColor));
            Assert.That(theme["SeriesCardDividerColor"]!.GetValue<string>(), Is.EqualTo(customDividerColor));
            Assert.That(theme["SeriesCardButtonBGHoverColor"]!.GetValue<string>(), Is.EqualTo(customMenuButtonBGHoverColor));
            Assert.That(theme["SeriesCardButtonBorderColor"]!.GetValue<string>(), Is.EqualTo(customMenuButtonBorderColor));
            Assert.That(theme["SeriesCardButtonBorderHoverColor"]!.GetValue<string>(), Is.EqualTo(customMenuButtonBorderHoverColor));
        }
    }

    [Test]
    public void V62Migration_UsesFallbackDefaults_WhenThemeColorsMissing()
    {
        JsonNode userData = CreatePreV62UserData(omitSourceColors: true);

        User.UpdateSchemaVersion(userData, isImport: true);

        JsonObject theme = userData["SavedThemes"]!.AsArray()[0]!.AsObject();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(theme["SeriesCardBorderColor"]!.GetValue<string>(), Is.EqualTo("#ffdfd59e"));
            Assert.That(theme["StatusAndBookTypeBorderColor"]!.GetValue<string>(), Is.EqualTo("#ffdfd59e"));
            Assert.That(theme["SeriesCoverBGColor"]!.GetValue<string>(), Is.EqualTo("#ff626460"));
            Assert.That(theme["SeriesCardButtonBGColor"]!.GetValue<string>(), Is.EqualTo("#ff626460"));
            Assert.That(theme["SeriesCardDividerColor"]!.GetValue<string>(), Is.EqualTo("#ffdfd59e"));
            Assert.That(theme["SeriesCardButtonBGHoverColor"]!.GetValue<string>(), Is.EqualTo("#ff2c2d42"));
            Assert.That(theme["SeriesCardButtonBorderColor"]!.GetValue<string>(), Is.EqualTo("#ff626460"));
            Assert.That(theme["SeriesCardButtonBorderHoverColor"]!.GetValue<string>(), Is.EqualTo("#ffdfd59e"));
        }
    }

    [Test]
    public void V62Migration_IsIdempotent()
    {
        JsonNode userData = CreatePreV62UserData();

        User.UpdateSchemaVersion(userData, isImport: true);

        // Capture the state after first migration
        JsonObject themeAfterFirst = userData["SavedThemes"]!.AsArray()[0]!.AsObject();
        Dictionary<string, string> valuesAfterFirst = [];
        foreach (string key in AddedKeys)
        {
            valuesAfterFirst[key] = themeAfterFirst[key]!.GetValue<string>();
        }
        double versionAfterFirst = userData["DataVersion"]!.GetValue<double>();

        // Run migration again
        User.UpdateSchemaVersion(userData, isImport: true);

        JsonObject themeAfterSecond = userData["SavedThemes"]!.AsArray()[0]!.AsObject();
        using (Assert.EnterMultipleScope())
        {
            foreach (string key in AddedKeys)
            {
                Assert.That(themeAfterSecond[key]!.GetValue<string>(), Is.EqualTo(valuesAfterFirst[key]), $"Key '{key}' changed on second run");
            }
            foreach (string key in RemovedKeys)
            {
                Assert.That(themeAfterSecond.ContainsKey(key), Is.False, $"Removed key '{key}' reappeared on second run");
            }
            Assert.That(userData["DataVersion"]!.GetValue<double>(), Is.EqualTo(versionAfterFirst));
        }
    }

    [Test]
    public void V62Migration_UpdatesDataVersionTo6Point2()
    {
        JsonNode userData = CreatePreV62UserData();

        User.UpdateSchemaVersion(userData, isImport: true);

        Assert.That(userData["DataVersion"]!.GetValue<double>(), Is.EqualTo(6.2));
    }

    [Test]
    public void V62Migration_HandlesMultipleThemes()
    {
        JsonNode userData = CreatePreV62UserData(themeCount: 3);

        User.UpdateSchemaVersion(userData, isImport: true);

        JsonArray themes = userData["SavedThemes"]!.AsArray();
        Assert.That(themes.Count, Is.EqualTo(3));

        for (int i = 0; i < themes.Count; i++)
        {
            JsonObject theme = themes[i]!.AsObject();
            using (Assert.EnterMultipleScope())
            {
                foreach (string key in RemovedKeys)
                {
                    Assert.That(theme.ContainsKey(key), Is.False, $"Theme {i}: expected key '{key}' to be removed");
                }
                foreach (string key in AddedKeys)
                {
                    Assert.That(theme.ContainsKey(key), Is.True, $"Theme {i}: expected key '{key}' to be added");
                }
            }
        }
    }

    private static JsonNode CreatePreV62UserData(
        string dividerColor = "#ffdfd59e",
        string menuButtonBGColor = "#ff626460",
        string menuButtonBorderColor = "#ff626460",
        string menuButtonBGHoverColor = "#ff2c2d42",
        string menuButtonBorderHoverColor = "#ffdfd59e",
        bool omitSourceColors = false,
        int themeCount = 1)
    {
        JsonArray themesArray = [];
        for (int i = 0; i < themeCount; i++)
        {
            JsonObject theme = new()
            {
                ["ThemeName"] = $"TestTheme{i}",
            };

            // Add the 15 keys that should be removed
            foreach (string key in RemovedKeys)
            {
                theme[key] = "#ff000000";
            }

            // Add source colors used as defaults for new keys
            if (!omitSourceColors)
            {
                theme["DividerColor"] = dividerColor;
                theme["MenuButtonBGColor"] = menuButtonBGColor;
                theme["MenuButtonBorderColor"] = menuButtonBorderColor;
                theme["MenuButtonBGHoverColor"] = menuButtonBGHoverColor;
                theme["MenuButtonBorderHoverColor"] = menuButtonBorderHoverColor;
            }

            themesArray.Add(theme);
        }

        return new JsonObject
        {
            ["DataVersion"] = 6.1,
            ["UserName"] = "TestUser",
            ["Language"] = "Romaji",
            ["MainTheme"] = "TestTheme0",
            ["Display"] = "Default",
            ["Currency"] = "USD",
            ["CollectionValue"] = "0",
            ["Region"] = "America",
            ["Memberships"] = new JsonObject(),
            ["SavedThemes"] = themesArray,
            ["UserCollection"] = new JsonArray(),
            ["UserIcon"] = string.Empty
        };
    }
}
