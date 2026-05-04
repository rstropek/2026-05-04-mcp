using System.Text.Json;
using AdcSessionMcpStdio.Models;

namespace AdcSessionMcpStdio.Services;

public static class SessionDeserializer
{
    private const string SessionsFileName = "sessions.json";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static async Task<IReadOnlyList<Session>> DeserializeSessionsAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, SessionsFileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Could not find {SessionsFileName} in the build output directory '{AppContext.BaseDirectory}'. Ensure the file is configured with CopyToOutputDirectory.",
                path);
        }

        await using var stream = File.OpenRead(path);

        try
        {
            var sessions = await JsonSerializer.DeserializeAsync<List<Session>>(stream, JsonSerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            if (sessions is not { Count: > 0 })
            {
                throw new InvalidDataException($"{SessionsFileName} did not contain any sessions.");
            }

            return sessions;
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"Could not deserialize {SessionsFileName}. Verify that the file contains a valid JSON array of sessions.", exception);
        }
    }
}
