namespace Tsundoku.Models;

[JsonSerializable(typeof(Series))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
internal partial class SeriesModelContext : JsonSerializerContext
{
}