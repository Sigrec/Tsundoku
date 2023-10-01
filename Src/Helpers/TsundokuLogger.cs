using NLog;
using NLog.Config;

namespace Src.Helpers
{
    public class TsundokuLogger
    {
        public static readonly Logger LOGGER;
        static TsundokuLogger()
		{
			var stream = typeof(Tsundoku.Program).Assembly.GetManifestResourceStream("Tsundoku.NLog.config");
			string xml;
			using (var reader = new StreamReader(stream))
			{
				xml = reader.ReadToEnd();
			}

			LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(xml);
            LOGGER = LogManager.GetLogger("TsundokuLogs");
		}
    }
}