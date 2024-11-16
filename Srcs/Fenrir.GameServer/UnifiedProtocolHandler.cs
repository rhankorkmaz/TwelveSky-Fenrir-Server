using System.Text;
using Fenrir.GameServer.Framing;
using Fenrir.GameServer.Metadata;
using Microsoft.Extensions.Logging;

namespace Fenrir.GameServer;

public class UnifiedProtocolHandler
{
    private readonly ILogger<TcpLoginServer> _logger;
    private readonly MessageEncoder _messageEncoder = new();
    private readonly SessionManager _sessionManager;

    public UnifiedProtocolHandler(SessionManager sessionManager, ILogger<TcpLoginServer> logger)
    {
        _sessionManager = sessionManager;
        _logger = logger;
    }

    public async Task HandleClientProtocolAsync(UserSession session, MessageMetadata messageMetadata)
    {
        _logger.LogInformation(
            $"Handling protocol: {messageMetadata.MessageProtocolId} for session {session.SessionId}");

        try
        {
            switch (messageMetadata.MessageProtocolId)
            {
                case (byte)PacketType.LoginRequest:
                    await HandleLoginRequestAsync(session, messageMetadata);
                    break;

                case (byte)PacketType.ServerListRequest:
                    await HandleRequestServerListAsync(session, messageMetadata);
                    break;

                // Ajouter d'autres protocoles ici si nécessaire
                default:
                    _logger.LogWarning($"Protocole non reconnu : {messageMetadata.MessageProtocolId}");
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Erreur lors du traitement du protocole {messageMetadata.MessageProtocolId} pour la session {session.SessionId}");
        }
    }

    private async Task HandleLoginRequestAsync(UserSession session, MessageMetadata messageMetadata)
    {
        _logger.LogInformation($"Traitement de la requête de connexion pour la session : {session.SessionId}");

        try
        {
            // Exemple de réponse à une requête de connexion
            var responsePayload = Encoding.ASCII.GetBytes("LOGIN_SUCCESS");
            var responseMetadata = new MessageMetadata(
                responsePayload.Length + 9, // Longueur totale du message
                session.SessionId, // ID utilisateur en fonction de la session
                (byte)PacketType.LoginRecv, // ID Protocole pour la réponse de connexion
                responsePayload
            );

            var encodedResponse = _messageEncoder.EncodeMessage(responseMetadata, true);

            using (var networkStream = session.Client.GetStream())
            {
                await networkStream.WriteAsync(encodedResponse.ToArray(), 0, encodedResponse.Length);
                _logger.LogInformation("Réponse de connexion envoyée au client.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Erreur lors de l'envoi de la réponse de connexion pour la session {session.SessionId}");
        }
    }

    private async Task HandleRequestServerListAsync(UserSession session, MessageMetadata messageMetadata)
    {
        _logger.LogInformation($"Traitement de la requête de liste de serveurs pour la session : {session.SessionId}");

        try
        {
            var serverListResponse = CreateServerListResponse();
            var encodedResponse = _messageEncoder.EncodeMessage(serverListResponse, true);

            using (var networkStream = session.Client.GetStream())
            {
                await networkStream.WriteAsync(encodedResponse.ToArray(), 0, encodedResponse.Length);
                _logger.LogInformation("Liste de serveurs envoyée au client.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de l'envoi de la liste de serveurs pour la session {session.SessionId}");
        }
    }

    private MessageMetadata CreateServerListResponse()
    {
        // Pour cet exemple, nous allons créer une réponse avec un seul serveur fictif.
        var serverData = "127.0.0.1:15000"u8.ToArray();
        var messageLength = 9 + serverData.Length; // 9 octets pour l'en-tête, et la longueur des données serveur

        var response = new MessageMetadata(messageLength, 0, (byte)PacketType.ServerZoneInfo, serverData);

        return response;
    }
}