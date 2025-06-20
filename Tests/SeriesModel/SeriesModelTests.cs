using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Tsundoku.ViewModels;
using static Tsundoku.Clients.AniList;
using static Tsundoku.Models.TsundokuLanguageModel;

[assembly: Description("Testing Series Model from Tsundoku")]
namespace Tsundoku.Tests.SeriesModel
{
    [Author("Sean (Alias -> Prem or Sigrec)")]
    [TestOf(typeof(Series))]
    [Description("Testing Series Model")]
    [TestFixture]
    public class SeriesModelTests
    {
        public static readonly JsonSerializerOptions options = new()
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        private BitmapHelper _bitmapHelper;
        private Clients.MangaDex _mangaDex;
        private Clients.AniList _aniList;
        private IServiceProvider _serviceProvider;
        private Series _series;

        [OneTimeSetUp]
        public void Setup()
        {
            App.ConfigureNLogToUseLocalCacheFolder();

            // 1. Build a minimal DI container that mirrors your app's setup
            ServiceCollection services = new ServiceCollection();

            // Register IHttpClientFactory for default clients (e.g., if BitmapHelper uses an unnamed client)
            services.AddHttpClient("AddCoverClient", client =>
            {
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));
                client.DefaultRequestVersion = HttpVersion.Version20;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                client.Timeout = TimeSpan.FromSeconds(30);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            // Register the named HttpClient for MangaDexApi as configured in your Program.cs
            services.AddHttpClient("MangaDexClient", client =>
            {
                client.BaseAddress = new Uri("https://api.mangadex.org/");
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));
                client.Timeout = TimeSpan.FromSeconds(30);
            }).SetHandlerLifetime(TimeSpan.FromMinutes(5));

            // --- AniList GraphQL Client Setup ---
            services.AddHttpClient("AniListHttpClient", client =>
            {
                client.BaseAddress = new Uri("https://graphql.anilist.co");
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestVersion = HttpVersion.Version30;
                client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku", ViewModelBase.CUR_TSUNDOKU_VERSION));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));
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
            _series = null;
        }

        [AvaloniaTest]
        [Ignore("Weird Entry, not working")]
        public async Task NotOnMangaDex_AniList_Test()
        {
            _series = await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "-0.5˚C", Format.Manga, 5, 0, []);
            Assert.That(_series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tests\\SeriesModel\SeriesModelTestData\0.5C.json")));
        }

        [AvaloniaTest]
        public async Task Novel_AniList_Test()
        {
            try
            {
                string actual = (await Series.CreateNewSeriesCardAsync(
                    bitmapHelper: _bitmapHelper,
                    mangaDex: _mangaDex,
                    aniList: _aniList,
                    title: "ようこそ実力至上主義の教室へ",
                    bookType: Format.Novel,
                    maxVolCount: 14,
                    minVolCount: 0,
                    additionalLanguages: [],
                    publisher: "Unknown",
                    demographic: Constants.Demographic.Seinen,
                    volumesRead: 4,
                    rating: (decimal)5.6,
                    value: (decimal)5000.65
                ))?.ToString();

                string expected = await File.ReadAllTextAsync(GetTestFilePath(@"SeriesModelTestData\CoTE.json"));

                Assert.That(actual, Is.EqualTo(expected));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test failed: {ex}");
                throw;
            }
        }

        [AvaloniaTest]
        public async Task MangaDexID_HasNativeStaff_Test()
        {
            _series = await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "32d76d19-8a05-4db0-9fc2-e0b0648fe9d0", Format.Manga, 2, 0, []);
            Assert.That(_series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tests\\SeriesModel\SeriesModelTestData\SoloLeveling.json")));
        }

        [AvaloniaTest]
        public async Task MangaDexID_NoNativeStaff_Test()
        {
            _series = await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "c0189f4a-dee6-48f4-abbe-53a4359cbcfb", Format.Manga, 2, 0, []);
            Assert.That(_series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tests\\SeriesModel\SeriesModelTestData\AtTheNorthernFort.json")));
        }

        [AvaloniaTest]
        public async Task MultipleAdditionalLangTitle_AniListLink_Test()
        {
            _series = await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "Rent-A-Girlfriend", Format.Manga, 18, 0, [TsundokuLanguage.Arabic, TsundokuLanguage.Chinese, TsundokuLanguage.French, TsundokuLanguage.Korean, TsundokuLanguage.Russian, TsundokuLanguage.Spanish]);
            Assert.That(_series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tests\\SeriesModel\SeriesModelTestData\Rent-A-Girlfriend.json")));
        }

        [AvaloniaTest]
        public async Task SimilarNotEquals_AniList_Test()
        {
            _series = await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "dont toy with me miss nagatoro", Format.Manga, 5, 0, []);
            Assert.That(_series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tests\\SeriesModel\SeriesModelTestData\IjiranaideNagatoro-san.json")));
        }

        [AvaloniaTest]
        public async Task Radiant_MangaDexTitle_Test()
        {
            _series = await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "Radiant", Format.Manga, 18, 0, []);
            Assert.That(_series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tests\\SeriesModel\SeriesModelTestData\Radiant.json")));
        }

        [AvaloniaTest]
        public async Task TheBeginningAfterTheEnd_NoDemographic_MangaDexTitle_Test()
        {
            _series = await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "The Beginning After The End", Format.Manga, 5, 0, []);
            Assert.That(_series.ToString(), Is.EqualTo(await File.ReadAllTextAsync(@"\Tests\\SeriesModel\SeriesModelTestData\TBATE.json")));
        }

        [Test]
        public async Task IdenticalSeriesNames_GetSeriesByID_Test()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That((await _aniList.GetSeriesByIDAsync(98282, Format.Manga, 1)).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").GetString(), Is.EqualTo("Getsuyoubi no Tawawa"));
                Assert.That((await _aniList.GetSeriesByIDAsync(125854, Format.Manga, 1)).RootElement.GetProperty("Media").GetProperty("title").GetProperty("romaji").GetString(), Is.EqualTo("Getsuyoubi no Tawawa"));
            }
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetCorrectFormat_Test()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Series.GetCorrectFormat("JP"), Is.EqualTo(Format.Manga));
                Assert.That(Series.GetCorrectFormat("KR"), Is.EqualTo(Format.Manhwa));
                Assert.That(Series.GetCorrectFormat("CN"), Is.EqualTo(Format.Manhua));
                Assert.That(Series.GetCorrectFormat("TW"), Is.EqualTo(Format.Manhua));
                Assert.That(Series.GetCorrectFormat("FR"), Is.EqualTo(Format.Manfra));
                Assert.That(Series.GetCorrectFormat("EN"), Is.EqualTo(Format.Comic));
            }
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetSeriesStatus_Test()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Series.GetSeriesStatus("RELEASING"), Is.EqualTo(Constants.Status.Ongoing));
                Assert.That(Series.GetSeriesStatus("NOT_YET_RELEASED"), Is.EqualTo(Constants.Status.Ongoing));
                Assert.That(Series.GetSeriesStatus("ongoing"), Is.EqualTo(Constants.Status.Ongoing));
                Assert.That(Series.GetSeriesStatus("FINISHED"), Is.EqualTo(Constants.Status.Finished));
                Assert.That(Series.GetSeriesStatus("completed"), Is.EqualTo(Constants.Status.Finished));
                Assert.That(Series.GetSeriesStatus("CANCELLED"), Is.EqualTo(Constants.Status.Cancelled));
                Assert.That(Series.GetSeriesStatus("cancelled"), Is.EqualTo(Constants.Status.Cancelled));
                Assert.That(Series.GetSeriesStatus("HIATUS"), Is.EqualTo(Constants.Status.Hiatus));
                Assert.That(Series.GetSeriesStatus("hiatus"), Is.EqualTo(Constants.Status.Hiatus));
                Assert.That(Series.GetSeriesStatus("UNICORN"), Is.EqualTo(Constants.Status.Error));
            }
        }

        [Test]
        [Parallelizable(scope: ParallelScope.Self)]
        public void GetSeriesDemographics_Test()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(Series.GetSeriesDemographic("shounen"), Is.EqualTo(Constants.Demographic.Shounen));
                Assert.That(Series.GetSeriesDemographic("Shounen"), Is.EqualTo(Constants.Demographic.Shounen));
                Assert.That(Series.GetSeriesDemographic("shoujo"), Is.EqualTo(Constants.Demographic.Shoujo));
                Assert.That(Series.GetSeriesDemographic("Shoujo"), Is.EqualTo(Constants.Demographic.Shoujo));
                Assert.That(Series.GetSeriesDemographic("josei"), Is.EqualTo(Constants.Demographic.Josei));
                Assert.That(Series.GetSeriesDemographic("Josei"), Is.EqualTo(Constants.Demographic.Josei));
                Assert.That(Series.GetSeriesDemographic("seinen"), Is.EqualTo(Constants.Demographic.Seinen));
                Assert.That(Series.GetSeriesDemographic("Seinen"), Is.EqualTo(Constants.Demographic.Seinen));
            }
        }

        //[AvaloniaTest]
        //public async Task GetSeriesStaff_Anthology_Test()
        //{
        //    //Lycoris Recoil Koushiki Comic Anthology: Repeat
        //    Assert.That((await Series.CreateNewSeriesCardAsync(_bitmapHelper, _mangaDex, _aniList, "リコリス・リコイル 公式コミックアンソロジー リピート", Format.Manga, 5, 0, [])).Staff[TsundokuLanguage.Romaji], Is.EqualTo("Takeshi Kojima | Mekimeki | Nyoijizai | GUNP | Itsuki Takano | Ren Sakuragi | sometime | Ryou Niina | Ginmoku | Mikaduchi | Nikomi Wakadori | Miki Morinaga | Raika Suzumi | Ree | Itsuki Tsutsui | Utashima | Shirou Urayama | Bonryuu | Yasuka Manuma | Yuichi | Marco Nii | Nana Komado | Yuu Kimura | Sugar.Kirikanoko | Atto | Kasumi Fukagawa | Tiv | Sou Hamayumiba | Kanari Abe | Nachi Aono | Koruse"));
        //}

        private static string GetTestFilePath(string relativePath, [CallerFilePath] string? callerFilePath = null)
        {
            if (callerFilePath is null)
            {
                throw new ArgumentNullException(nameof(callerFilePath));
            }

            string testDirectory = Path.GetDirectoryName(callerFilePath)!;
            return Path.Combine(testDirectory, relativePath);
        }
    }
}