using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Tsundoku.Tests.MangaDex;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class MangaDexClientTests
{
    private IServiceProvider _provider = null!;
    private Clients.MangaDex _mangaDex;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        ServiceCollection services = new ServiceCollection();

        services.AddHttpClient("MangaDexClient", client =>
        {
            client.BaseAddress = new Uri("https://api.mangadex.org/");
            client.DefaultRequestVersion = HttpVersion.Version30;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Tsundoku-Test", "1.0"));
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("(https://github.com/Sigrec/Tsundoku)"));
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        _provider = services.BuildServiceProvider();
        IHttpClientFactory factory = _provider.GetRequiredService<IHttpClientFactory>();
        _mangaDex = new Clients.MangaDex(factory);
    }

    [OneTimeTearDown]
    public void OneTimeTeardown()
    {
        if (_provider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [TestCase("dont toy with me miss nagatoro")]
    [TestCase("The Beginning After the End")]
    [TestCase("Radiant")]
    public async Task GetSeriesByTitleAsync_ContainsExpectedFields(string title)
    {
        JsonDocument? result = await _mangaDex.GetSeriesByTitleAsync(title);
        Assert.That(result, Is.Not.Null);

        JsonElement root = result!.RootElement;
        Assert.That(root.TryGetProperty("data", out JsonElement dataElem));

        JsonElement first = dataElem.ValueKind == JsonValueKind.Array
            ? dataElem.EnumerateArray().FirstOrDefault()
            : dataElem;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined));
            Assert.That(first.TryGetProperty("id", out _));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first.TryGetProperty("attributes", out JsonElement attrElem));
            Assert.That(attrElem.TryGetProperty("title", out JsonElement titleElem));
            Assert.That(titleElem.TryGetProperty("en", out _), "Title should contain 'en' sub-property.");
            Assert.That(attrElem.TryGetProperty("altTitles", out _));
            Assert.That(attrElem.TryGetProperty("publicationDemographic", out _));
            Assert.That(attrElem.TryGetProperty("status", out _));
            Assert.That(attrElem.TryGetProperty("description", out _));
            Assert.That(attrElem.TryGetProperty("links", out _));
            Assert.That(attrElem.TryGetProperty("originalLanguage", out _));
            Assert.That(attrElem.TryGetProperty("tags", out _));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first.TryGetProperty("relationships", out JsonElement relationships));
            Assert.That(relationships.ValueKind, Is.EqualTo(JsonValueKind.Array), "Relationships should be an array.");
            Assert.That(relationships.EnumerateArray().Any(e => e.TryGetProperty("type", out JsonElement typeProp) && typeProp.GetString() == "cover_art"), "Expected at least one relationship with type 'cover_art'.");
        }
    }

    [TestCase("38204a99-8c9f-43f4-ac29-1e3896841946")] // https://mangadex.org/title/38204a99-8c9f-43f4-ac29-1e3896841946/my-rei-of-light
    public async Task GetSeriesByIdAsync_NoEnglishDesc_DefaultToOriginalLang(string id)
    {
        JsonDocument? result = await _mangaDex.GetSeriesByIdAsync(id);
        Assert.That(result, Is.Not.Null);

        JsonElement root = result!.RootElement;
        Assert.That(root.TryGetProperty("data", out JsonElement dataElem));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataElem.TryGetProperty("id", out _));
            Assert.That(dataElem.TryGetProperty("attributes", out JsonElement attrElem));
            Assert.That(attrElem.TryGetProperty("description", out JsonElement descElem));
            Assert.That(attrElem.TryGetProperty("originalLanguage", out JsonElement origLangElem), Is.True, "expected an originalLanguage property");

            string lang = origLangElem.GetString()!;
            Assert.That(lang, Is.EqualTo("vi"));

            Assert.That(descElem.TryGetProperty(lang, out JsonElement localizedDescElem), Is.True, $"expected a description for language '{lang}'");
            string expectedDesc = "“Tôi muốn cậu trở thành nguồn cảm hứng của tôi!”\n\nHai con người với những đam mê cháy bỏng nhưng lạc lối trong chính câu chuyện của riêng mình. Một cách tình cờ, họ đã gặp được nhau.\n\nỞ cạnh nhau, Yuji và Masa, hai con người yêu cái đẹp, được sống là chính mình. Cùng niềm đam mê quá đỗi đặc biệt họ dần khám phá ra những cảm xúc chôn giấu và điều sẽ thay đổi cuộc đời của họ mãi mãi.";
            Assert.That(localizedDescElem.GetString(), Is.EqualTo(expectedDesc));
        }
    }

    [TestCase("4413d794-e0da-4a6e-b61a-afd5758914e6")] // https://mangadex.org/title/4413d794-e0da-4a6e-b61a-afd5758914e6
    [TestCase("c0189f4a-dee6-48f4-abbe-53a4359cbcfb")] // https://mangadex.org/title/c0189f4a-dee6-48f4-abbe-53a4359cbcfb
    [TestCase("b6641b6c-eb13-448b-b296-1be4912b4cc8")] // https://mangadex.org/title/b6641b6c-eb13-448b-b296-1be4912b4cc8
    public async Task GetSeriesByIdAsync_ContainsExpectedFields(string id)
    {
        JsonDocument? result = await _mangaDex.GetSeriesByIdAsync(id);
        Assert.That(result, Is.Not.Null);

        JsonElement root = result!.RootElement;
        Assert.That(root.TryGetProperty("data", out JsonElement dataElem));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataElem.TryGetProperty("id", out _));
            Assert.That(dataElem.TryGetProperty("attributes", out JsonElement attrElem));
            Assert.That(attrElem.TryGetProperty("title", out JsonElement titleElem));
            Assert.That(titleElem.TryGetProperty("en", out _), "Title should contain 'en' sub-property.");
            Assert.That(attrElem.TryGetProperty("altTitles", out _));
            Assert.That(attrElem.TryGetProperty("publicationDemographic", out _));
            Assert.That(attrElem.TryGetProperty("status", out _));
            Assert.That(attrElem.TryGetProperty("description", out _));
            Assert.That(attrElem.TryGetProperty("links", out _));
            Assert.That(attrElem.TryGetProperty("originalLanguage", out _));
            Assert.That(attrElem.TryGetProperty("tags", out _));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dataElem.TryGetProperty("relationships", out JsonElement relationships));
            Assert.That(relationships.ValueKind, Is.EqualTo(JsonValueKind.Array), "Relationships should be an array.");
            Assert.That(relationships.EnumerateArray().Any(e => e.TryGetProperty("type", out JsonElement typeProp) && typeProp.GetString() == "cover_art"), "Expected at least one relationship with type 'cover_art'.");
        }
    }

    [TestCase("4413d794-e0da-4a6e-b61a-afd5758914e6", "Hanninmae no Koibito", "Kawada Daichi", "川田大智")]
    [TestCase("32d76d19-8a05-4db0-9fc2-e0b0648fe9d0", "Solo Leveling", "h-goon | Chugong | Gi So-Ryeong | REDICE Studio | Jang Sung-Rak", "현군 | 추공 | 기소령 | 레드아이스 스튜디오 | 장성락")]
    [TestCase("553d7ab9-5657-418f-9e4b-070d5b9141eb", "Cyfandir Chronicles", "Tony Valente | Naokuren", "Tony Valente | Naokuren")]
    [TestCase("f7b62193-bdfb-4953-a6c6-0bd1b9a872f9", "The Bugle Call: Song of War", "Sora Mozuku | Toumori Higoro", "空もずく | 十森ひごろ")]
    public async Task GetStaffAsync_ParsesExpectedStaffData_ById(string seriesId, string title, string expectedFull, string expectedNative)
    {
        JsonDocument? doc = await _mangaDex.GetSeriesByIdAsync(seriesId);
        Assert.That(doc, Is.Not.Null);

        JsonElement data = doc!.RootElement.GetProperty("data");
        JsonElement relationships = data.GetProperty("relationships");
        JsonElement[] relationshipsArray = relationships.EnumerateArray().ToArray();

        (StringBuilder fullStaff, StringBuilder nativeStaff) = await _mangaDex.GetStaffAsync(relationshipsArray, title);

        Assert.That(fullStaff.ToString(), Is.EqualTo(expectedFull), "Unexpected full staff string.");
        Assert.That(nativeStaff.ToString(), Is.EqualTo(expectedNative), "Unexpected native staff string.");
    }

    [TestCase("e52a6df5-0c82-4168-9477-8795696f31af", "author & artist", "Tony Valente", null)]
    [TestCase("3ffb7a8b-3b85-430a-ae7e-3c27e91861fd", "author & artist", "Kawada Daichi", "Native name: 川田大智")] // author & artist
    public async Task GetRelationshipAsync_ReturnsCorrectInfo(string staffId, string staffType, string expectedFullName, string? expectedBio)
    {
        (string name, string? bio) = await _mangaDex.GetRelationshipAsync(staffId, staffType);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(name, Is.EqualTo(expectedFullName));
            Assert.That(bio, Is.EqualTo(expectedBio));
        }
    }
}
