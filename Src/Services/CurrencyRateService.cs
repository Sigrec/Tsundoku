using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Net.Http;
using System.Text.Json;

namespace Tsundoku.Services;

public interface ICurrencyRateService
{
    /// <summary>UTC timestamp of the most recent successful refresh, or null if only defaults are loaded.</summary>
    DateTime? LastRefreshedUtc { get; }

    /// <summary>Whether an ISO code is known to this service (both by mapping and by rate lookup).</summary>
    bool SupportsSymbol(string symbol);

    /// <summary>
    /// Converts an amount from one currency (identified by its user-facing symbol,
    /// e.g. "$" or "€") to another. Returns null if either symbol is unmapped or
    /// its rate is missing.
    /// </summary>
    decimal? Convert(decimal amount, string fromSymbol, string toSymbol);

    /// <summary>Fetches current rates from the upstream FX API and swaps them in atomically.</summary>
    Task<bool> RefreshAsync(CancellationToken cancellationToken = default);
}

public sealed class CurrencyRateService : ICurrencyRateService
{
    private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

    private readonly IHttpClientFactory _httpClientFactory;

    // Map from the user-facing symbol (whatever `User.Currency` stores) to the ISO-4217 code
    // used by the rate provider. Keep in lockstep with CurrencyModel.Currency enum.
    private static readonly FrozenDictionary<string, string> SymbolToIso =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["$"]     = "USD",
            ["€"]     = "EUR",
            ["£"]     = "GBP",
            ["¥"]     = "JPY",
            ["₹"]     = "INR",
            ["₱"]     = "PHP",
            ["₩"]     = "KRW",
            ["₽"]     = "RUB",
            ["₺"]     = "TRY",
            ["₫"]     = "VND",
            ["฿"]     = "THB",
            ["₸"]     = "KZT",
            ["₼"]     = "AZN",
            ["₾"]     = "GEL",
            ["Rp"]    = "IDR",
            ["RM"]    = "MYR",
            ["R$"]    = "BRL",
            ["₪"]     = "ILS",
            ["₴"]     = "UAH",
            ["zł"]    = "PLN",
            ["Ft"]    = "HUF",
            ["Kč"]    = "CZK",
            ["kr"]    = "SEK",
            ["lei"]   = "RON",
            ["৳"]     = "BDT",
            ["₮"]     = "MNT",
            ["KM"]    = "BAM",
            ["Br"]    = "BYN",
            ["L"]     = "ALL",
            ["din"]   = "RSD",
            ["ден"]   = "MKD",
            ["ر.س"]   = "SAR",
            ["د.إ"]   = "AED",
            ["د.ك"]   = "KWD",
            ["Rs"]    = "LKR",
        }.ToFrozenDictionary(StringComparer.Ordinal);

    // Fallback rates (base = USD, i.e. 1 USD → X of that currency). Snapshot as of late 2025.
    // Used until RefreshAsync succeeds. Values are approximate.
    private static readonly IReadOnlyDictionary<string, decimal> DefaultUsdRates =
        new Dictionary<string, decimal>(StringComparer.Ordinal)
        {
            ["USD"] = 1m,
            ["EUR"] = 0.92m,
            ["GBP"] = 0.79m,
            ["JPY"] = 149m,
            ["INR"] = 83m,
            ["PHP"] = 56.5m,
            ["KRW"] = 1340m,
            ["RUB"] = 92m,
            ["TRY"] = 32m,
            ["VND"] = 24500m,
            ["THB"] = 35m,
            ["KZT"] = 470m,
            ["AZN"] = 1.7m,
            ["GEL"] = 2.7m,
            ["IDR"] = 15600m,
            ["MYR"] = 4.7m,
            ["BRL"] = 5m,
            ["ILS"] = 3.7m,
            ["UAH"] = 39m,
            ["PLN"] = 4m,
            ["HUF"] = 360m,
            ["CZK"] = 23m,
            ["SEK"] = 10.5m,
            ["RON"] = 4.6m,
            ["BDT"] = 110m,
            ["MNT"] = 3400m,
            ["BAM"] = 1.8m,
            ["BYN"] = 3.3m,
            ["ALL"] = 96m,
            ["RSD"] = 108m,
            ["MKD"] = 57m,
            ["SAR"] = 3.75m,
            ["AED"] = 3.67m,
            ["KWD"] = 0.31m,
            ["LKR"] = 320m,
        };

    private readonly ConcurrentDictionary<string, decimal> _usdRates;
    public DateTime? LastRefreshedUtc { get; private set; }

    public CurrencyRateService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _usdRates = new ConcurrentDictionary<string, decimal>(DefaultUsdRates, StringComparer.Ordinal);
    }

    public bool SupportsSymbol(string symbol) =>
        !string.IsNullOrEmpty(symbol) &&
        SymbolToIso.TryGetValue(symbol, out string? iso) &&
        _usdRates.ContainsKey(iso);

    public decimal? Convert(decimal amount, string fromSymbol, string toSymbol)
    {
        if (string.Equals(fromSymbol, toSymbol, StringComparison.Ordinal)) return amount;
        if (!SymbolToIso.TryGetValue(fromSymbol, out string? fromIso)) return null;
        if (!SymbolToIso.TryGetValue(toSymbol, out string? toIso)) return null;
        if (!_usdRates.TryGetValue(fromIso, out decimal fromRate) || fromRate <= 0) return null;
        if (!_usdRates.TryGetValue(toIso, out decimal toRate) || toRate <= 0) return null;

        // Pivot through USD.
        decimal usd = amount / fromRate;
        return usd * toRate;
    }

    public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient(nameof(CurrencyRateService));
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Tsundoku/1.0");

            using HttpResponseMessage response = await client.GetAsync(
                "https://open.er-api.com/v6/latest/USD",
                cancellationToken);
            response.EnsureSuccessStatusCode();

            using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (!doc.RootElement.TryGetProperty("result", out JsonElement resultEl) ||
                resultEl.GetString() != "success" ||
                !doc.RootElement.TryGetProperty("rates", out JsonElement ratesEl) ||
                ratesEl.ValueKind != JsonValueKind.Object)
            {
                LOGGER.Warn("Currency refresh response missing 'rates' or not successful");
                return false;
            }

            int updated = 0;
            foreach (JsonProperty prop in ratesEl.EnumerateObject())
            {
                if (prop.Value.TryGetDecimal(out decimal rate) && rate > 0)
                {
                    _usdRates[prop.Name] = rate;
                    updated++;
                }
            }

            LastRefreshedUtc = DateTime.UtcNow;
            LOGGER.Info("Currency rates refreshed: {Count} rates from open.er-api.com", updated);
            return true;
        }
        catch (OperationCanceledException)
        {
            LOGGER.Info("Currency refresh cancelled");
            return false;
        }
        catch (Exception ex)
        {
            LOGGER.Warn(ex, "Currency refresh failed — falling back to prior rates");
            return false;
        }
    }
}
