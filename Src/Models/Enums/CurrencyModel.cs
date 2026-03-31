using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;

namespace Tsundoku.Models.Enums;

/// <summary>
/// Specifies the symbol and culture code for a currency enum value.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class CurrencyInfoAttribute(string symbol, string culture) : Attribute
{
    public string Symbol { get; } = symbol;
    public string Culture { get; } = culture;
}

public static class CurrencyModel
{
    public static readonly ImmutableArray<string> AVAILABLE_CURRENCIES;
    public static readonly FrozenDictionary<string, (int Index, string Culture)> AVAILABLE_CURRENCY_WITH_CULTURE;

    static CurrencyModel()
    {
        List<string> symbols = [];
        Dictionary<string, (int Index, string Culture)> currencyMap = new(StringComparer.Ordinal);

        int index = 0;
        foreach (Currency currency in Enum.GetValues<Currency>())
        {
            string name = Enum.GetName(currency)!;
            FieldInfo? field = typeof(Currency).GetField(name);
            CurrencyInfoAttribute? info = field?.GetCustomAttribute<CurrencyInfoAttribute>();
            if (info is null) continue;

            symbols.Add(info.Symbol);
            currencyMap[info.Symbol] = (index, info.Culture);
            index++;
        }

        AVAILABLE_CURRENCIES = [.. symbols];
        AVAILABLE_CURRENCY_WITH_CULTURE = currencyMap.ToFrozenDictionary(StringComparer.Ordinal);
    }

    public enum Currency
    {
        [CurrencyInfo("$", "en-US")] USDollar,
        [CurrencyInfo("€", "de-DE")] Euro,
        [CurrencyInfo("£", "en-GB")] BritishPound,
        [CurrencyInfo("¥", "ja-JP")] Yen,
        [CurrencyInfo("₹", "hi-IN")] IndianRupee,
        [CurrencyInfo("₱", "fil-PH")] PhilippinePeso,
        [CurrencyInfo("₩", "ko-KR")] KoreanWon,
        [CurrencyInfo("₽", "ru-RU")] RussianRuble,
        [CurrencyInfo("₺", "tr-TR")] TurkishLira,
        [CurrencyInfo("₫", "vi-VN")] VietnameseDong,
        [CurrencyInfo("฿", "th-TH")] ThaiBaht,
        [CurrencyInfo("₸", "kk-KZ")] KazakhstaniTenge,
        [CurrencyInfo("₼", "az-Latn-AZ")] AzerbaijaniManat,
        [CurrencyInfo("₾", "ka-GE")] GeorgianLari,
        [CurrencyInfo("Rp", "id-ID")] IndonesianRupiah,
        [CurrencyInfo("RM", "ms-MY")] MalaysianRinggit,
        [CurrencyInfo("R$", "pt-BR")] BrazilianReal,
        [CurrencyInfo("₪", "he-IL")] IsraeliShekel,
        [CurrencyInfo("₴", "uk-UA")] UkrainianHryvnia,
        [CurrencyInfo("zł", "pl-PL")] PolishZloty,
        [CurrencyInfo("Ft", "hu-HU")] HungarianForint,
        [CurrencyInfo("Kč", "cs-CZ")] CzechKoruna,
        [CurrencyInfo("kr", "sv-SE")] SwedishKrona,
        [CurrencyInfo("lei", "ro-RO")] RomanianLeu,
        [CurrencyInfo("৳", "bn-BD")] BangladeshiTaka,
        [CurrencyInfo("₮", "mn-MN")] MongolianTugrik,
        [CurrencyInfo("KM", "bs-Latn-BA")] BosnianMark,
        [CurrencyInfo("Br", "be-BY")] BelarusianRuble,
        [CurrencyInfo("L", "sq-AL")] AlbanianLek,
        [CurrencyInfo("din", "sr-RS")] SerbianDinar,
        [CurrencyInfo("ден", "mk-MK")] MacedonianDenar,
        [CurrencyInfo("ر.س", "ar-SA")] SaudiRiyal,
        [CurrencyInfo("د.إ", "ar-AE")] UAEDirham,
        [CurrencyInfo("د.ك", "ar-KW")] KuwaitiDinar,
        [CurrencyInfo("Rs", "ta-IN")] SriLankanRupee,
    }
}
