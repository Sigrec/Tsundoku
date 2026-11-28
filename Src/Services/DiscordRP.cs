using DiscordRPC;

namespace Tsundoku.Services;

/// <summary>
/// Custom Discord RPC logger that suppresses a known harmless library exception.
/// </summary>
/// <remarks>
/// DiscordRPC throws a NullReferenceException in <c>Assets.Merge</c> when Discord echoes our presence
/// back; it only affects the library's cached copy of the presence, which Tsundoku never reads. The
/// library reports one exception as three separate Error calls (a header naming the exception type,
/// the exception message, then the stack trace), so the header is held until the stack trace shows
/// whether it is that known case. Anything else is logged in full.
/// </remarks>
internal sealed class FilteredDiscordLogger : DiscordRPC.Logging.ILogger
{
    private const string ExceptionHeaderPrefix = "Unhandled Exception while processing event:";
    private const string KnownFailingFrame = "Assets.Merge";

    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private readonly Lock _gate = new();
    private readonly List<string> _pending = [];

    public DiscordRPC.Logging.LogLevel Level { get; set; } = DiscordRPC.Logging.LogLevel.Error;

    public void Trace(string message, params object[] args) { }
    public void Info(string message, params object[] args) { }
    public void Warning(string message, params object[] args) { }

    public void Error(string message, params object[] args)
    {
        string formatted = args.Length > 0 ? string.Format(message, args) : message;
        lock (_gate)
        {
            if (formatted.StartsWith(ExceptionHeaderPrefix, StringComparison.Ordinal))
            {
                FlushPending();
                _pending.Add(formatted);
                return;
            }

            if (_pending.Count == 0)
            {
                LOGGER.Warn("DiscordRPC: {Message}", formatted);
                return;
            }

            _pending.Add(formatted);
            if (!formatted.TrimStart().StartsWith("at ", StringComparison.Ordinal))
            {
                return;
            }

            if (formatted.Contains(KnownFailingFrame, StringComparison.Ordinal))
            {
                _pending.Clear();
            }
            else
            {
                FlushPending();
            }
        }
    }

    private void FlushPending()
    {
        if (_pending.Count == 0) return;
        LOGGER.Warn("DiscordRPC: {Message}", string.Join(Environment.NewLine, _pending));
        _pending.Clear();
    }
}

public static class DiscordRP
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
    private static readonly Assets _staticAssets = new()
    {
        LargeImageKey = "rp_large_icon",
        LargeImageText = "Tsundoku"
    };
    private static DiscordRpcClient? client;
    private static string UserName = string.Empty;
    private static PresenceState _presence = new();
    private static Button[]? _cachedButtons;
    private static bool _buttonsDirty = true;

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
                Logger = new FilteredDiscordLogger()
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

        if (_presence.Buttons.Count > 1)
        {
            _presence.Buttons.RemoveRange(1, _presence.Buttons.Count - 1);
            _buttonsDirty = true;
        }
        if (additionalButton is not null)
        {
            LOGGER.Debug("Adding additional button");
            _presence.Buttons.Add(additionalButton);
            _buttonsDirty = true;
        }

        ResetPresence();
    }

    private static void ResetPresence()
    {
        if (client is null || !client.IsInitialized || client.IsDisposed)
        {
            return;
        }

        if (_buttonsDirty || _cachedButtons is null)
        {
            _cachedButtons = [.. _presence.Buttons];
            _buttonsDirty = false;
        }

        client.SetPresence(new RichPresence
        {
            Details = _presence.Details,
            State = _presence.State,
            Timestamps = _presence.Timestamps,
            Buttons = _cachedButtons,
            Assets = _staticAssets
        });
    }

    public static void ClearPresence()
    {
        if (client is null || !client.IsInitialized)
        {
            return;
        }
        _presence = new PresenceState();
        _buttonsDirty = true;
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