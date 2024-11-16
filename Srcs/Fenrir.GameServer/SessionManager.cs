using System.Net.Sockets;
using Fenrir.GameServer.Framing;
using Fenrir.GameServer.Metadata;
using Microsoft.Extensions.Logging;

namespace Fenrir.GameServer;

public class SessionManager
{
    private readonly ILogger<TcpLoginServer> _logger;
    private readonly TimeSpan _sessionCheckInterval = TimeSpan.FromSeconds(10);
    private readonly Dictionary<int, UserSession> _sessions;
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(3);

    public SessionManager(ILogger<TcpLoginServer> logger)
    {
        _logger = logger;
        _sessions = new Dictionary<int, UserSession>();
        _ = MonitorSessionsAsync();
    }

    public UserSession CreateSession(TcpClient client, int sessionId)
    {
        var session = new UserSession(sessionId, client, DateTime.UtcNow);
        _sessions[sessionId] = session;
        _logger.LogInformation($"Session créée : {sessionId}");
        return session;
    }

    public void RemoveSession(int sessionId)
    {
        if (!_sessions.ContainsKey(sessionId)) return;
        _sessions.Remove(sessionId);
        _logger.LogInformation($"Session supprimée : {sessionId}");
    }

    public UserSession? GetSession(int sessionId)
    {
        return _sessions.TryGetValue(sessionId, out var session) ? session : null;
    }

    private async Task MonitorSessionsAsync()
    {
        while (true)
        {
            await Task.Delay(_sessionCheckInterval);
            CheckSessionsForTimeout();
        }
    }

    private void CheckSessionsForTimeout()
    {
        var now = DateTime.UtcNow;
        var sessionsToRemove = new List<int>();

        foreach (var session in _sessions.Values)
            if (now - session.LastActivity > _sessionTimeout)
            {
                sessionsToRemove.Add(session.SessionId);
                _logger.LogWarning($"Session {session.SessionId} expirée due à l'inactivité.");
            }

        foreach (var sessionId in sessionsToRemove) RemoveSession(sessionId);
    }

    public void UpdateSessionActivity(int sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session)) return;
        session.LastActivity = DateTime.UtcNow;
        _logger.LogInformation($"Activité mise à jour pour la session {sessionId}");
    }

    // Nouvelle méthode pour envoyer un message à toutes les sessions
    public async Task BroadcastMessageAsync(MessageMetadata message)
    {
        var encodedMessage = new MessageEncoder().EncodeMessage(message, true);
        foreach (var session in _sessions.Values)
            try
            {
                using (var networkStream = session.Client.GetStream())
                {
                    await networkStream.WriteAsync(encodedMessage.ToArray(), 0, encodedMessage.Length);
                    _logger.LogInformation($"Message diffusé à la session {session.SessionId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Impossible d'envoyer un message à la session {session.SessionId}");
            }
    }

    // Méthode supplémentaire pour vérifier si une session est valide
    public bool IsSessionValid(int sessionId)
    {
        return _sessions.ContainsKey(sessionId);
    }
}

public class UserSession
{
    public UserSession(int sessionId, TcpClient client, DateTime lastActivity)
    {
        SessionId = sessionId;
        Client = client;
        LastActivity = lastActivity;
        IsAuthenticated = false; // Initialement non authentifié
    }

    public int SessionId { get; }
    public TcpClient Client { get; }
    public DateTime LastActivity { get; set; }
    public bool IsAuthenticated { get; set; } // Nouveau champ pour indiquer si l'utilisateur est authentifié
}