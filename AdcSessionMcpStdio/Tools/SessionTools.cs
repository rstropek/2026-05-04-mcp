using System.ComponentModel;
using AdcSessionMcpStdio.Models;
using AdcSessionMcpStdio.Services;
using ModelContextProtocol.Server;

namespace AdcSessionMcpStdio.Tools;

[McpServerToolType]
public sealed class SessionTools
{
    private readonly SessionQueryService sessionQueryService;

    public SessionTools(SessionQueryService sessionQueryService)
    {
        this.sessionQueryService = sessionQueryService;
    }

    [McpServerTool(
        Name = "adc_get_distinct_themes",
        UseStructuredContent = true,
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false),
     Description("Gets all distinct ADC session themes in ascending order with case-insensitive deduplication. This is a read-only tool.")]
    public Task<IReadOnlyList<string>> GetDistinctThemesAsync()
        => sessionQueryService.GetDistinctThemesAsync();

    [McpServerTool(
        Name = "adc_get_sessions",
        UseStructuredContent = true,
        ReadOnly = true,
        Idempotent = true,
        Destructive = false,
        OpenWorld = false),
     Description("Gets ADC sessions with optional case-insensitive filters, optional ascending sorts, and fixed paging of five sessions per page. This is a read-only tool.")]
    public Task<SessionQueryResult> GetSessionsAsync(
        [Description("Optional theme filter. Matches sessions where any theme contains this text, ignoring case.")] string? theme = null,
        [Description("Optional speaker filter. Matches sessions where the speaker field contains this text, ignoring case.")] string? speaker = null,
        [Description("Optional title filter. Matches sessions where the title contains this text, ignoring case.")] string? title = null,
        [Description("Optional ascending sort keys in priority order. Allowed values are start and title. Accepted aliases for start are startdatetime, start_date_time, and date. ID ascending is always applied after these sorts. Blank values are rejected.")] string[]? sortBy = null,
        [Description("One-based page number. Each page contains five sessions. Values below 1 are rejected. Pages beyond the last page return an empty session list.")] int page = 1)
        => sessionQueryService.GetSessionsAsync(theme, speaker, title, ParseSortKeys(sortBy), page);

    internal static IReadOnlyList<SessionSortKey>? ParseSortKeys(string[]? sortBy)
    {
        if (sortBy is not { Length: > 0 })
        {
            return null;
        }

        var sortKeys = new List<SessionSortKey>(sortBy.Length);

        foreach (var rawSortKey in sortBy)
        {
            if (string.IsNullOrWhiteSpace(rawSortKey))
            {
                throw new ArgumentException("Sort keys cannot be null, empty, or whitespace.", nameof(sortBy));
            }

            var sortKey = rawSortKey.Trim().ToLowerInvariant() switch
            {
                "start" or "startdatetime" or "start_date_time" or "date" => SessionSortKey.Start,
                "title" => SessionSortKey.Title,
                _ => throw new ArgumentException($"Unsupported sort key '{rawSortKey}'. Allowed values are 'start' and 'title'.", nameof(sortBy))
            };

            sortKeys.Add(sortKey);
        }

        return sortKeys;
    }
}
