namespace Tsundoku.Tests.Helpers;

[TestFixture]
public class AppFileHelperTests
{
    #region VALID_IMAGE_EXTENSIONS

    [Test]
    public void ValidImageExtensions_ContainsPng()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".png"), Is.True);
    }

    [Test]
    public void ValidImageExtensions_ContainsJpg()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".jpg"), Is.True);
    }

    [Test]
    public void ValidImageExtensions_ContainsJpeg()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".jpeg"), Is.True);
    }

    [Test]
    public void ValidImageExtensions_DoesNotContainGif()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".gif"), Is.False);
    }

    [Test]
    public void ValidImageExtensions_DoesNotContainBmp()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".bmp"), Is.False);
    }

    [Test]
    public void ValidImageExtensions_DoesNotContainWebp()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".webp"), Is.False);
    }

    [Test]
    public void ValidImageExtensions_CaseInsensitiveLookup_UpperCasePng()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".PNG"), Is.True);
    }

    [Test]
    public void ValidImageExtensions_CaseInsensitiveLookup_MixedCaseJpeg()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(".JpEg"), Is.True);
    }

    [Test]
    public void ValidImageExtensions_EmptyString_ReturnsFalse()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS.Contains(string.Empty), Is.False);
    }

    [Test]
    public void ValidImageExtensions_HasExactlyThreeEntries()
    {
        Assert.That(AppFileHelper.VALID_IMAGE_EXTENSIONS, Has.Count.EqualTo(3));
    }

    #endregion
}
