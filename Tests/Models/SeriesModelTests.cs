using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using Tsundoku.ViewModels;
using static Tsundoku.Models.Enums.SeriesDemographicModel;
using static Tsundoku.Models.Enums.SeriesFormatModel;
using static Tsundoku.Models.Enums.SeriesGenreModel;
using static Tsundoku.Models.Enums.SeriesStatusModel;
using static Tsundoku.Models.Enums.TsundokuLanguageModel;

namespace Tsundoku.Tests.Models;

[TestFixture]
[NonParallelizable]
public class SeriesModelTests
{
    private BitmapHelper _bitmapHelper;
    private Clients.MangaDex _mangaDex;
    private Clients.AniList _aniList;
    private IServiceProvider _serviceProvider;
    private Series? _series;

    [OneTimeSetUp]
    public void Setup()
    {
        // 1. Build a minimal DI container that mirrors your app's setup
        ServiceCollection services = new ServiceCollection();

        // Register IHttpClientFactory for default clients (e.g., if BitmapHelper uses an unnamed client)
        services.AddHttpClient("AddCoverClient", client =>
        {
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;

            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku-Test", ViewModelBase.CUR_TSUNDOKU_VERSION));
            client.Timeout = TimeSpan.FromSeconds(30);
        }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Register the named HttpClient for MangaDexApi as configured in your Program.cs
        services.AddHttpClient("MangaDexClient", client =>
        {
            client.BaseAddress = new Uri("https://api.mangadex.org/");
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku-Test", ViewModelBase.CUR_TSUNDOKU_VERSION));
            client.Timeout = TimeSpan.FromSeconds(30);
        }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // --- AniList GraphQL Client Setup ---
        services.AddHttpClient("AniListHttpClient", client =>
        {
            client.BaseAddress = new Uri("https://graphql.anilist.co");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku-Test", ViewModelBase.CUR_TSUNDOKU_VERSION));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("RequestType", "POST");
            client.DefaultRequestHeaders.Add("ContentType", "application/json");
        }).SetHandlerLifetime(TimeSpan.FromMinutes(5))
          .AddTypedClient<AniListGraphQLClient>();

        services.AddSingleton<BitmapHelper>();
        services.AddSingleton<Clients.MangaDex>();
        services.AddSingleton<Clients.AniList>();

        _serviceProvider = services.BuildServiceProvider();

        // 2. Resolve both real services from the service provider
        _bitmapHelper = _serviceProvider.GetRequiredService<BitmapHelper>();
        _mangaDex = _serviceProvider.GetRequiredService<Clients.MangaDex>();
        _aniList = _serviceProvider.GetRequiredService<Clients.AniList>();
    }

    [OneTimeTearDown]
    public void OneTimeCleanup()
    {
        (_serviceProvider as IDisposable)?.Dispose();
    }

    [TearDown]
    public void Cleanup()
    {
        _series?.Dispose();
    }

    [AvaloniaTest]
    public async Task CreateNewSeriesCardAsync_IncludesAllRequestedAdditionalLanguages()
    {
        string input = "Rent-A-Girlfriend";
        TsundokuLanguage[] requestedLangs =
        [
            TsundokuLanguage.German,
            TsundokuLanguage.Chinese,
            TsundokuLanguage.French,
            TsundokuLanguage.Korean,
            TsundokuLanguage.Russian,
            TsundokuLanguage.Spanish
        ];

        _series = await Series.CreateNewSeriesCardAsync(
            bitmapHelper: _bitmapHelper,
            mangaDex: _mangaDex,
            aniList: _aniList,
            input: input,
            bookType: SeriesFormat.Manga,
            maxVolCount: 18,
            minVolCount: 0,
            additionalLanguages: requestedLangs
        );

        Assert.That(_series, Is.Not.Null, "Expected non-null Series");

        foreach (TsundokuLanguage lang in requestedLangs)
        {
            Assert.That(
                _series!.Titles.ContainsKey(lang),
                $"Missing expected language: {lang}"
            );
        }
    }

    [AvaloniaTest]
    public async Task CreateNewSeriesCardAsync_Radiant_HasJapaneseTitle()
    {
        _series = await Series.CreateNewSeriesCardAsync(
            bitmapHelper: _bitmapHelper,
            mangaDex: _mangaDex,
            aniList: _aniList,
            input: "Radiant",
            bookType: SeriesFormat.Manga,
            maxVolCount: 10,
            minVolCount: 0,
            additionalLanguages: []
        );

        Assert.That(_series, Is.Not.Null, "Expected non-null Series");

        bool hasJapanese = _series!.Titles.TryGetValue(TsundokuLanguage.Japanese, out var jpTitle);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(hasJapanese, Is.True, "Japanese title key not found");
            Assert.That(jpTitle, Is.EqualTo("ラディアン"), "Japanese title did not match expected value");
        }
    }

    [AvaloniaTest]
    public async Task CreateNewSeriesCardAsync_Anthology_HasFullStaffList()
    {
        App.ConfigureNLog();
        string input = "157671";
        string expectedFullStaff = "Takeshi Kojima | Mekimeki | Nyoijizai | GUNP | Itsuki Takano | Ren Sakuragi | sometime | Ryou Niina | Ginmoku | Mikaduchi | Nikomi Wakadori | Miki Morinaga | Raika Suzumi | Ree | Itsuki Tsutsui | Utashima | Shirou Urayama | Bonryuu | Yasuka Manuma | Yuichi | Marco Nii | Nana Komado | Yuu Kimura | Sugar.Kirikanoko | AttoKasumi Fukagawa | Atto | Tiv | Sou Hamayumiba | Kanari Abe | Nachi Aono | Koruse";
        string expectedNativeStaff = "こじまたけし | めきめき | 如意自在 | GUNP | 高野いつき | 桜木蓮 | そめちめ | にいな涼 | ぎんもく | みかづち | 若鶏にこみ | 森永ミキ | 涼海来夏 | れぇ | 筒井いつき | うたしま | しろううらやま | 凡竜 | 真沼靖佳 | ゆいち | 弐尉マルコ | 奈々鎌土 | キ村由宇 | Sugar.栗かのこ | あっと深川可純 | あっと | Tiv | 浜弓場双 | 阿部かなり | あおのなち | こるせ";

        _series = await Series.CreateNewSeriesCardAsync(
            bitmapHelper: _bitmapHelper,
            mangaDex: _mangaDex,
            aniList: _aniList,
            input: input,
            bookType: SeriesFormat.Manga,
            maxVolCount: 5,
            minVolCount: 0,
            additionalLanguages: []
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_series, Is.Not.Null, "Series creation failed");
            Assert.That(_series!.Staff.ContainsKey(TsundokuLanguage.Romaji), Is.True, "Missing Romaji staff");
            Assert.That(_series.Staff[TsundokuLanguage.Romaji], Is.EqualTo(expectedFullStaff));
            Assert.That(_series!.Staff.ContainsKey(TsundokuLanguage.Japanese), Is.True, "Missing Native staff");
            Assert.That(_series.Staff[TsundokuLanguage.Japanese], Is.EqualTo(expectedNativeStaff));
        }
    }

    [AvaloniaTest]
    public void Series_EqualsAndHashCode_WorkCorrectly()
    {
        Guid sameId = Guid.Parse("12345678-1234-5678-1234-567812345678");

        Series a = CreateDummySeries(sameId);
        Series b = CreateDummySeries(sameId);
        Series c = CreateDummySeries(); // different ID

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.EqualTo(c));
            Assert.That(a, Is.Not.Null);
            Assert.That(a, Is.EqualTo(b));
            Assert.That(a, Is.Not.Null);
            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }
    }

    [AvaloniaTest]
    public void Series_UpdateFrom_UpdatesCorrectly()
    {
        Series original = CreateDummySeries();
        Series updated = CreateDummySeries();
        updated.Description = "New description";
        updated.CurVolumeCount = 99;

        original.UpdateFrom(updated, false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(original.Description, Is.EqualTo("New description"));
            Assert.That(original.CurVolumeCount, Is.EqualTo(99));
            Assert.That(original.Titles, Is.EqualTo(updated.Titles));
            Assert.That(original.Staff, Is.EqualTo(updated.Staff));
        }
    }

    [AvaloniaTest]
    public void Series_AdditionalLanguagesDetection_Works()
    {
        Series series = CreateDummySeries();
        series.Titles[TsundokuLanguage.Korean] = "타이틀";
        series.Titles[TsundokuLanguage.French] = "Titre";

        TsundokuLanguage[] langs = series.SeriesContainsAdditionalLanagues();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(langs, Does.Contain(TsundokuLanguage.Korean));
            Assert.That(langs, Does.Contain(TsundokuLanguage.French));
            Assert.That(langs, Has.Length.EqualTo(2));
        }
    }

    [AvaloniaTest]
    public void Series_Dispose_And_DeleteCover_Safe()
    {
        Series series = CreateDummySeries();
        Assert.DoesNotThrow(series.Dispose);
        Assert.That(series.CoverBitMap, Is.Null);
    }

    [AvaloniaTest]
    public void Series_ToString_SerializesCorrectly()
    {
        Series series = CreateDummySeries(Guid.Parse("12345678-1234-5678-1234-567812345678"));

        string json = series.ToString();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(json, Does.Contain("Title"));
            Assert.That(json, Does.Contain("SamplePub"));
        }
    }

    private static Series CreateDummySeries(Guid? id = null)
    {
        WriteableBitmap emptyBitmap = new WriteableBitmap(
            new PixelSize(1, 1),                  // 1x1 pixel
            new Vector(96, 96),                   // DPI
            PixelFormat.Bgra8888,
            AlphaFormat.Premul  // Premultiplied alpha
        );

        return new Series(
            Titles: new() { [TsundokuLanguage.English] = "Title" },
            Staff: new() { [TsundokuLanguage.English] = "Staff" },
            Description: "A sample description",
            Format: SeriesFormat.Manga,
            Status: SeriesStatus.Ongoing,
            Cover: "cover/path.jpg",
            Link: new Uri("https://example.com"),
            Genres: [SeriesGenre.Action, SeriesGenre.Fantasy],
            MaxVolumeCount: 10,
            CurVolumeCount: 5,
            Rating: 9.5m,
            VolumesRead: 3,
            Value: 49.99m,
            Demographic: SeriesDemographic.Shounen,
            CoverBitMap: emptyBitmap,
            Publisher: "SamplePub",
            DuplicateIndex: 1
        )
        {
            Id = id ?? Guid.NewGuid()
        };
    }
}
