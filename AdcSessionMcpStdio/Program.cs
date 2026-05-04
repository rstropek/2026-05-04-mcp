using AdcSessionMcpStdio.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<SessionQueryService>();
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

using var host = builder.Build();

await host.Services
    .GetRequiredService<SessionQueryService>()
    .EnsureInitializedAsync()
    .ConfigureAwait(false);

await host.RunAsync().ConfigureAwait(false);
