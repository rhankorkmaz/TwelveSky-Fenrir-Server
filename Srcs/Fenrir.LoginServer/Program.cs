using Fenrir.LoginServer;
using Fenrir.LoginServer.Network.Dispatcher;
using Fenrir.LoginServer.Network.Framing;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Collections;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Options;
using Fenrir.Network.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services
            .AddSingleton<ISessionCollection<FenrirSession<MessageMetadata>>,
                SessionCollection<FenrirSession<MessageMetadata>, MessageMetadata>>();
        services.AddSingleton<IMessageDispatcher<MessageMetadata>, MessageDispatcher>();
        services.AddSingleton<IMessageParser<MessageMetadata>, MessageParser>();
        services.AddSingleton<ILoggerFactory, LoggerFactory>();

        services.Configure<FenrirServerOptions>(options =>
        {
            options.IpAddress = "127.0.0.1";
            options.Port = 11091;
            options.MaxConnections = 100;
            options.EnableKeepAlive = true;
            options.KeepAliveInterval = 10;
            options.EnableLogging = true;
            options.MaxConnectionsByIpAddress = 10;
        });

        services.AddSingleton<LoginServer>();
    })
    .Build();

var loginServer = host.Services.GetRequiredService<LoginServer>();

await loginServer.StartAsync();