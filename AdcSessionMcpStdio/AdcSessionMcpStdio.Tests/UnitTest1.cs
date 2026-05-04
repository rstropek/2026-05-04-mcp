using AdcSessionMcpStdio.Models;
using AdcSessionMcpStdio.Services;
using AdcSessionMcpStdio.Tools;

namespace AdcSessionMcpStdio.Tests;

public sealed class SessionTests
{
    [Fact]
    public void ParseSortKeys_AcceptsAliasesInOrder()
    {
        var sortKeys = SessionTools.ParseSortKeys(["start_date_time", "title"]);

        Assert.Equal([SessionSortKey.Start, SessionSortKey.Title], sortKeys);
    }

    [Fact]
    public void ParseSortKeys_RejectsBlankEntries()
    {
        var exception = Assert.Throws<ArgumentException>(() => SessionTools.ParseSortKeys(["title", " "]));

        Assert.Contains("cannot be null, empty, or whitespace", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetDistinctThemesAsync_DeduplicatesThemesCaseInsensitively()
    {
        var service = CreateService();

        var themes = await service.GetDistinctThemesAsync();

        Assert.Equal(["AI & ML", "Cloud", "DevOps", "Testing"], themes);
    }

    [Fact]
    public async Task GetSessionsAsync_AppliesFiltersSortingAndPaging()
    {
        var service = CreateService();

        var result = await service.GetSessionsAsync(theme: "cloud", speaker: null, title: null, sortBy: [SessionSortKey.Start, SessionSortKey.Title], page: 1);

        Assert.Equal(1, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(5, result.ReturnedCount);
        Assert.Equal(["4", "2", "10", "3", "5"], result.Sessions.Select(session => session.Id));
    }

    [Fact]
    public async Task GetSessionsAsync_ReturnsEmptyPageBeyondLastPage()
    {
        var service = CreateService();

        var result = await service.GetSessionsAsync(theme: null, speaker: null, title: null, sortBy: [SessionSortKey.Start, SessionSortKey.Title], page: 3);

        Assert.Equal(3, result.Page);
        Assert.Equal(6, result.TotalCount);
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(0, result.ReturnedCount);
        Assert.Empty(result.Sessions);
    }

    [Fact]
    public async Task GetSessionsAsync_RejectsPagesBelowOne()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => service.GetSessionsAsync(theme: null, speaker: null, title: null, sortBy: null, page: 0));
    }

    private static SessionQueryService CreateService()
        => new(CreateSessions());

    private static IReadOnlyList<Session> CreateSessions()
        =>
        [
            CreateSession(id: "10", date: "2026-05-05", start: "09:00", speaker: "Alice", title: "Alpha", themes: ["Cloud"]),
            CreateSession(id: "2", date: "2026-05-05", start: "09:00", speaker: "Bob", title: "Alpha", themes: ["cloud", "AI & ML"]),
            CreateSession(id: "3", date: "2026-05-05", start: "09:00", speaker: "Cara", title: "Beta", themes: ["Cloud", "DevOps"]),
            CreateSession(id: "4", date: "2026-05-05", start: "08:00", speaker: "Dora", title: "Zeta", themes: ["Testing", "CLOUD"]),
            CreateSession(id: "5", date: "2026-05-05", start: "10:00", speaker: "Evan", title: "Aardvark", themes: ["Cloud"]),
            CreateSession(id: "6", date: "2026-05-05", start: "11:00", speaker: "Finn", title: "Gamma", themes: ["AI & ML"])
        ];

    private static Session CreateSession(string id, string date, string start, string speaker, string title, IReadOnlyList<string> themes)
        => new(
            Id: id,
            Date: date,
            Start: start,
            End: "09:45",
            Level: "100",
            LevelLabel: "Basics",
            LanguagesRaw: "english",
            Themes: themes,
            SpeakerImage: "https://example.com/speaker.jpg",
            Speaker: speaker,
            Title: title,
            Duration: "45 min",
            Language: "english",
            Description: "demo");
}
