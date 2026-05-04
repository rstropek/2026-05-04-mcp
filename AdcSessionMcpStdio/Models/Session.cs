using System.Text.Json.Serialization;

namespace AdcSessionMcpStdio.Models;

public sealed record Session(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("date")] string Date,
    [property: JsonPropertyName("start")] string Start,
    [property: JsonPropertyName("end")] string End,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("level_label")] string LevelLabel,
    [property: JsonPropertyName("languages_raw")] string LanguagesRaw,
    [property: JsonPropertyName("themes")] IReadOnlyList<string> Themes,
    [property: JsonPropertyName("speaker_image")] string SpeakerImage,
    [property: JsonPropertyName("speaker")] string Speaker,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("duration")] string Duration,
    [property: JsonPropertyName("language")] string Language,
    [property: JsonPropertyName("description")] string Description);
