using DiscordRPC;
using DiscordRPC.Logging;

namespace Tsundoku.Services;

public static class DiscordRP
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static DiscordRpcClient? client;
    private static string UserName;
    private static PresenceState _presence = new();

    public static void Initialize()
    {
        if (client is not null && client.IsInitialized)
        {
            return;
        }

        try
        {
            client = new DiscordRpcClient("1050229234674696252")
            {
#if DEBUG
                Logger = new ConsoleLogger(DiscordRPC.Logging.LogLevel.Warning)
#else
                Logger = null  // Disable logging in Release builds
#endif
            };

            client.OnError += (_, e) => LOGGER.Error("DiscordRPC error {0}: {1}", e.Code, e.Message);
            client.OnConnectionFailed += (_, e) => LOGGER.Error("DiscordRPC connection failed: {0}", e);

            client.OnReady += (_, msg) =>
            {
                try
                {
                    UserName = msg.User.Username;
                    LOGGER.Info("Connected to Discord with user: {0}", UserName);
                }
                catch (Exception ex)
                {
                    LOGGER.Error(ex, "Error in OnReady handler");
                }
            };

            client.Initialize();
            SetPresence();
        }
        catch (Exception ex)
        {
            LOGGER.Error(ex, "Failed to initialize Discord Rich Presence. Discord may not be running.");
            client?.Dispose();
            client = null;
        }
    }

    public static void Deinitialize()
    {
        if (client is not null && !client.IsDisposed)
        {
            LOGGER.Info("Disconnected from Discord with user: {0}", UserName);
            client.Dispose();
            client = null;
        }
    }

    public static void SetPresence(
        string? details = null,
        string? state = null,
        bool refreshTimestamp = true,
        Button? additionalButton = null)
    {
        if (client is null || !client.IsInitialized)
        {
            return;
        }

        if (details is not null)
        {
            _presence.Details = details;
        }

        if (state is not null)
        {
            _presence.State = state;
        }

        if (refreshTimestamp || _presence.Timestamps is null)
        {
            _presence.Timestamps = Timestamps.Now;
        }

        _presence.Buttons.RemoveRange(1, _presence.Buttons.Count - 1);
        if (additionalButton is not null)
        {
            LOGGER.Debug("Adding additional button");
            _presence.Buttons.Add(additionalButton);
        }

        ResetPresence();
    }

    private static void ResetPresence()
    {
        client.SetPresence(new RichPresence
        {
            Details = _presence.Details,
            State = _presence.State,
            Timestamps = _presence.Timestamps,
            Buttons = [.. _presence.Buttons],
            Assets = new Assets
            {
                LargeImageKey = "rp_large_icon",
                LargeImageText = "Tsundoku"
            }
        });
    }

    public static void ClearPresence()
    {
        if (client is null || !client.IsInitialized)
        {
            return;
        }
        _presence = new PresenceState();
        ResetPresence();
    }

    private sealed record PresenceState
    {
        public string? Details { get; set; } = "Manga & Light Novel Collection Tracking App";
        public string? State { get; set; } = "Browsing Collection";
        public Timestamps? Timestamps { get; set; }
        public List<Button> Buttons { get; set; } = [
            new Button
            {
                Label = "Download Tsundoku",
                Url = "https://apps.microsoft.com/detail/9P85XXDQFHS2?hl=en-us&gl=US&ocid=pdpshare"
            }
        ];
    }
}