using DiscordRPC;
using DiscordRPC.Logging;

namespace Tsundoku.Helpers
{
    internal class DiscordRP
    {
        private static DiscordRpcClient client;
        private static string UserName;

        public static void Initialize()
        {
            client = new DiscordRpcClient("1050229234674696252")
            {
                Logger = new ConsoleLogger(LogLevel.Error, true)
            };

            client.OnReady += (sender, msg) =>
            {
                UserName = msg.User.Username;
                LOGGER.Info("Connected To Discord With User {}", UserName);
            };

            client.Initialize();

            client.SetPresence(new RichPresence()
            {
                Details = "Manga & Light Novel Collection App",
                State = "Browsing Collection",
                Timestamps = Timestamps.Now,
                Buttons =
                [
                    new Button() 
                    { 
                        Label = "Download Tsundoku", 
                        Url = "https://github.com/Sigrec/Tsundoku/blob/main/README.md"
                    }
                ],
                Assets = new Assets()
                {
                    LargeImageKey = "rp_large_icon",
                    LargeImageText = "Tsundoku",
                }
            });
        }

        public static void Deinitialize() 
        {
            LOGGER.Info("Disconnected From Discord With User {}", UserName);
            client.Dispose();
        }
    }
}
