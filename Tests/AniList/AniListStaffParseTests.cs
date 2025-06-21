using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Tsundoku.Clients;
using static Tsundoku.Models.Enums.SeriesFormatEnum;

namespace Tsundoku.Tests.AniList;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class AniListStaffParseTests
{
    // TODO - Need a test for "リコリス・リコイル 公式コミックアンソロジー リピート" anthology
    [TestCase("リコリス・リコイル 公式コミックアンソロジー リピート", SeriesFormat.Manga)]
    public async Task ExtractStaffFromAniList_ShouldReturnCorrectStaffString_Anthology(string title, SeriesFormat format)
    {
        HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://graphql.anilist.co"),
            Timeout = TimeSpan.FromSeconds(30),
            DefaultRequestVersion = HttpVersion.Version30,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher
        };
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku-Test", "1.0"));
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        Clients.AniList aniList = new Clients.AniList(new AniListGraphQLClient(httpClient));

        JsonDocument? result = await aniList.GetSeriesByTitleAsync(title, format, pageNum: 1);
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

        httpClient.Dispose();
    }

    [Test]
    public async Task ExtractStaffFromAniList_ShouldReturnCorrectStaffString_WhenNotAnthology()
    {
        string json = await File.ReadAllTextAsync(@"AniList\AniListTestData\ToManyStaffIllustrators.json");
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string nativeStaff = string.Empty;
        string fullStaff = string.Empty;

        Clients.AniList.ExtractStaffFromAniList(root, ref nativeStaff, ref fullStaff);

        // Assert: The exact expected string values (pipe-separated and trimmed) and no illutrators
        const string expectedFullStaff = "Ito Oogure | NISIOISIN | VOFAN | Akio Watanabe";
        const string expectedNativeStaff = "大暮維人 | 西尾維新 | 戴源亨 | 渡辺明夫";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fullStaff, Is.EqualTo(expectedFullStaff), "Full staff string does not match expected.");
            Assert.That(nativeStaff, Is.EqualTo(expectedNativeStaff), "Native staff string does not match expected.");
        }
    }

    [Test]
    public async Task ExtractStaffFromAniList_ShouldTrimRolesAndIgnoreInvalid_OnUntrimmedStaffRoleJson()
    {
        string json = await File.ReadAllTextAsync(@"AniList\AniListTestData\UntrimmedStaffRole.json");
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string nativeStaff = string.Empty;
        string fullStaff = string.Empty;

        Clients.AniList.ExtractStaffFromAniList(root, ref nativeStaff, ref fullStaff);

        // Assert: Should only include valid roles ("Story", "Art") after trimming
        const string expectedFull = "Kafka Asagiri | Harukawa35";
        const string expectedNative = "朝霧カフカ | 春河35";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fullStaff, Is.EqualTo(expectedFull), "Full staff should include only valid, trimmed roles.");
            Assert.That(nativeStaff, Is.EqualTo(expectedNative), "Native staff should include only valid, trimmed roles.");
        }
    }

    [Test]
    public async Task ExtractStaffFromAniList_ShouldHandleSimilarRolesCorrectly_WhenRolesAreVariant()
    {
        string json = await File.ReadAllTextAsync(@"AniList\AniListTestData\SimilarStaffRoles.json");
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string nativeStaff = string.Empty;
        string fullStaff = string.Empty;

        Clients.AniList.ExtractStaffFromAniList(root, ref nativeStaff, ref fullStaff);

        // Assert: Should include only valid staff with normalized roles (Story, Original Story, Art)
        const string expectedFull = "Chu-Gong | So-Ryeong Gi | Hyeon-Gun | Seong-Rak Jang";
        const string expectedNative = "추공 | 기소령 | 현군 | 장성락";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fullStaff, Is.EqualTo(expectedFull), "Full staff string should include valid, normalized roles.");
            Assert.That(nativeStaff, Is.EqualTo(expectedNative), "Native staff string should match valid contributors.");
        }
    }

    [Test]
    public async Task ExtractStaffFromAniList_ShouldUseFallbacks_WhenStaffNamesAreMissing()
    {
        string json = await File.ReadAllTextAsync(@"AniList\AniListTestData\NullStaffNames.json");
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string nativeStaff = string.Empty;
        string fullStaff = string.Empty;

        Clients.AniList.ExtractStaffFromAniList(root, ref nativeStaff, ref fullStaff);

        const string expectedFull = "FullNameOnly | NativeOnly | NativeWithAlt | AltOnly1 | FullWithAlt";
        const string expectedNative = "FullNameOnly | NativeOnly | NativeWithAlt | AltOnly1 | FullWithAlt";

        using (Assert.EnterMultipleScope())
        {
            Assert.That(fullStaff, Is.EqualTo(expectedFull), "Full staff fallback logic failed.");
            Assert.That(nativeStaff, Is.EqualTo(expectedNative), "Native staff fallback logic failed.");
        }
    }

}