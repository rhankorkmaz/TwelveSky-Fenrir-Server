using System.Net.Sockets;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Collections;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Options;
using Fenrir.Network.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Fenrir.LoginServer;

public class LoginServer : FenrirServer<FenrirSession<MessageMetadata>, MessageMetadata>
{
    private readonly ILogger<LoginServer> _logger;
    private readonly IMessageDispatcher<MessageMetadata> _messageDispatcher;
    private readonly IServiceProvider _provider;
    private readonly ISessionCollection<FenrirSession<MessageMetadata>> _sessions;

    public LoginServer(
        IOptions<FenrirServerOptions> options,
        IMessageDispatcher<MessageMetadata> messageDispatcher,
        ILoggerFactory loggerFactory,
        IServiceProvider provider,
        ISessionCollection<FenrirSession<MessageMetadata>> sessions)
        : base(options, messageDispatcher, loggerFactory, provider, sessions)
    {
        _messageDispatcher = messageDispatcher;
        _logger = loggerFactory.CreateLogger<LoginServer>();
        _provider = provider;
        _sessions = sessions;
    }

    protected override FenrirSession<MessageMetadata> CreateSession(Socket socket,
        IMessageParser<MessageMetadata> messageParser, IMessageDispatcher<MessageMetadata> messageDispatcher,
        ILogger logger, FenrirServerOptions options)
    {
        var session = new LoginSession(socket, messageParser, messageDispatcher, logger, options);

        _logger.LogInformation($"Nouvelle session créée avec l'ID de session : {session.SessionId}");

        return session;
    }

    protected override bool CanAddSession(FenrirSession<MessageMetadata> session)
    {
        var canAdd = !_sessions.IsFull;
        if (!canAdd) _logger.LogWarning("Cannot add session. The session collection is full.");
        return canAdd;
    }

    protected override async Task OnSessionConnectedAsync(FenrirSession<MessageMetadata> session)
    {
        _sessions.AddSession(session);
        _logger.LogInformation("Client {SessionId} connected.", session.SessionId);
        await base.OnSessionConnectedAsync(session);
    }

    protected override async Task OnSessionDisconnectedAsync(FenrirSession<MessageMetadata> session)
    {
        _sessions.RemoveSession(session.SessionId);
        _logger.LogInformation("Client {SessionId} disconnected.", session.SessionId);
        await base.OnSessionDisconnectedAsync(session);
    }
}