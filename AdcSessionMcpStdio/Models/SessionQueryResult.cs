namespace AdcSessionMcpStdio.Models;

public sealed record SessionQueryResult(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    int ReturnedCount,
    IReadOnlyList<Session> Sessions);
