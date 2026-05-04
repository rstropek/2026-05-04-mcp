using System.Globalization;
using AdcSessionMcpStdio.Models;

namespace AdcSessionMcpStdio.Services;

public sealed class SessionQueryService
{
    private const int PageSize = 5;
    private readonly Task<IReadOnlyList<Session>> sessions;

    public SessionQueryService()
        : this(SessionDeserializer.DeserializeSessionsAsync())
    {
    }

    internal SessionQueryService(IReadOnlyList<Session> sessions)
        : this(Task.FromResult(sessions))
    {
    }

    internal SessionQueryService(Task<IReadOnlyList<Session>> sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        this.sessions = sessions;
    }

    public async Task EnsureInitializedAsync()
        => _ = await sessions.ConfigureAwait(false);

    public async Task<IReadOnlyList<string>> GetDistinctThemesAsync()
    {
        var allSessions = await sessions.ConfigureAwait(false);

        return allSessions
            .SelectMany(session => session.Themes)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<SessionQueryResult> GetSessionsAsync(
        string? theme,
        string? speaker,
        string? title,
        IReadOnlyList<SessionSortKey>? sortBy,
        int page)
    {
        if (page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page), page, "Page must be greater than or equal to 1.");
        }

        var allSessions = await sessions.ConfigureAwait(false);
        var filteredSessions = allSessions.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(theme))
        {
            filteredSessions = filteredSessions.Where(session =>
                session.Themes.Any(sessionTheme => Contains(sessionTheme, theme)));
        }

        if (!string.IsNullOrWhiteSpace(speaker))
        {
            filteredSessions = filteredSessions.Where(session => Contains(session.Speaker, speaker));
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            filteredSessions = filteredSessions.Where(session => Contains(session.Title, title));
        }

        var orderedSessions = ApplySorting(filteredSessions, sortBy).ToList();
        var totalCount = orderedSessions.Count;
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)PageSize);
        var pagedSessions = orderedSessions
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return new SessionQueryResult(page, PageSize, totalCount, totalPages, pagedSessions.Count, pagedSessions);
    }

    private static IOrderedEnumerable<Session> ApplySorting(
        IEnumerable<Session> filteredSessions,
        IReadOnlyList<SessionSortKey>? sortBy)
    {
        IOrderedEnumerable<Session>? orderedSessions = null;

        if (sortBy is not null)
        {
            foreach (var sortKey in sortBy)
            {
                orderedSessions = sortKey switch
                {
                    SessionSortKey.Start => orderedSessions is null
                        ? filteredSessions.OrderBy(GetStartDateTime)
                        : orderedSessions.ThenBy(GetStartDateTime),
                    SessionSortKey.Title => orderedSessions is null
                        ? filteredSessions.OrderBy(session => session.Title, StringComparer.OrdinalIgnoreCase)
                        : orderedSessions.ThenBy(session => session.Title, StringComparer.OrdinalIgnoreCase),
                    _ => throw new ArgumentOutOfRangeException(nameof(sortBy), $"Unsupported sort key '{sortKey}'.")
                };
            }
        }

        return orderedSessions is null
            ? filteredSessions.OrderBy(session => GetNumericId(session.Id)).ThenBy(session => session.Id, StringComparer.OrdinalIgnoreCase)
            : orderedSessions.ThenBy(session => GetNumericId(session.Id)).ThenBy(session => session.Id, StringComparer.OrdinalIgnoreCase);
    }

    private static bool Contains(string value, string filter)
        => value.Contains(filter, StringComparison.OrdinalIgnoreCase);

    private static DateTime GetStartDateTime(Session session)
    {
        if (DateTime.TryParseExact(
            $"{session.Date} {session.Start}",
            "yyyy-MM-dd HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var startDateTime))
        {
            return startDateTime;
        }

        throw new InvalidDataException($"Session '{session.Id}' has invalid date/start values '{session.Date}' and '{session.Start}'.");
    }

    private static int GetNumericId(string id)
    {
        if (int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var numericId))
        {
            return numericId;
        }

        return int.MaxValue;
    }
}
