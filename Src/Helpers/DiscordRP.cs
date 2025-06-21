using DiscordRPC;
using DiscordRPC.Logging;

namespace Tsundoku.Helpers;

// TODO - Now I can set the presence when people are looking at various windows, also possibly display the cover image of the series they are editing?
internal static class DiscordRP
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static DiscordRpcClient? client;
    private static string UserName;
    private const string APP_ID = "1050229234674696252";
    private static PresenceState _presence = new();

    public static void Initialize()
    {
        if (client != null && client.IsInitialized)
            return;

        client = new DiscordRpcClient(APP_ID)
        {
            Logger = new ConsoleLogger(DiscordRPC.Logging.LogLevel.Error, true)
        };

        client.OnReady += (_, msg) =>
        {
            UserName = msg.User.Username;
            LOGGER.Info("Connected to Discord with user: {0}", UserName);
        };

        client.Initialize();

        SetPresence("Manga & Light Novel Collection App", "Browsing Collection");
    }

    public static void Deinitialize()
    {
        if (client != null && !client.IsDisposed)
        {
            LOGGER.Info("Disconnected from Discord with user: {0}", UserName);
            client.Dispose();
            client = null;
        }
    }

    public static void SetPresence(string? details = null, string? state = null, bool refreshTimestamp = false)
    {
        if (client == null || !client.IsInitialized)
        {
            return;
        }

        if (details != null)
        {
            _presence.Details = details;
        }
        if (state != null)
        {
            _presence.State = state;
        }
        if (refreshTimestamp || _presence.Timestamps == null)
        {
            _presence.Timestamps = Timestamps.Now;
        }

        client.SetPresence(new RichPresence
        {
            Details = _presence.Details,
            State = _presence.State,
            Timestamps = _presence.Timestamps,
            Buttons = [
                new Button
                {
                    Label = "Download Tsundoku",
                    Url = "https://apps.microsoft.com/detail/9P85XXDQFHS2?hl=en-us&gl=US&ocid=pdpshare"
                }
            ],
            Assets = new Assets
            {
                LargeImageKey = "rp_large_icon",
                LargeImageText = "Tsundoku"
            }
        });
    }

    public static void ClearPresence()
    {
        _presence = new PresenceState();
        client?.ClearPresence();
    }

    private sealed class PresenceState
    {
        public string? Details { get; set; }
        public string? State { get; set; }
        public Timestamps? Timestamps { get; set; }
    }
}