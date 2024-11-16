using Microsoft.Extensions.Logging;

namespace Fenrir.GameServer;

internal class Program
{
    public static async Task Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

        var logger = loggerFactory.CreateLogger<TcpLoginServer>();
        var server = new TcpLoginServer(logger);
        await server.StartAsync();
    }
}