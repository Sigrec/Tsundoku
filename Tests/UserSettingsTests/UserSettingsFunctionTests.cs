using Tsundoku.Views;

namespace Tests.UserSettingsTests
{
    [Author("Sean (Alias -> Prem or Sigrec)")]
    [TestOf(typeof(SettingsWindow))]
    [Description("Testing User Settings Window Button Functions")]
    public class UserSettingsFunctionTests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void ExportToSpreadsheetCSV_Test()
        {
            Assert.Pass();
        }
    }
}