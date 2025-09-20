using Avalonia.Media.Imaging;
using MangaAndLightNovelWebScrape.Websites;
using System.Text.Json.Nodes;
using Tsundoku.Helpers;
using Tsundoku.Converters;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;
using System.Text.Encodings.Web;
using System.Diagnostics.CodeAnalysis;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicModel;

namespace Tsundoku.Models;

public sealed class User : ReactiveObject
{
    public static readonly JsonSerializerOptions JSON_SERIALIZATION_OPTIONS = new JsonSerializerOptions(UserModelContext.Default.Options)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    [Reactive] public double DataVersion { get; set; } = ViewModelBase.SCHEMA_VERSION;
    [Reactive] public string UserName { get; set; } = "UserName";
    [Reactive] public TsundokuLanguage Language { get; set; } = TsundokuLanguage.Romaji;
    [Reactive] public string Display { get; set; } = "Card";
    [Reactive] public string MainTheme { get; set; } = "Default";
    [Reactive] public string Currency { get; set; } = "$";
    [Reactive] public string CollectionValue { get; set; }
    [Reactive] public uint NumVolumesCollected { get; set; }
    [Reactive] public uint NumVolumesToBeCollected { get; set; }
    [Reactive] public decimal MeanRating { get; set; }
    [Reactive] public uint VolumesRead { get; set; }
    [Reactive] public Region Region { get; set; } = Region.America;
    [Reactive] public Dictionary<string, bool> Memberships { get; set; }
    [Reactive] public string Notes { get; set; }
    public List<TsundokuTheme> SavedThemes { get; set; }
    public List<Series> UserCollection { get; set; }

    [JsonConverter(typeof(UserIconBitmapJsonConverter))]
    [Reactive] public Bitmap? UserIcon { get; set; }

    public User(string UserName, TsundokuLanguage Language, string MainTheme, string Display, double DataVersion, string Currency, string CollectionValue, Region Region, Dictionary<string, bool> Memberships, List<TsundokuTheme> SavedThemes, List<Series> UserCollection)
    {
        this.UserName = UserName;
        this.Language = Language;
        this.Display = Display;
        this.DataVersion = DataVersion;
        this.Currency = Currency;
        this.MainTheme = MainTheme;
        this.CollectionValue = CollectionValue;
        this.Region = Region;
        this.SavedThemes = SavedThemes;
        this.UserCollection = UserCollection;
        this.Memberships = Memberships;
        NumVolumesCollected = 0;
        NumVolumesToBeCollected = 0;
        Notes = string.Empty;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, JSON_SERIALIZATION_OPTIONS);
    }

    /// <summary>
    /// Updates the users data version
    /// </summary>
    /// <param name="userData">Users parsed Json data</param>
    /// <param name="isImport">Whether the data that needs to check for an update is an import</param>
    /// <param name="updatedCovers">Whether the covers </param>
    /// <returns></returns>
    public static bool UpdateSchemaVersion(JsonNode userData, bool isImport = false)
    {
        bool updatedVersion = false;
        JsonObject series;
        JsonObject theme;
        JsonArray collectionJsonArray = userData[nameof(UserCollection)].AsArray();
        JsonArray themeJsonArray = userData[nameof(SavedThemes)].AsArray();

        // For users who did not get the older update
        bool containsOldDataVerionProperty = userData.AsObject().ContainsKey("CurDataVersion");
        if (!containsOldDataVerionProperty && !userData.AsObject().ContainsKey(nameof(DataVersion)))
        {
            userData.AsObject().Add(nameof(DataVersion), "1.0");
        }
        double curVersion = containsOldDataVerionProperty ? double.Parse(userData["CurDataVersion"].ToString()) : double.Parse(userData[nameof(DataVersion)].ToString());

        if (curVersion < 1.5) // 1.5 Version Upgrade changes Series Title, Staff. & Status to accomadte new changes & adds Stats related variables to Series and also changes Native to Japanese
        {
            userData.AsObject().Add(nameof(Currency), "$");
            userData.AsObject().Add(nameof(MeanRating), 0);
            userData.AsObject().Add(nameof(VolumesRead), 0);
            userData.AsObject().Add(nameof(CollectionValue), "");

            if (userData[nameof(Language)].ToString().Equals("Native"))
            {
                userData[nameof(Language)] = "Japanese";
            }

            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                series = collectionJsonArray.ElementAt(x).AsObject();
                series["Titles"] = new JsonObject
                {
                    ["Romaji"] = series["Titles"][0].ToString(),
                    ["English"] = series["Titles"][1].ToString(),
                    [series["Format"].ToString() switch
                    {
                        "Manhwa" => "Korean",
                        "Manhua" => "Chinese",
                        _ => "Japanese"
                    }] = series["Titles"][2].ToString()
                };

                series["Staff"] = new JsonObject
                {
                    ["Romaji"] = series["Staff"][0].ToString(),
                    [series["Format"].ToString() switch
                    {
                        "Manhwa" => "Korean",
                        "Manhua" => "Chinese",
                        _ => "Japanese"
                    }] = series["Staff"][1].ToString()
                };
                series.AsObject().Add("Score", 0);

                if (series["Titles"]["Romaji"].ToString().Equals(series["Titles"]["English"].ToString()))
                {
                    series["Titles"].AsObject().Remove("English");
                }

                if (curVersion < 1.4 && series["Status"].ToString().Equals("Complete"))
                {
                    series["Status"] = "Finished";
                }
            }
            userData[nameof(DataVersion)] = 1.5;
            LOGGER.Info("Updated Users Data to v1.5", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 1.6) // 1.6 Version Upgrade fixes scores defaulting to 0 to -1 instead so it doesn't show up i stats
        {
            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                series = collectionJsonArray.ElementAt(x).AsObject();
                if (double.Parse(series["Score"].ToString()) == 0)
                {
                    series["Score"] = -1;
                }
            }

            userData[nameof(DataVersion)] = 1.6;
            LOGGER.Info("Updated Users Data to v1.6", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 1.7) // 1.7 Version Upgrade To accomdate adding user icon
        {
            string defaultFileByte = "iVBORw0KGgoAAAANSUhEUgAAAEcAAABHCAYAAABVsFofAAAABHNCSVQICAgIfAhkiAAAED9JREFUeJztm3uUV9V1xz/73Pt7DMwMjDgQnipIfIvGEGN8RLpQCahRUbMaW7KaGqXVYDSJ7cpKWq1Lm7Y\u002BokajISpqdSkpacW3tvgKGmJQKooKIhJkeDrDMK/f657dP\u002B49997fQIT5zbDsH7NnrZnfvfecc\u002B/ev72/\u002B7v3uQODMiiDMiiDMiiDMijVoqqiqmYPYz71\u002Bv8H2ScPWN767NHBx49/pJuePnB314Mtz11a\u002BfjJlfvi3gMp\u002B\u002BbbK9s6UcYH5dKK8tZnZrjTqmrKLU9cbwtdtxmCCfvk3gMo/r5YNOODLSqCHUZPYXHQ8sTlxmQeKm9cfKMJuFQwolDcF/ceSNknnlMpWOOWFySj5crdlXJhtRfYuYIVAOy\u002BuPPAivR/iQu8EQds\u002BeqQrH7HIA0qyrQTJg2/95/OPRFAAVQRiY9QFbpKleCoWbc/LQKiqtbYaoMZjY4lXMB4AoJaq/GDGxGsCGptSViqO9vv2LLlra7\u002B6xRKf8PKjDtk62LfMzNFQTV83h0dScQIoCgoiIBVRVAqFeuJZ84MBymCQQygEprQEB6DWwEUjO\u002BhgCDhWSMIHkOwZ9umpquHZE88unvD0pZ\u002B6gX00zjjJk/7QdbozEtGdHJWQw95A4KSHZMDQNUiIogIqiBOQzEMkYAlk7Y6FcNxgIaDMAiqigqEHqPhOtjI3SU8L4oorCn6zGtpGjEi79/XDWf0Ry8nNRtnzOdPPCnj6bVfHlpkzvCuUHkJ/cQ3irpQ0lAphOhcEsl1Blxkizj/0Ejt6DgyCGH8xWMjZ4zOKUfly0zOVVhp/Ynx9H5KTcYZM\u002BasIVmv8\u002B5RmSB/3agdWIWPWjsplAOsQs5sZvwbrycT4kcNA0JFsd1l1rW0RoaT0Ii70ynyHETDkIuMHjsh4ZeSzXgY3Q/RgUsytRhHMvU7FnriHX7l/u1kUfyDx/Hlb58e64ECgQIBiSaAMWBt7A0T750XnlM3oFd\u002BcAtG18MQ3NWAhY4iy/5\u002BAcVyBR1AdtLnlQ488NSc8c20GQ3dnDSkBL6P/8WJdK5cGylKjBPgnCLCCxdWSsoOkfJO59Rf1WS\u002BQbCEBkIBEy7gNQ5h6OgR5BrroI0BJSd9Nk6p1GFydY35bYEhUMWUAwoPvxxeTBnA6ZjGGdXkGw/DQbFa7S1pTKoaL9XjkvtYMldfgLucNZIpPXPbxRkpPlzR3BcspjU34/J3\u002Bqon1Ig5qvB6d5bz/9jMSD9IpdYIN2P3T01IeVNktRhZezuTOIqTcqPwk8VlKUSZnLNc2bQDCQJAsCg/OPPI8V658suK8c8VrZzii7Tqsw8cJmfM6TP/qTFALX/Z2MnqUoa15Qyt1mfK50fheU55RSPvSXiOxEaoiqrYkC5Bp/BFJQyTFJi7sN2weScb2jr5bhMQ3au5oY6/PnkSBIqx\u002BrVo3tCy334JcEtftazROIYp\u002BSLfaergrrYGflNo4t7rv05j3k8pbqiivCIRwbOIeKBB1RgVQaIwUnG\u002BYsJxLneLgA2v//zR5dzz4NKqp2puzLtsH2O2AMbaL9WiZU3Gkdi9wz9CqJiq4yVujIn8IgJRkYiWOA0i48WGiQMLUFSD8MgZTaszlRjBGzsaDQyq0FUqp9aO7hAC\u002BmYAXfrAyIqxk33bs0ZO/Jute9KzJmy3sX9ELNWFgpheyVhTVxXUuk9pFcMwxJHA0ICqCdaAO7Zx2E0a1cCSa85k7NfPxi\u002BM4Jg53\u002BTYoybSVgpAoOvjjQRBBRGseLIsePzmN2zr9hazrfW3tq3wcfnZ22/YJ8ZxTNgpL6QRhJRCNh6vCKqRWUXpLdUrJDGhkeFIZytVdnQVyYggVhFVckPquGT64dhKAKq0Ln2NcusO1KpSChYoHIviRfMz4pk39qRnjazAhM8qEn2XWv3wQGeh7AIq8TM3JuX6zq82buvkk/YeSpWArlKF1p3FOKO9uXpr9TyF8w8dydCsFx8D2EKBppyPDQI0CAiKRQTxgu6enNheX0ixtEBfumvyp2tZi2hMx\u002BLfIawIS9/eRFuhzPSLH2Ld5g4QYc6PF9NTsbz6zmaef309gYZVeHh7QUW46YHf8eATK1mxeivX/XIpf/ez/0GA36/azNxrn6Stq8TOriLligUBr1yJNEjoQctz/826\u002Bfex/qFHqbS2senxJ9nw9DP88eFHsNZWGVdUc9ie8sAbJzVNIhIXAq9l1Zot/MOtL3DNd0/l\u002Bvmv0FG0vPvBNjIZj8kT9uMntyzh9keWA5adhRLnfW8h5bIlCAICC8cd9jmumnM8d/xoBjsLFX544/MMa6xj1tyH\u002BMq3HuCuRSsAeKW1iPbyxHFnfo1x35iN7epCRDBWyQ5r5IA5FyGeQ6vwu7RZ/zqZduVHn6ZlPwqR6ionxBXDjJMmsXLNNqZPPYCjD27mnkXLmXrUGDyguSHL03dfRKlcwYrhpgW/AxGMEawV7l20nCXL1jHrlMlcMnsK3/j\u002BIs477TCu\u002BPOpdPSUOWXOfZx83AQQWLmhjRt\u002B8SY3HlvHmOOPwOsuM3xkM5nGBiSTQUslUKX\u002B4IPx/AwW22Z87zlreFfVW5w9fd6be9KwH8YJQTYmFhqm7/HNDdx29WkIUChb/uPZVdz0w9ORCJz3a8hTqATcMP\u002B3vLZiAwtvno1nQgz7q9nHMfXw0fzXkvcplC3zr5nF2OZ6ykHA9fOXcvShozli4oiIUAqbW7vY9NrbDD9yDM37jwMjdHz4Ed6IEYw85US6N7bQuvQ16s49G6OmSUUCX80tMnPezr3RsHaeI64ITPiOE6vw1pqtzL32CS6\u002B8Iscf\u002BRoVJXt7QWWv7uZmxa8yv7Dh/Dv/3ouw4dkUVGMQMYTTj1uAl89bjwCNOQzbGnr5uePLOet1ZtYdPMF\u002BBHGuOZXLpdh1H5jyebyYBWTyzH\u002BnDMREfIj9sOvHxo/n1TsNyse24Er9kbPmjAn5i2SZCBXJAqwrqWNv73uaa6Z92dcfM7RISYorFi9lVsWLOXHc6fx8E/PYWRjPlpQyed9shkv7Oyl7jPnR4/R2t7Dk3deRF3WwyGciKIKo089kWw\u002BH/WDoGHsWIwYVGwbIgybNBGTKk\u002BM6lx9Z2F2b/SsPaw0ImskVbiTSWObWPKri6jL\u002BalejjB96oFMnzqhCqvc3\u002BsuOzXKIikcAx679UJyGQ9sUDXeKTvsc6OSTkf0HBaL9f3L/EBuQu3oOEuFTewsO9rGA2v3pGJt2UpMUiZUlQOJ5HO97K6O9fQyTDTX9zwyvlc9Bchl/cj4rjANf7s1WlaspBxUqFQCQLFqwfNasuo9Scb7tzDyxbUL3re\u002Bt5rhTRv2Rs2aW0MhDqf9xTk8KSdOJCGD7sd1MdJKV3uNC5XdNEHikW1vr\u002BLjlvfo2L6Voiqvfti6vQdzjMyct9OcfvnPyGUuE0/WK0olw\u002BzMzCsOkSMuLO2NjjUCchhOStpjEsxJKxmfU1c29DZbMr93szSkB6mGu7iWaYphW6jL\u002BKx\u002B7Cn\u002BuaWJt7pl5/r3XtkezVfgTlX9BS/eOcqfdtnmvujZr4ar6\u002Ba5fSn32J1dJQrFsutxRkoSKZZqPxACuRGhd2D23qmowg1Venoq0WEIzrZUplQqA7tibWSkPhkG\u002BkkCIWwjaCqj/8v9y3ho8QrKpaiR7rJYr8\u002B4Pam4xoxr8rg4VTWpbRhNYpbQePm4wxitL4B4uwJgjVJjm9TVVmFucIB5\u002B6PL\u002BdXCPzDloAa\u002BNW0cJq57NARxtVUYE2J50nQPbZbaY5Bocy/6cRt9iPD8ik946Y1Nsb3CzWJgFx\u002BsXWrzHBPieLpBpSiLnnuXIybUc8elR5JraMTzM1ThA9Upvyqn9zptSe1YuHtoUuiuaVVeemMTJn4HKgH6gZLaADnx5lgCDBs372DWjAMYOqKZhnEHYMvlVMMwNUNcYziV2xxqIwm\u002BpPEpfU6VbMP2\u002BFDi0MK1CwdEagur1Od459qESgowdMx4sFDc1pJ0/1QRY8LaImqtJoYjDr1kO6ZX7nJ2iq4FXTuj06HxBMHqgNqmxrCKM1OSZn21jB8zjNfW9kQYYhkyZkK1JVWJm\u002BriEb2WQbp9ighiwTVjMYLYqMB17REBr/6TZEkbPo2E4z9bQE5niLilAvzFWVO44e6XmX3VImadcjAZL/R5kfRbEr2pY3qLN/KmKsadjE7Pfev9LcmV1HD5rMMqll5lw7D6HJ5nePu9Ftau3oggeBK\u002BNGIROoOUJR0hdJCyi9lCSZktuS2hBzYYx7ITEBy4XDUQPCf11Pf85g0OyZa466B2sqRQQ4RiucKqlh2MG15PTF5gF2\u002BKESfFf9zvNIz/ur2e/\u002BwM2xGG3v41MFJj\u002BZACSZvQ/p5CwDH5AJ\u002BQ6qf3ugOFjPFoHDcSwfGXCMijloZGRE4wWEMKkyKwtjY2QnaDQofbIHbPoyDmMyaBDpGjulWqriS0v\u002BqlAIB8ltE/uSqiSZEO8Z55eiDJuhEbVPdeXTR22KPL4f5XQts5BtVr06\u002B/UnO2CulG70o5zhm7bYJpqULbr5/AGJMigDZO65oqTqsTeVJcOOlZ2YYJ94ec44WzPmtArt7ES0h7LuOxozPsu6mNWqlhfg43YoISPS\u002B8gmeiuj5lgXSTKy0aV1upc6qU2xtR6kmhFHaXr6p/UmNYBdEzJe0DUeX8M47gp/Nbmf7\u002BcLyIlkikuSjsrDQTdMSvTbjFIhJX3bLQ\u002BLerPKPQij0TGiSIlxEEg8ZbRQMhtXmOmF16LypCoRzukVWy9Qwf2VxVOglQL\u002BkwS2WgFPsNs7xGWV7iqsKKxgWuqtLR3o7d8UlIQqP2aBkDQqUWnXYn/SsfUjuIRuGpFz\u002BgqXkEJ582HRETe0x6vJpofzt6FzCNJRE5SKp0YHehBsKaVavYsOITbNT8eifI82GQo1QJlv7JaX2UmjuBElV8EaSgQGd3kcbhzeRzHt\u002BfOSwEzPSuZJpOV7E6GxajaVKZrqXEYVQMYtyndTy4grg3dE93M4EtflCg9L1adNqd1JitbFT59KJvUQvDN8L\u002B9T6qQs\u002B2jRAEqWLT5Z9U2ECVsYSoceVeq4028Ui9F\u002BSXO8O7Kry\u002Bavs/bvik1FIolxe3rl22Vxt2eyN9No4xWUVMwitSdV61qUJUqRs5OqHRMaDurnZKX6suFsI3vJwbhXPN0JZwP1yEuhd/f/\u002B6dzet76sue5I\u002BG8f3c4pIpS2QbPiscRVKLpuhWCjSVVTueqGdvJe6jGDVxmES/z\u002BEKzbZDcZEBolB2Z0XYcX6bodZmjHlvdpN6LOufZ3w0UfN5YMO37751rbhEw7NRW9wiFBBOH7KeBY\u002B9Rb/u\u002BwP2KlfwPf9qv6Me6e4OkxM8q8AqdzW\u002B/3l8DYJsdzaZrFK0B7ot89bvn1Tf4zwp6QmUjDmwBMOzeZzLxsjze6ctel/Aknwh8SxiKuO\u002BKVsV2pIPDwpBTSZ5GhgLyPZQJatX/3CCQxsMR5LzYxp1KSvjPSN\u002BZKBhgBwe5WOlnkAnheCcfQ5CIJd1ql\u002BGtE0/fd6XY7ePxULipFuOrqfb2lZ3l2rDoMyKIMyKIMyKIMyKIMyKIPSF/k/FjClO\u002BdWYasAAAAASUVORK5CYII=";

            userData.AsObject().Add(nameof(UserIcon), defaultFileByte);
            userData[nameof(DataVersion)] = 1.7;
            LOGGER.Info("Updated Users Data to v1.7", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 1.8) // 1.8 Version Upgrade to rename button color identifiers for TsundokuTheme change
        {
            for (int x = 0; x < themeJsonArray.Count; x++)
            {
                theme = themeJsonArray.ElementAt(x).AsObject();
                theme["SeriesButtonBGColor"] = (uint)theme["SeriesSwitchPaneButtonBGColor"];
                theme["SeriesButtonBGHoverColor"] = (uint)theme["SeriesSwitchPaneButtonBGHoverColor"];
                theme["SeriesButtonIconColor"] = (uint)theme["SeriesSwitchPaneButtonIconColor"];
                theme["SeriesButtonIconHoverColor"] = (uint)theme["SeriesSwitchPaneButtonIconHoverColor"];
            }

            userData[nameof(DataVersion)] = 1.8;
            LOGGER.Info("Updated Users Data to v1.8", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 1.9) // 1.9 Version Upgrade resizes all bitmaps in the users cover folder
        {
            string coverPath;
            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                coverPath = collectionJsonArray.ElementAt(x)["Cover"].ToString();
                if (File.Exists(coverPath))
                {
                    Bitmap resizedBitMap = new Bitmap(coverPath).CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
                    resizedBitMap.Save(coverPath, 100);
                }
            }
            userData[nameof(DataVersion)] = 1.9;
            LOGGER.Info("Updated Users Data to v1.9", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 2.0) // 2.0 Verion Upgrade adds Memberships for Price Analysis and changes "Score" to "Rating"
        {
            userData[nameof(Memberships)] = new JsonObject
            {
                [BooksAMillion.TITLE] = false,
                [KinokuniyaUSA.TITLE] = false
            };

            userData.AsObject()[nameof(MeanRating)] = (decimal)userData["MeanScore"];
            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                series = collectionJsonArray.ElementAt(x).AsObject();
                series["Rating"] = decimal.Parse(series["Score"].ToString());
            }
            userData.AsObject().Remove("MeanScore");

            userData[nameof(DataVersion)] = 2.0;
            LOGGER.Info("Updated Users Data to v2.0", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 2.1) // 2.1 Version updates empty demographics from null or empty string to "Unknown" & removes Default thme from file
        {
            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                series = collectionJsonArray.ElementAt(x).AsObject();
                if (string.IsNullOrWhiteSpace(series["Demographic"]?.ToString()))
                {
                    series["Demographic"] = SeriesDemographic.Unknown.ToString();
                }
            }
            JsonArray themes = userData[nameof(SavedThemes)].AsArray();
            for (int x = 0; x < themes.Count; x++)
            {
                if (themes.ElementAt(x)["ThemeName"].ToString().Equals("Default"))
                {
                    themes.RemoveAt(x);
                }
            }
            userData[nameof(DataVersion)] = 2.1;
            LOGGER.Info("Updated Users Data to v2.1", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 2.2) // 2.2 Removes RightStufAnime as the site no longer exists
        {
            userData[nameof(Memberships)].AsObject().Remove("RightStufAnime");
            userData[nameof(DataVersion)] = 2.2;
            LOGGER.Info("Updated Users Data to v2.2", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 2.4) // 2.4  Add Region property to User
        {
            userData.AsObject().Add(new KeyValuePair<string, JsonNode?>("Region", Region.America.ToString()));
            userData[nameof(DataVersion)] = 2.4;
            LOGGER.Info("Updated Users Data to v2.4", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 2.5) // 2.5 Update the cover name to add the series format
        {
            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                series = collectionJsonArray.ElementAt(x).AsObject();
                string[] coverFileName = series["Cover"].ToString().Split('_');
                string format = series["Format"].ToString().ToUpper();
                string oldCoverFileName = series["Cover"].ToString();
                if (!coverFileName[1].StartsWith(format) && File.Exists(oldCoverFileName))
                {
                    series["Cover"] = $@"{coverFileName[0]}_{format}.{oldCoverFileName.Substring(oldCoverFileName.Length - 3)}";
                    File.Move(oldCoverFileName, series["Cover"].ToString());
                    LOGGER.Debug("Updated {} Cover File Name", series["Titles"]["Romaji"].ToString());
                }
            }
            userData[nameof(DataVersion)] = 2.5;
            LOGGER.Info("Updated Users Data to v2.5", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 3.0) // 3.0 Update the colors in themes to be hex string
        {
            for (int x = 0; x < themeJsonArray.Count; x++)
            {
                theme = themeJsonArray.ElementAt(x).AsObject();
                foreach (System.Reflection.PropertyInfo property in typeof(TsundokuTheme).GetProperties().Skip(1))
                {
                    theme[property.Name] = Avalonia.Media.Color.FromUInt32(uint.Parse(theme[property.Name].ToString())).ToString();
                }
            }
            userData[nameof(DataVersion)] = 3.0;
            LOGGER.Info("Updated Users Data to v3.0", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 4.0) // 4.0 Remove "SeriesButtonBGColor" & SeriesButtonBGHoverColor" Colors from Themes
        {
            for (int x = 0; x < themeJsonArray.Count; x++)
            {
                themeJsonArray.ElementAt(x).AsObject().Remove("SeriesButtonBGColor");
                themeJsonArray.ElementAt(x).AsObject().Remove("SeriesButtonBGHoverColor");
            }
            userData[nameof(DataVersion)] = 4.0;
            LOGGER.Info("Updated Users Data to v4.0", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 5.0) // 5.0 Update series property "Cost" to "Value
        {
            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                series = collectionJsonArray.ElementAt(x).AsObject();
                series.Add("Value", decimal.Parse(series["Cost"].ToString()));
                series.Remove("Cost");
            }
            userData[nameof(DataVersion)] = 5.0;
            LOGGER.Info("Updated Users Data to v5.0", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 5.1) // 5.1 Add new staff theme color, and remove B&N membership since B&N site no longer works in scrape
        {
            for (int x = 0; x < themeJsonArray.Count; x++)
            {
                theme = themeJsonArray.ElementAt(x).AsObject();
                if (!theme.ContainsKey("SeriesCardPublisherColor")) theme.Add("SeriesCardPublisherColor", theme["SeriesCardStaffColor"].ToString());
            }
            userData[nameof(Memberships)].AsObject().Remove("Barnes & Noble");
            userData[nameof(DataVersion)] = 5.1;
            LOGGER.Info("Updated Users Data to v5.1", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 5.2) // 5.2 Add new staff theme color
        {
            for (int x = 0; x < themeJsonArray.Count; x++)
            {
                theme = themeJsonArray.ElementAt(x).AsObject();
                if (!theme.ContainsKey("UserIconBorderColor"))
                {
                    theme.Add("UserIconBorderColor", theme["DividerColor"].ToString());
                }
                else if (theme["UserIconBorderColor"] is null)
                {
                    theme["UserIconBorderColor"] = theme["DividerColor"].ToString();
                }
            }
            userData[nameof(DataVersion)] = 5.2;
            LOGGER.Info("Updated Users Data to v5.2", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 6.0) // 6.0 Remove the full file path from Series Cover property & change User proeprty "CurDataVersion" to "DataVersion" 
        {
            for (int x = 0; x < collectionJsonArray.Count; x++)
            {
                series = collectionJsonArray.ElementAt(x).AsObject();
                series["Cover"] = Path.GetFileName(series["Cover"].ToString());
            }
            userData.AsObject().Remove("CurDataVersion");
            userData.AsObject().Add(nameof(DataVersion), "6.0");
            LOGGER.Info("Updated Users Data to v6.0", !isImport);
            updatedVersion = true;
        }

        if (curVersion < 6.1) // 6.1 Update "CollectionPrice" to "CollectionValue"
        {
            userData.AsObject()[nameof(CollectionValue)] = (string)userData["CollectionPrice"];
            userData.AsObject().Remove("CollectionPrice");
            userData[nameof(DataVersion)] = 6.1;
        }

        if (!isImport)
        {
            AppFileHelper.WriteUserDataToFile(userData);
        }
        return updatedVersion;
    }
}

[JsonSerializable(typeof(User))]
[JsonSourceGenerationOptions(
    UseStringEnumConverter = true,
    WriteIndented = true,
    ReadCommentHandling = JsonCommentHandling.Disallow,
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    IncludeFields = false,
    NumberHandling = JsonNumberHandling.AllowReadingFromString
)]
internal sealed partial class UserModelContext : JsonSerializerContext { }