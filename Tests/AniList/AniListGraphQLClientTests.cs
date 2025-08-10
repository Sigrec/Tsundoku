using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using static Tsundoku.Models.Enums.SeriesFormatModel;

namespace Tsundoku.Tests.AniList;

[TestFixture]
public class AniListGraphQLClientTests
{
    private HttpClient _httpClient;
    private Clients.AniList _aniList;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://graphql.anilist.co"),
            Timeout = TimeSpan.FromSeconds(30),
            DefaultRequestVersion = HttpVersion.Version30,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku-Test", "1.0"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _aniList = new Clients.AniList(new AniListGraphQLClient(_httpClient));
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        _httpClient.Dispose();
    }

    [TestCase("ようこそ実力至上主義の教室へ", SeriesFormat.Novel)]
    [TestCase("-0.5˚C", SeriesFormat.Manga)]
    [TestCase("!", SeriesFormat.Manga)]
    [TestCase("오늘만 사는 기사", SeriesFormat.Manga)]
    [TestCase("Tian Guan Ci Fu", SeriesFormat.Manga)]
    public async Task GetSeriesByTitleAsync_ContainsExpectedFields(string title, SeriesFormat format)
    {
        JsonDocument? result = await _aniList.GetSeriesByTitleAsync(title, format, pageNum: 1);
        Assert.That(result, Is.Not.Null, "Expected non-null result from AniList query.");
        JsonElement root = result!.RootElement;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(root.TryGetProperty("Media", out JsonElement mediaElem), Is.True, "Missing 'Media' property.");
            Assert.That(mediaElem.ValueKind, Is.Not.EqualTo(JsonValueKind.Null), "Media is null.");

            Assert.That(mediaElem.TryGetProperty("id", out _));
            Assert.That(mediaElem.TryGetProperty("countryOfOrigin", out _));

            Assert.That(mediaElem.TryGetProperty("title", out JsonElement titleElem));
            Assert.That(titleElem.TryGetProperty("romaji", out _));
            Assert.That(titleElem.TryGetProperty("english", out _));
            Assert.That(titleElem.TryGetProperty("native", out _));

            Assert.That(mediaElem.TryGetProperty("staff", out JsonElement staffElem));
            Assert.That(staffElem.TryGetProperty("edges", out JsonElement edgesElem));

            foreach (JsonElement edge in edgesElem.EnumerateArray())
            {
                Assert.That(edge.TryGetProperty("role", out _));
                Assert.That(edge.TryGetProperty("node", out JsonElement nodeElem));
                Assert.That(nodeElem.TryGetProperty("name", out JsonElement nameElem));
                Assert.That(nameElem.TryGetProperty("full", out _));
                Assert.That(nameElem.TryGetProperty("native", out _));
                Assert.That(nameElem.TryGetProperty("alternative", out _));
            }

            Assert.That(mediaElem.TryGetProperty("genres", out _));
            Assert.That(mediaElem.TryGetProperty("description", out _));
            Assert.That(mediaElem.TryGetProperty("status", out _));
            Assert.That(mediaElem.TryGetProperty("siteUrl", out _));
            Assert.That(mediaElem.TryGetProperty("coverImage", out JsonElement coverElem));
            Assert.That(coverElem.TryGetProperty("extraLarge", out _));
        }
    }

    [Test]
    public async Task GetSeriesByIDAsync_ContainsExpectedFields()
    {
        JsonDocument? result = await _aniList.GetSeriesByIDAsync(128067, SeriesFormat.Manga, pageNum: 1);
        Assert.That(result, Is.Not.Null, "Expected non-null result from AniList query.");

        JsonElement root = result!.RootElement;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(root.TryGetProperty("Media", out JsonElement mediaElem), Is.True, "Missing 'Media' property.");
            Assert.That(mediaElem.ValueKind, Is.Not.EqualTo(JsonValueKind.Null), "Media is null.");

            Assert.That(mediaElem.TryGetProperty("id", out _));
            Assert.That(mediaElem.TryGetProperty("countryOfOrigin", out _));

            Assert.That(mediaElem.TryGetProperty("title", out JsonElement titleElem));
            Assert.That(titleElem.TryGetProperty("romaji", out _));
            Assert.That(titleElem.TryGetProperty("english", out _));
            Assert.That(titleElem.TryGetProperty("native", out _));

            Assert.That(mediaElem.TryGetProperty("staff", out JsonElement staffElem));
            Assert.That(staffElem.TryGetProperty("edges", out JsonElement edgesElem));

            foreach (JsonElement edge in edgesElem.EnumerateArray())
            {
                Assert.That(edge.TryGetProperty("role", out _));
                Assert.That(edge.TryGetProperty("node", out JsonElement nodeElem));
                Assert.That(nodeElem.TryGetProperty("name", out JsonElement nameElem));
                Assert.That(nameElem.TryGetProperty("full", out _));
                Assert.That(nameElem.TryGetProperty("native", out _));
                Assert.That(nameElem.TryGetProperty("alternative", out _));
            }

            Assert.That(mediaElem.TryGetProperty("genres", out _));
            Assert.That(mediaElem.TryGetProperty("description", out _));
            Assert.That(mediaElem.TryGetProperty("status", out _));
            Assert.That(mediaElem.TryGetProperty("siteUrl", out _));
            Assert.That(mediaElem.TryGetProperty("coverImage", out JsonElement coverElem));
            Assert.That(coverElem.TryGetProperty("extraLarge", out _));
        }
    }
}
