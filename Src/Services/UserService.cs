using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
using ReactiveUI;
using System.Reactive.Disposables;
using Avalonia.Media.Imaging;
using Tsundoku.Helpers;
using MangaAndLightNovelWebScrape.Websites;
using Tsundoku.ViewModels;
using Tsundoku.Converters;
using System.Text.Json.Nodes;
using static Tsundoku.Models.Enums.TsundokuLanguageEnums;
using System.Diagnostics.CodeAnalysis;
using DynamicData.Kernel;
using static Tsundoku.Models.Enums.TsundokuFilterEnums;
using ReactiveUI.Fody.Helpers;
using Tsundoku.Models;

namespace Tsundoku.Services;

public interface IUserService : IDisposable
{
    IObservable<User?> CurrentUser { get; }
    IObservable<IChangeSet<Series, Guid>> UserCollectionChanges { get; }
    IObservable<IChangeSet<TsundokuTheme, string>> SavedThemeChanges { get; }
    void UpdateUser(Action<User> updateAction);
    void LoadUserData();
    void SaveUserData(User user);
    void SaveUserData();
    void ImportUserDataFromJson(string filePath);
    void UpdateUserIcon(string filePath);
    User? GetCurrentUserSnapshot();
    uint GetCurrentThemeIndex();
    Series? GetSeriesByCoverPath(string coverPath);
    TsundokuLanguage GetLanguage();
    bool DoesDuplicateExist(Series series);
    IReadOnlyList<Series> GetUserCollection();
    int GetCurCollectionCount();
    uint SelectedThemeIndex { get; set; }

    ReadOnlyObservableCollection<TsundokuTheme> SavedThemes { get; }

    IObservable<TsundokuTheme?> CurrentTheme { get; } // Expose as an observable
    void SetCurrentTheme(TsundokuTheme theme);
    void SetCurrentTheme(string themeName);
    void OverrideCurrentTheme(TsundokuTheme theme);
    TsundokuTheme? GetCurrentThemeSnapshot();
    TsundokuTheme? GetMainTheme();
    bool AddSeries(Series? series, bool allowDuplicate = false);
    bool RefreshSeries(Series originalSeries, Series? refreshdSeries);
    void UpdateSeriesCoverBitmap(Guid seriesId, Bitmap bitmap);
    void RemoveSeries(Series series);

    void AddTheme(TsundokuTheme theme);
    void RemoveTheme(TsundokuTheme theme);
    void RemoveTheme(string themeName);
    void ExportTheme(string fileName);
    Task ImportThemeAsync(string themeFilePath); 

    void ClearUserCollection();
}

public sealed class UserService : IUserService, IDisposable
{
    public static readonly string USER_AGENT = "Tsundoku/1.0";
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly BehaviorSubject<User?> _userSubject = new(null);
    public IObservable<User?> CurrentUser => _userSubject.AsObservable();

    private readonly BehaviorSubject<TsundokuTheme?> _currentThemeSubject = new(null);
    public IObservable<TsundokuTheme?> CurrentTheme => _currentThemeSubject.AsObservable();


    private readonly SourceCache<Series, Guid> _userCollectionSourceCache = new(s => s.Id);
    public IObservable<IChangeSet<Series, Guid>> UserCollectionChanges => _userCollectionSourceCache.Connect();
    private readonly SourceCache<TsundokuTheme, string> _savedThemesSourceCache = new(t => t.ThemeName);
    public IObservable<IChangeSet<TsundokuTheme, string>> SavedThemeChanges => _savedThemesSourceCache.Connect();

    // private readonly ReadOnlyObservableCollection<Series> _userCollection;
    private readonly ReadOnlyObservableCollection<TsundokuTheme> _savedThemes;

    [Reactive] public string SeriesFilterText { get; set; } = string.Empty;
    [Reactive] public TsundokuFilter SelectedFilter { get; set; } = TsundokuFilter.None;
    [Reactive] public uint SelectedThemeIndex { get; set; }

    private readonly CompositeDisposable _disposables = [];
    private bool _disposed = false;

    public UserService()
    {
        SavedThemeChanges
            .SortAndBind(out _savedThemes, new TsundokuThemeComparer(TsundokuLanguage.English))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe()
            .DisposeWith(_disposables);

        _userSubject
            .Where(user => user is not null)
            .Select(user => user!.MainTheme)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(mainThemeName =>
            {
                Optional<TsundokuTheme> lookupTheme = _savedThemesSourceCache.Lookup(mainThemeName);
                TsundokuTheme theme;

                if (lookupTheme == null)
                {
                    // Fall back to the first available theme, if any
                    theme = _savedThemes.FirstOrDefault();

                    User user = _userSubject.Value;
                    if (theme is not null)
                    {
                        LOGGER.Warn($"MainTheme '{mainThemeName}' not found, defaulted to '{theme.ThemeName}'.");
                    }
                    else
                    {
                        theme = TsundokuTheme.DEFAULT_THEME;
                        LOGGER.Warn("No themes available to fallback to, adding default theme");
                    }

                    user.MainTheme = theme.ThemeName;
                    _userSubject.OnNext(user);
                }
                else
                {
                    theme = lookupTheme.Value;
                }

                _currentThemeSubject.OnNext(theme);
                LOGGER.Info($"Set Theme to '{theme?.ThemeName ?? "null"}'");
            })
            .DisposeWith(_disposables);
    }
    public ReadOnlyObservableCollection<TsundokuTheme> SavedThemes => _savedThemes;

    public void LoadUserData()
    {
        LOGGER.Info("Attempting to load user data...");
        User loadedUser;
        string userDataFilePath = AppFileHelper.GetUserDataJsonPath();
        bool invalidJson = false;
        if (invalidJson || !File.Exists(userDataFilePath))
        {
            loadedUser = CreateDefaultUser();
            SaveUserData(loadedUser);
        }

        string userFileData = File.ReadAllText(userDataFilePath);
        if (string.IsNullOrWhiteSpace(userFileData))
        {
            LOGGER.Debug("Json is Empty Creating Default User");
            loadedUser = CreateDefaultUser();
        }
        else
        {
            JsonNode? userData = JsonNode.Parse(userFileData);
            if (userData is not null)
            {
                User.UpdateSchemaVersion(userData, false);
                loadedUser = userData.Deserialize(UserModelContext.Default.User);
            }
            else
            {
                LOGGER.Debug("Json is Malformed Creating Default User");
                loadedUser = CreateDefaultUser();
            }
        }

        RefreshSourceCache(loadedUser);
        _userSubject.OnNext(loadedUser);
        SaveUserData();
        LOGGER.Info($"Finished Loading \"{loadedUser.UserName}'s\" Data!");
    }

    public uint GetCurrentThemeIndex()
    {
        string currentName = GetCurrentThemeSnapshot().ThemeName;

        // This only loops until it finds the first match (or until the end).
        var match = _savedThemes
            .AsValueEnumerable()
            .Select((theme, idx) => new { theme, idx })
            .FirstOrDefault(x => x.theme.ThemeName.Equals(currentName));

        // If no match, FirstOrDefault returns null, so idx = 0 by default.
        int foundIndex = (match is null) ? -1 : match.idx;

        return (uint)(foundIndex >= 0 ? foundIndex : 0);
    }


    public int GetCurCollectionCount()
    {
        return _userCollectionSourceCache.Count;
    }

    private static void PreallocateSeriesImageBitmaps(SourceCache<Series, Guid> userCollection)
    {
        string[] coverFilesList = [ ..Directory
            .EnumerateFiles(AppFileHelper.GetCoversFolderPath(), "*.*", SearchOption.TopDirectoryOnly)];

        Dictionary<string, string> coverFiles = coverFilesList
            .AsValueEnumerable()
            .Where(file => AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(Path.GetExtension(file)))
            .ToDictionary(
                f => Path.GetFileNameWithoutExtension(f),
                f => f,
                StringComparer.OrdinalIgnoreCase
            );

        userCollection.Edit(innerCache =>
        {
            foreach (Series curSeries in innerCache.Items)
            {
                string? fullCoverPath = AppFileHelper.GetFullCoverPath(curSeries.Cover);
                if (string.IsNullOrEmpty(fullCoverPath))
                    return;

                bool hasPngExtension = Path.GetExtension(curSeries.Cover).Equals(".png", StringComparison.OrdinalIgnoreCase);
                string baseName = Path.GetFileNameWithoutExtension(curSeries.Cover);
                Bitmap? loadedBitmap = null;

                if (hasPngExtension && File.Exists(fullCoverPath))
                {
                    loadedBitmap = GenerateCoverBitmap(curSeries, fullCoverPath, true);
                }
                else
                {
                    LOGGER.Debug("'.png' Extension file not found for {title} or File Does Not Exist, Searching for Another Valid File...", curSeries.Titles[TsundokuLanguage.Romaji]);
                    if (coverFiles.TryGetValue(baseName, out string fallbackPath))
                    {
                        loadedBitmap = GenerateCoverBitmap(curSeries, fallbackPath, false);

                        if (hasPngExtension)
                        {
                            AppFileHelper.DeleteCoverFile(Path.GetFileName(fallbackPath));
                        }
                        else
                        {
                            AppFileHelper.DeleteCoverFile(curSeries.Cover);
                            curSeries.Cover = Path.ChangeExtension(curSeries.Cover, ".png");
                        }
                    }
                    else
                    {
                        if (!curSeries.Cover.EndsWith(".png"))
                        {
                            curSeries.Cover = Path.ChangeExtension(curSeries.Cover, ".png");
                        }
                        LOGGER.Warn("{name} {format} Series cover image {image} does not exist", curSeries.Titles[TsundokuLanguage.Romaji], curSeries.Format, curSeries.Cover);
                    }
                }

                curSeries.CoverBitMap = loadedBitmap;
            }

            HashSet<string> covers = [ ..innerCache.Items
                .AsValueEnumerable()
                .Select(series => series.Cover)
                .Where(cover => !string.IsNullOrWhiteSpace(cover))];

            // Traverse files and delete any not referenced in collection
            foreach (string filePath in coverFilesList)
            {
                string fileName = Path.GetFileName(filePath);
                if (!covers.Contains(fileName))
                {
                    try
                    {
                        File.Delete(filePath);
                        LOGGER.Info("Deleted unused file {File}", filePath);
                    }
                    catch (Exception ex)
                    {
                        // Optional: log or handle the error
                        LOGGER.Warn(ex, "Failed to delete {File}", fileName);
                    }
                }
            }
        });

        LOGGER.Info("Cover Images fully Loaded");
        coverFilesList = null;
        coverFiles.Clear();
        coverFiles = null;
    }

    private static Bitmap GenerateCoverBitmap(Series series, string fullCoverPath, bool isPngExtension)
    {
        Bitmap loadedBitmap = new Bitmap(fullCoverPath);
        if (!isPngExtension)
        {
            fullCoverPath = Path.ChangeExtension(fullCoverPath, ".png");
        }

        if (loadedBitmap.Size.Width != LEFT_SIDE_CARD_WIDTH || loadedBitmap.Size.Height != IMAGE_HEIGHT)
        {
            Bitmap scaledBitmap = loadedBitmap.CreateScaledBitmap(new PixelSize(LEFT_SIDE_CARD_WIDTH, IMAGE_HEIGHT), BitmapInterpolationMode.HighQuality);
            LOGGER.Debug("Scaled {} Cover Image", series.Titles[TsundokuLanguage.Romaji]);
            loadedBitmap.Dispose();

            scaledBitmap.Save(fullCoverPath, 100);
            LOGGER.Info("Saved Scaled Cover {path}", fullCoverPath);

            return scaledBitmap;
        }

        if (!isPngExtension)
        {
            loadedBitmap.Save(fullCoverPath, 100);
            LOGGER.Info("Saved Cover {path}", fullCoverPath);
        }
        
        return loadedBitmap;
    }

    public Series? GetSeriesByCoverPath(string coverPath)
    {
        if (string.IsNullOrWhiteSpace(coverPath))
            return null;

        return _userCollectionSourceCache.Items
            .FirstOrDefault(series => string.Equals(series.Cover, coverPath));
    }

    private void RefreshSourceCache(User? user)
    {
        if (user!.SavedThemes is not null)
        {
            _savedThemesSourceCache.Clear();
            _savedThemesSourceCache.AddOrUpdate(user!.SavedThemes);
        }
        if (user!.UserCollection is not null)
        {
            _userCollectionSourceCache.Clear();
            _userCollectionSourceCache.AddOrUpdate(user!.UserCollection);
            PreallocateSeriesImageBitmaps(_userCollectionSourceCache);
        }
    }

    private static User CreateDefaultUser()
    {
        LOGGER.Info("Creating New User");
        User user = new User(
                    "UserName",
                    TsundokuLanguage.Romaji,
                    "Default",
                    "Card",
                    ViewModelBase.SCHEMA_VERSION,
                    "$",
                    "$0.00",
                    Region.America,
                    new Dictionary<string, bool>
                    {
                        { BooksAMillion.WEBSITE_TITLE, false },
                        { Indigo.WEBSITE_TITLE, false },
                        { KinokuniyaUSA.WEBSITE_TITLE , false }
                    },
                    [],
                    []
                );
        user.SavedThemes.Add(TsundokuTheme.DEFAULT_THEME);
        user.UserIcon = BitmapConverter.BytesToBitmap(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEcAAABHCAYAAABVsFofAAAABHNCSVQICAgIfAhkiAAAED9JREFUeJztm3uUV9V1xz/73Pt7DMwMjDgQnipIfIvGEGN8RLpQCahRUbMaW7KaGqXVYDSJ7cpKWq1Lm7Y\u002BokajISpqdSkpacW3tvgKGmJQKooKIhJkeDrDMK/f657dP\u002B49997fQIT5zbDsH7NnrZnfvfecc\u002B/ev72/\u002B7v3uQODMiiDMiiDMiiDMijVoqqiqmYPYz71\u002Bv8H2ScPWN767NHBx49/pJuePnB314Mtz11a\u002BfjJlfvi3gMp\u002B\u002BbbK9s6UcYH5dKK8tZnZrjTqmrKLU9cbwtdtxmCCfvk3gMo/r5YNOODLSqCHUZPYXHQ8sTlxmQeKm9cfKMJuFQwolDcF/ceSNknnlMpWOOWFySj5crdlXJhtRfYuYIVAOy\u002BuPPAivR/iQu8EQds\u002BeqQrH7HIA0qyrQTJg2/95/OPRFAAVQRiY9QFbpKleCoWbc/LQKiqtbYaoMZjY4lXMB4AoJaq/GDGxGsCGptSViqO9vv2LLlra7\u002B6xRKf8PKjDtk62LfMzNFQTV83h0dScQIoCgoiIBVRVAqFeuJZ84MBymCQQygEprQEB6DWwEUjO\u002BhgCDhWSMIHkOwZ9umpquHZE88unvD0pZ\u002B6gX00zjjJk/7QdbozEtGdHJWQw95A4KSHZMDQNUiIogIqiBOQzEMkYAlk7Y6FcNxgIaDMAiqigqEHqPhOtjI3SU8L4oorCn6zGtpGjEi79/XDWf0Ry8nNRtnzOdPPCnj6bVfHlpkzvCuUHkJ/cQ3irpQ0lAphOhcEsl1Blxkizj/0Ejt6DgyCGH8xWMjZ4zOKUfly0zOVVhp/Ynx9H5KTcYZM\u002BasIVmv8\u002B5RmSB/3agdWIWPWjsplAOsQs5sZvwbrycT4kcNA0JFsd1l1rW0RoaT0Ii70ynyHETDkIuMHjsh4ZeSzXgY3Q/RgUsytRhHMvU7FnriHX7l/u1kUfyDx/Hlb58e64ECgQIBiSaAMWBt7A0T750XnlM3oFd\u002BcAtG18MQ3NWAhY4iy/5\u002BAcVyBR1AdtLnlQ488NSc8c20GQ3dnDSkBL6P/8WJdK5cGylKjBPgnCLCCxdWSsoOkfJO59Rf1WS\u002BQbCEBkIBEy7gNQ5h6OgR5BrroI0BJSd9Nk6p1GFydY35bYEhUMWUAwoPvxxeTBnA6ZjGGdXkGw/DQbFa7S1pTKoaL9XjkvtYMldfgLucNZIpPXPbxRkpPlzR3BcspjU34/J3\u002Bqon1Ig5qvB6d5bz/9jMSD9IpdYIN2P3T01IeVNktRhZezuTOIqTcqPwk8VlKUSZnLNc2bQDCQJAsCg/OPPI8V658suK8c8VrZzii7Tqsw8cJmfM6TP/qTFALX/Z2MnqUoa15Qyt1mfK50fheU55RSPvSXiOxEaoiqrYkC5Bp/BFJQyTFJi7sN2weScb2jr5bhMQ3au5oY6/PnkSBIqx\u002BrVo3tCy334JcEtftazROIYp\u002BSLfaergrrYGflNo4t7rv05j3k8pbqiivCIRwbOIeKBB1RgVQaIwUnG\u002BYsJxLneLgA2v//zR5dzz4NKqp2puzLtsH2O2AMbaL9WiZU3Gkdi9wz9CqJiq4yVujIn8IgJRkYiWOA0i48WGiQMLUFSD8MgZTaszlRjBGzsaDQyq0FUqp9aO7hAC\u002BmYAXfrAyIqxk33bs0ZO/Jute9KzJmy3sX9ELNWFgpheyVhTVxXUuk9pFcMwxJHA0ICqCdaAO7Zx2E0a1cCSa85k7NfPxi\u002BM4Jg53\u002BTYoybSVgpAoOvjjQRBBRGseLIsePzmN2zr9hazrfW3tq3wcfnZ22/YJ8ZxTNgpL6QRhJRCNh6vCKqRWUXpLdUrJDGhkeFIZytVdnQVyYggVhFVckPquGT64dhKAKq0Ln2NcusO1KpSChYoHIviRfMz4pk39qRnjazAhM8qEn2XWv3wQGeh7AIq8TM3JuX6zq82buvkk/YeSpWArlKF1p3FOKO9uXpr9TyF8w8dydCsFx8D2EKBppyPDQI0CAiKRQTxgu6enNheX0ixtEBfumvyp2tZi2hMx\u002BLfIawIS9/eRFuhzPSLH2Ld5g4QYc6PF9NTsbz6zmaef309gYZVeHh7QUW46YHf8eATK1mxeivX/XIpf/ez/0GA36/azNxrn6Stq8TOriLligUBr1yJNEjoQctz/826\u002Bfex/qFHqbS2senxJ9nw9DP88eFHsNZWGVdUc9ie8sAbJzVNIhIXAq9l1Zot/MOtL3DNd0/l\u002Bvmv0FG0vPvBNjIZj8kT9uMntyzh9keWA5adhRLnfW8h5bIlCAICC8cd9jmumnM8d/xoBjsLFX544/MMa6xj1tyH\u002BMq3HuCuRSsAeKW1iPbyxHFnfo1x35iN7epCRDBWyQ5r5IA5FyGeQ6vwu7RZ/zqZduVHn6ZlPwqR6ionxBXDjJMmsXLNNqZPPYCjD27mnkXLmXrUGDyguSHL03dfRKlcwYrhpgW/AxGMEawV7l20nCXL1jHrlMlcMnsK3/j\u002BIs477TCu\u002BPOpdPSUOWXOfZx83AQQWLmhjRt\u002B8SY3HlvHmOOPwOsuM3xkM5nGBiSTQUslUKX\u002B4IPx/AwW22Z87zlreFfVW5w9fd6be9KwH8YJQTYmFhqm7/HNDdx29WkIUChb/uPZVdz0w9ORCJz3a8hTqATcMP\u002B3vLZiAwtvno1nQgz7q9nHMfXw0fzXkvcplC3zr5nF2OZ6ykHA9fOXcvShozli4oiIUAqbW7vY9NrbDD9yDM37jwMjdHz4Ed6IEYw85US6N7bQuvQ16s49G6OmSUUCX80tMnPezr3RsHaeI64ITPiOE6vw1pqtzL32CS6\u002B8Iscf\u002BRoVJXt7QWWv7uZmxa8yv7Dh/Dv/3ouw4dkUVGMQMYTTj1uAl89bjwCNOQzbGnr5uePLOet1ZtYdPMF\u002BBHGuOZXLpdh1H5jyebyYBWTyzH\u002BnDMREfIj9sOvHxo/n1TsNyse24Er9kbPmjAn5i2SZCBXJAqwrqWNv73uaa6Z92dcfM7RISYorFi9lVsWLOXHc6fx8E/PYWRjPlpQyed9shkv7Oyl7jPnR4/R2t7Dk3deRF3WwyGciKIKo089kWw\u002BH/WDoGHsWIwYVGwbIgybNBGTKk\u002BM6lx9Z2F2b/SsPaw0ImskVbiTSWObWPKri6jL\u002BalejjB96oFMnzqhCqvc3\u002BsuOzXKIikcAx679UJyGQ9sUDXeKTvsc6OSTkf0HBaL9f3L/EBuQu3oOEuFTewsO9rGA2v3pGJt2UpMUiZUlQOJ5HO97K6O9fQyTDTX9zwyvlc9Bchl/cj4rjANf7s1WlaspBxUqFQCQLFqwfNasuo9Scb7tzDyxbUL3re\u002Bt5rhTRv2Rs2aW0MhDqf9xTk8KSdOJCGD7sd1MdJKV3uNC5XdNEHikW1vr\u002BLjlvfo2L6Voiqvfti6vQdzjMyct9OcfvnPyGUuE0/WK0olw\u002BzMzCsOkSMuLO2NjjUCchhOStpjEsxJKxmfU1c29DZbMr93szSkB6mGu7iWaYphW6jL\u002BKx\u002B7Cn\u002BuaWJt7pl5/r3XtkezVfgTlX9BS/eOcqfdtnmvujZr4ar6\u002Ba5fSn32J1dJQrFsutxRkoSKZZqPxACuRGhd2D23qmowg1Venoq0WEIzrZUplQqA7tibWSkPhkG\u002BkkCIWwjaCqj/8v9y3ho8QrKpaiR7rJYr8\u002B4Pam4xoxr8rg4VTWpbRhNYpbQePm4wxitL4B4uwJgjVJjm9TVVmFucIB5\u002B6PL\u002BdXCPzDloAa\u002BNW0cJq57NARxtVUYE2J50nQPbZbaY5Bocy/6cRt9iPD8ik946Y1Nsb3CzWJgFx\u002BsXWrzHBPieLpBpSiLnnuXIybUc8elR5JraMTzM1ThA9Upvyqn9zptSe1YuHtoUuiuaVVeemMTJn4HKgH6gZLaADnx5lgCDBs372DWjAMYOqKZhnEHYMvlVMMwNUNcYziV2xxqIwm\u002BpPEpfU6VbMP2\u002BFDi0MK1CwdEagur1Od459qESgowdMx4sFDc1pJ0/1QRY8LaImqtJoYjDr1kO6ZX7nJ2iq4FXTuj06HxBMHqgNqmxrCKM1OSZn21jB8zjNfW9kQYYhkyZkK1JVWJm\u002BriEb2WQbp9ighiwTVjMYLYqMB17REBr/6TZEkbPo2E4z9bQE5niLilAvzFWVO44e6XmX3VImadcjAZL/R5kfRbEr2pY3qLN/KmKsadjE7Pfev9LcmV1HD5rMMqll5lw7D6HJ5nePu9Ftau3oggeBK\u002BNGIROoOUJR0hdJCyi9lCSZktuS2hBzYYx7ITEBy4XDUQPCf11Pf85g0OyZa466B2sqRQQ4RiucKqlh2MG15PTF5gF2\u002BKESfFf9zvNIz/ur2e/\u002BwM2xGG3v41MFJj\u002BZACSZvQ/p5CwDH5AJ\u002BQ6qf3ugOFjPFoHDcSwfGXCMijloZGRE4wWEMKkyKwtjY2QnaDQofbIHbPoyDmMyaBDpGjulWqriS0v\u002BqlAIB8ltE/uSqiSZEO8Z55eiDJuhEbVPdeXTR22KPL4f5XQts5BtVr06\u002B/UnO2CulG70o5zhm7bYJpqULbr5/AGJMigDZO65oqTqsTeVJcOOlZ2YYJ94ec44WzPmtArt7ES0h7LuOxozPsu6mNWqlhfg43YoISPS\u002B8gmeiuj5lgXSTKy0aV1upc6qU2xtR6kmhFHaXr6p/UmNYBdEzJe0DUeX8M47gp/Nbmf7\u002BcLyIlkikuSjsrDQTdMSvTbjFIhJX3bLQ\u002BLerPKPQij0TGiSIlxEEg8ZbRQMhtXmOmF16LypCoRzukVWy9Qwf2VxVOglQL\u002BkwS2WgFPsNs7xGWV7iqsKKxgWuqtLR3o7d8UlIQqP2aBkDQqUWnXYn/SsfUjuIRuGpFz\u002BgqXkEJ582HRETe0x6vJpofzt6FzCNJRE5SKp0YHehBsKaVavYsOITbNT8eifI82GQo1QJlv7JaX2UmjuBElV8EaSgQGd3kcbhzeRzHt\u002BfOSwEzPSuZJpOV7E6GxajaVKZrqXEYVQMYtyndTy4grg3dE93M4EtflCg9L1adNqd1JitbFT59KJvUQvDN8L\u002B9T6qQs\u002B2jRAEqWLT5Z9U2ECVsYSoceVeq4028Ui9F\u002BSXO8O7Kry\u002Bavs/bvik1FIolxe3rl22Vxt2eyN9No4xWUVMwitSdV61qUJUqRs5OqHRMaDurnZKX6suFsI3vJwbhXPN0JZwP1yEuhd/f/\u002B6dzet76sue5I\u002BG8f3c4pIpS2QbPiscRVKLpuhWCjSVVTueqGdvJe6jGDVxmES/z\u002BEKzbZDcZEBolB2Z0XYcX6bodZmjHlvdpN6LOufZ3w0UfN5YMO37751rbhEw7NRW9wiFBBOH7KeBY\u002B9Rb/u\u002BwP2KlfwPf9qv6Me6e4OkxM8q8AqdzW\u002B/3l8DYJsdzaZrFK0B7ot89bvn1Tf4zwp6QmUjDmwBMOzeZzLxsjze6ctel/Aknwh8SxiKuO\u002BKVsV2pIPDwpBTSZ5GhgLyPZQJatX/3CCQxsMR5LzYxp1KSvjPSN\u002BZKBhgBwe5WOlnkAnheCcfQ5CIJd1ql\u002BGtE0/fd6XY7ePxULipFuOrqfb2lZ3l2rDoMyKIMyKIMyKIMyKIMyKIPSF/k/FjClO\u002BdWYasAAAAASUVORK5CYII="));
        return user;
    }

    public void ImportUserDataFromJson(string filePath)
    {
        try
        {
            JsonNode? uploadedUserData = JsonNode.Parse(File.ReadAllText(filePath));

            if (uploadedUserData is null)
            {
                LOGGER.Warn("File '{}' could not be parsed it is Malformed", filePath);
                return;
            }

            // Ensure schema is updated (optional flag depending on your implementation)
            User.UpdateSchemaVersion(uploadedUserData, true);

            User? newUser = uploadedUserData.Deserialize(UserModelContext.Default.User);
            if (newUser is null)
            {
                LOGGER.Warn("Deserialization returned null User from file '{}'.", filePath);
                return;
            }

            // Backup current data before replacing
            int count = 1;
            string backupFileName;
            do
            {
                backupFileName = $"UserData_Backup_{count}.json";
                count++;
            } while (File.Exists(backupFileName));

            string originalUserDataPath = AppFileHelper.GetUserDataJsonPath(); // This is the user data file you want to modify
            string backupOfOriginalPath = AppFileHelper.GetFilePath(backupFileName); // This is where the old data will be backed up

            try
            {
                // 1. Validate that the source of new content exists
                if (!File.Exists(filePath))
                {
                    LOGGER.Error("ImportUserData failed: Temporary source file with new content not found. File path: {filePath}", filePath);
                    return; // Or throw an exception, as you can't proceed without the new data
                }

                // 2. Make a backup of the original file (if it exists)
                if (File.Exists(originalUserDataPath))
                {
                    try
                    {
                        // Copy the original file to the backup location. Overwrite if backup already exists.
                        File.Copy(originalUserDataPath, backupOfOriginalPath, true);
                        LOGGER.Info("Successfully backed up original user data. Original path: {originalPath}, Backup path: {backupPath}", originalUserDataPath, backupOfOriginalPath);
                    }
                    catch (Exception ex)
                    {
                        // Log as an Error because backup failure is now critical
                        LOGGER.Error(ex, "CRITICAL: Failed to create backup of original user data. Import operation aborted. Original path: {originalPath}, Backup path: {backupPath}. Error: {errorMessage}", originalUserDataPath, backupOfOriginalPath, ex.Message);
                        // Re-throw the exception to ensure the outer catch block handles it and stops the import.
                        throw;
                    }
                }
                else
                {
                    // If original file doesn't exist, no backup is made, but it's not a failure condition for backup itself.
                    LOGGER.Info("Original user data file not found. Path: {originalPath}. No backup created before importing new data.", originalUserDataPath);
                }

                // 3. Write the new content to the original destination path (this will overwrite the old file)
                File.Copy(filePath, originalUserDataPath, true); // 'true' means overwrite the destination file
                LOGGER.Info("Successfully imported new user data. Source path: {sourcePath}, Destination path: {destinationPath}", filePath, originalUserDataPath);

                // 4. Optionally, delete the temporary source file once it's no longer needed
                try
                {
                    File.Delete(filePath);
                    LOGGER.Debug("Deleted temporary user data file. Path: {filePath}", filePath);
                }
                catch (Exception ex)
                {
                    LOGGER.Warn(ex, "Failed to delete temporary user data file. File path: {filePath}. Error: {errorMessage}", filePath, ex.Message);
                }
            }
            catch (IOException ex)
            {
                // This catch block will handle if the File.Copy(new, original, true) fails due to locking,
                // or if the initial backup copy failed and was re-thrown.
                LOGGER.Error(ex, "IOException during user data import. The destination file might be locked by another process. Destination path: {destinationPath}. Error: {errorMessage}", originalUserDataPath, ex.Message);
                // You might want to attempt to restore from backup here if this copy failed and backup was successful
            }
            catch (UnauthorizedAccessException ex)
            {
                LOGGER.Error(ex, "Permission error during user data import. Ensure the application has write access to the destination path. Destination path: {destinationPath}. Error: {errorMessage}", originalUserDataPath, ex.Message);
            }
            catch (Exception ex)
            {
                LOGGER.Error(ex, "An unexpected error occurred during user data import. Error: {errorMessage}", ex.Message);
            }

            // Push to reactive stream
            RefreshSourceCache(newUser);
            _userSubject.OnNext(newUser);
            SaveUserData();

            LOGGER.Info("Successfully imported user data from '{}'. Backup created as '{}'.", filePath, backupFileName);
        }
        catch (JsonException ex)
        {
            LOGGER.Error(ex, "File '{}' is not valid JSON.", filePath);
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Unexpected error during ImportUserData from '{}'.", filePath);
        }
    }

    public IReadOnlyList<Series> GetUserCollection()
    {
        return GetCurrentUserSnapshot().UserCollection;
    }

    public void UpdateSeriesCoverBitmap(Guid seriesId, Bitmap bitmap)
    {
        _userCollectionSourceCache.Lookup(seriesId).IfHasValue(series => series.CoverBitMap = bitmap);
    }

    public void UpdateUser(Action<User> updateAction)
    {
        User user = _userSubject.Value;
        updateAction(user);
        _userSubject.OnNext(user);
        LOGGER.Trace($"User properties updated for: {user.UserName}");
    }

    public User? GetCurrentUserSnapshot()
    {
        return _userSubject.Value;
    }

    public TsundokuTheme? GetCurrentThemeSnapshot()
    {
        return _currentThemeSubject.Value;
    }

    public TsundokuTheme? GetMainTheme()
    {
        return _savedThemes.First(theme => theme.ThemeName.Equals(GetCurrentUserSnapshot().MainTheme));
    }

    /// <summary>
    /// Sets the current theme used by the app without updating the User object
    /// </summary>
    /// <param name="theme">The theme you want to override the current app theme with</param>
    public void OverrideCurrentTheme(TsundokuTheme theme)
    {
        _currentThemeSubject.OnNext(theme);
    }

    public void SetCurrentTheme(TsundokuTheme theme)
    {
        // Also update the User object's MainTheme property
        UpdateUser(user => user.MainTheme = theme.ThemeName);
        if (GetCurrentUserSnapshot().MainTheme.Equals(theme.ThemeName))
        {
            OverrideCurrentTheme(theme);
        }
    }

    public TsundokuLanguage GetLanguage()
    {
        return GetCurrentUserSnapshot().Language;
    }

    public void SetCurrentTheme(string themeName)
    {
        UpdateUser(user => user.MainTheme = themeName);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public void SaveUserData(User user)
    {
        if (user is null) return;

        string userDataFullPath = AppFileHelper.GetUserDataJsonPath();
        LOGGER.Info($"Saving \"{user.UserName}'s\" Collection Data to {userDataFullPath}");

        try
        {
            using FileStream createStream = File.Create(userDataFullPath);
            JsonSerializer.Serialize(createStream, user, User.JSON_SERIALIZATION_OPTIONS);
            createStream.Flush();
            LOGGER.Debug($"Successfully saved user data to: {userDataFullPath}");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, $"Failed to save user data to {userDataFullPath}.");
        }
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public void SaveUserData()
    {
        User user = GetCurrentUserSnapshot();
        if (user is null) return;

        string userDataFullPath = AppFileHelper.GetUserDataJsonPath();
        LOGGER.Info($"Saving \"{user.UserName}'s\" Collection Data to {userDataFullPath}");

        try
        {
            using FileStream createStream = File.Create(userDataFullPath);
            JsonSerializer.Serialize(createStream, user, User.JSON_SERIALIZATION_OPTIONS);
            createStream.Flush();
            LOGGER.Debug($"Successfully saved user data to: {userDataFullPath}");
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, $"Failed to save user data to {userDataFullPath}.");
        }
    }

    public bool DoesDuplicateExist(Series series)
    {
        return _userCollectionSourceCache.Items.Contains(series, new SeriesValueComparer());
    }

    public bool AddSeries(Series? series, bool allowDuplicate = false)
    {
        ArgumentNullException.ThrowIfNull(series);

        if (!allowDuplicate)
        {
            bool customDuplicateExists = DoesDuplicateExist(series);

            if (customDuplicateExists)
            {
                LOGGER.Warn("A duplicate series (based on Romaji, & Japanese Titles + Format) already exists in the collection. Skipping add for {SeriesTitle}.", series.Titles.GetValueOrDefault(TsundokuLanguage.Romaji, "Unknown Title"));
                series.Dispose();
                return false;
            }
        }

        _userCollectionSourceCache.AddOrUpdate(series);

        UpdateUser(user =>
        {
            user.UserCollection.Add(series);
        });

        LOGGER.Debug("Added {series} ({id}) to Collection", series.Titles[TsundokuLanguage.Romaji] + (series.DuplicateIndex == 0 ? string.Empty : $" ({series.DuplicateIndex})"), series.Id);
        return true;
    }

    public bool RefreshSeries(Series originalSeries, Series? refreshedSeries)
    {
        if (refreshedSeries is null)
        {
            LOGGER.Warn("{title} returned a null series entry on refresh", originalSeries.Titles[TsundokuLanguage.Romaji]);
        }
        else if (_userCollectionSourceCache.Lookup(originalSeries.Id).HasValue)
        {
            bool isCoverEmpty = originalSeries.IsCoverImageEmpty();
            originalSeries.UpdateFrom(refreshedSeries, isCoverEmpty);

            UpdateUser(user =>
            {
                int index = user.UserCollection.BinarySearch(originalSeries, new SeriesComparer(user.Language));
                index = index < 0 ? ~index : index;
                user.UserCollection.RemoveAt(index);
                user.UserCollection.Insert(index, originalSeries);
            });

            LOGGER.Info("Refreshed {series} ({id} | {CoverEmpty?}) in Collection", originalSeries.Titles[TsundokuLanguage.Romaji] + (originalSeries.DuplicateIndex == 0 ? string.Empty : $" ({originalSeries.DuplicateIndex})"), originalSeries.Id, isCoverEmpty);
            return true;
        }
        
        refreshedSeries.Cover = string.Empty;
        refreshedSeries?.Dispose();
        return false;
    }

    public void RemoveSeries(Series series)
    {
        if (series is null)
        {
            return;
        }

        if (_userCollectionSourceCache.Lookup(series.Id).HasValue)
        {
            if (_userCollectionSourceCache.Count > 0)
            {
                _userCollectionSourceCache.Remove(series.Id);
            }
            else
            {
                _userCollectionSourceCache.Clear();
            }
        }
        else
        {
            LOGGER.Warn($"Series with ID {series.Id} not found in cache before removal.");
        }

        UpdateUser(user =>
        {
            user.UserCollection.Remove(series);
        });

        series?.Dispose();
        LOGGER.Info("Removed {series} ({id}) from Collection", series.Titles[TsundokuLanguage.Romaji] + (series.DuplicateIndex == 0 ? string.Empty : $" ({series.DuplicateIndex})"), series.Id);
    }

    public void AddTheme(TsundokuTheme theme)
    {
        if (theme is null)
        {
            throw new ArgumentNullException(nameof(theme), "Theme cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(theme.ThemeName))
        {
            throw new ArgumentException("Theme name cannot be null, empty, or whitespace.", nameof(theme));
        }

        if (theme.ThemeName.Equals("Default", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Theme name cannot be 'Default'.", nameof(theme));
        }

        if (theme.ThemeName.Length > 60)
        {
            throw new ArgumentException("Theme name must be 60 characters or fewer.", nameof(theme));
        }

        // Add theme to the source cache and update user
        _savedThemesSourceCache.AddOrUpdate(theme);

        UpdateUser(user =>
        {
            // Instantiate the comparer once for this operation
            TsundokuThemeComparer comparer = new TsundokuThemeComparer(user.Language);
            int insertionIndex = user.SavedThemes.BinarySearch(theme, comparer);

            if (insertionIndex >= 0)
            {
                // Remove the existing theme from its current position.
                user.SavedThemes.RemoveAt(insertionIndex);
                LOGGER.Debug($"Removed existing theme '{theme.ThemeName}' at index {insertionIndex} for update");
            }
            else
            {
                insertionIndex = ~insertionIndex;
            }

            // Insert the new/updated theme at the determined sorted position.
            user.SavedThemes.Insert(insertionIndex, theme);
            user.MainTheme = theme.ThemeName;
            LOGGER.Debug($"Inserted theme '{theme.ThemeName}' at index {insertionIndex}");
        });
        
        // _currentThemeSubject.OnNext(theme);
        LOGGER.Info("Added or Updated {theme} theme", theme.ThemeName);
    }

    public void RemoveTheme(TsundokuTheme theme)
    {
        if (theme is null)
            return;

        if (_savedThemesSourceCache.Count <= 1)
        {
            LOGGER.Warn($"Cannot remove theme '{theme.ThemeName}'. There must be at least one theme saved");
            throw new InvalidOperationException("Cannot remove the last remaining theme. There must be at least one theme saved");
        }

        // Find the next available theme *before* removing the current one
        TsundokuTheme? newCurrentTheme = _savedThemes
            .Where(t => !t.ThemeName.Equals(theme.ThemeName, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault();

        if (newCurrentTheme is not null)
        {
            SetCurrentTheme(newCurrentTheme);
            LOGGER.Debug($"Switched current theme to '{newCurrentTheme.ThemeName}' before removal");
        }
        else
        {
            LOGGER.Error("No other themes found to switch to after attempting to remove the current theme");
        }

        if (_savedThemesSourceCache.Lookup(theme.ThemeName).HasValue)
        {
            _savedThemesSourceCache.Remove(theme.ThemeName);
            UpdateUser(user =>
            {
                user.SavedThemes.Remove(theme);
            });
            LOGGER.Info("Removed {theme} Theme", theme.ThemeName);
        }
        else
        {
            LOGGER.Warn($"Attempted to remove theme '{theme.ThemeName}' which was not found in saved themes");
        }
    }

    public void RemoveTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            throw new ArgumentNullException(nameof(themeName), "Theme name cannot be null or empty.");
        }

        TsundokuTheme? themeToRemove = _savedThemesSourceCache.Lookup(themeName).Value;

        if (themeToRemove is not null)
        {
            RemoveTheme(themeToRemove);
        }
        else
        {
            LOGGER.Warn($"Theme '{themeName}' not found. Cannot remove");
        }
    }
    
    public void ClearUserCollection() => _userCollectionSourceCache.Clear();

    public void ExportTheme(string fileName)
    {
        TsundokuTheme theme = _currentThemeSubject.Value;
        if (theme is null)
            throw new ArgumentNullException(nameof(theme));

        string themesFolderPath = AppFileHelper.GetThemesFolderPath();
        string themeFileName = $"{fileName}_Theme.json";
        string themeFullPath = Path.Combine(themesFolderPath, themeFileName);

        using FileStream createStream = File.Create(themeFullPath);
        JsonSerializer.Serialize(createStream, theme, TsundokuThemeModelContext.Default.TsundokuTheme);
        createStream.Flush();
    }

    public async Task ImportThemeAsync(string themeFilePath)
    {
        if (string.IsNullOrWhiteSpace(themeFilePath))
        {
            LOGGER.Error("Theme file path cannot be null or empty for import");
            throw new ArgumentNullException(nameof(themeFilePath));
        }

        if (!File.Exists(themeFilePath))
        {
            LOGGER.Error("Theme file not found at {themeFilePath}", themeFilePath);
            throw new FileNotFoundException("Theme file not found.", themeFilePath);
        }

        if (!themeFilePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            LOGGER.Error("Invalid file type for theme import: {theneFilePath}. Expected a .json file.", themeFilePath);
            throw new ArgumentException("Only .json files can be imported as themes.", nameof(themeFilePath));
        }

        try
        {
            await using FileStream openStream = File.OpenRead(themeFilePath);
            TsundokuTheme? importedTheme = await JsonSerializer.DeserializeAsync(openStream, TsundokuThemeModelContext.Default.TsundokuTheme);

            if (importedTheme is not null)
            {
                AddTheme(importedTheme); // This will also set it as the current theme and persist
                LOGGER.Info("Successfully imported and set theme '{name}' from {themeFilePath}", importedTheme.ThemeName, themeFilePath);
            }
            else
            {
                LOGGER.Error($"Deserialization failed for theme file: {themeFilePath}. The file might be corrupted or malformed.");
                throw new JsonException("Failed to deserialize theme file. Invalid JSON format.");
            }
        }
        catch (JsonException jEx)
        {
            LOGGER.Error(jEx, "JSON format error when importing theme from {themeFilePath}", themeFilePath);
            throw new InvalidDataException("The selected file is not a valid theme JSON.", jEx);
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "An unexpected error occurred while importing theme from {themeFilePath}", themeFilePath);
            throw; // Re-throw the exception after logging
        }
    }

    public void UpdateUserIcon(string filePath)
    {
        User currentUser = GetCurrentUserSnapshot();
        if (currentUser is null)
        {
            LOGGER.Warn("Attempted to update user icon, but no user is currently loaded.");
            return;
        }
        
        currentUser.UserIcon = new Bitmap(filePath).CreateScaledBitmap(new PixelSize(USER_ICON_WIDTH, USER_ICON_HEIGHT), BitmapInterpolationMode.HighQuality);

        _userSubject.OnNext(currentUser);
        LOGGER.Info("Updated UserIcon.");
    }

    // This is the common Dispose pattern (Dispose(bool disposing))
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _disposables.Dispose();

                _userSubject.OnCompleted();
                _userSubject.Dispose();
                _userCollectionSourceCache.Dispose();

                _currentThemeSubject.OnCompleted();
                _currentThemeSubject.Dispose();
                _savedThemesSourceCache.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}