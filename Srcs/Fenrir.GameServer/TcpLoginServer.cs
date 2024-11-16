using System.Net;
using System.Net.Sockets;
using System.Text;
using Fenrir.GameServer.Metadata;
using Microsoft.Extensions.Logging;

namespace Fenrir.GameServer;

public class TcpLoginServer
{
    private const int Port = 11091;
    private const string IpAddress = "127.0.0.1";
    private readonly ILogger<TcpLoginServer> _logger;
    private readonly UnifiedProtocolHandler _protocolHandler;
    private readonly TcpListener _server;
    private readonly SessionManager _sessionManager;
    private static int _sessionIdCounter = 0;

    public TcpLoginServer(ILogger<TcpLoginServer> logger)
    {
        _server = new TcpListener(IPAddress.Parse(IpAddress), Port);
        _logger = logger;
        _sessionManager = new SessionManager(logger);
        _protocolHandler = new UnifiedProtocolHandler(_sessionManager, logger);
    }

    public async Task StartAsync()
    {
        _server.Start();
        _logger.LogInformation($"Serveur démarré sur {IpAddress}:{Port}");

        while (true)
        {
            try
            {
                var client = await _server.AcceptTcpClientAsync();
                _logger.LogInformation("Nouveau client accepté.");
                var session = _sessionManager.CreateSession(client, ++_sessionIdCounter);
                _logger.LogInformation($"Session créée : {session.SessionId}");
                _ = HandleClientAsync(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'acceptation d'un nouveau client.");
            }
        }
    }

    private async Task HandleClientAsync(UserSession session)
    {
        _logger.LogInformation("Début de la gestion du client...");
        try
        {
            using (var networkStream = session.Client.GetStream())
            {
                networkStream.ReadTimeout = 5000;

                // Envoi du message de handshake initial
                byte[] handshakePacket = CreateHandshakePacket(session.SessionId);
                await session.Client.Client.AcceptAsync();
                await networkStream.WriteAsync(handshakePacket, 0, handshakePacket.Length);
                _logger.LogInformation("Message de handshake envoyé.");

                byte[] buffer = new byte[4096];
                Socket socket = session.Client.Client;
                bool handshakeValid = false;

                // Tentative de handshake avec le client
                for (int attempt = 0; attempt < 10 && !handshakeValid; attempt++)
                {
                    _logger.LogInformation("Attente de la réponse de handshake du client...");
                    if (networkStream.DataAvailable)
                    {
                        int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                        string clientResponse = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                        if (clientResponse == "HELLO SERVER")
                        {
                            _logger.LogInformation("Handshake avec le client réussi.");
                            handshakeValid = true;
                        }
                        else
                        {
                            _logger.LogWarning("Handshake invalide. Fermeture de la connexion.");
                            return;
                        }
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }

                if (!handshakeValid)
                {
                    _logger.LogWarning("Le handshake n'a pas pu être validé après plusieurs tentatives. Fermeture de la connexion.");
                    return;
                }

                // Gestion du client après validation du handshake
                while (true)
                {
                    _logger.LogInformation("Tentative de lecture du buffer...");

                    // Vérifier si le client est toujours connecté
                    if (socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0)
                    {
                        _logger.LogInformation("Le client semble avoir fermé la connexion.");
                        break;
                    }

                    try
                    {
                        if (networkStream.DataAvailable)
                        {
                            int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                            _logger.LogInformation($"Bytes lus: {bytesRead}");

                            if (bytesRead == 0)
                            {
                                _logger.LogInformation("Le client a fermé la connexion.");
                                break;
                            }

                            // Log des données brutes avant déchiffrement
                            string rawData = BitConverter.ToString(buffer, 0, bytesRead);
                            _logger.LogInformation($"Données brutes reçues : {rawData}");

                            // Appliquer le XOR pour déchiffrer les données reçues
                            _logger.LogInformation("Application du XOR pour le déchiffrement...");
                            var decryptedBuffer = XorHandler.ApplyXor(buffer, bytesRead);
                            _logger.LogInformation("Déchiffrement du buffer terminé.");

                            // Log des données déchiffrées
                            string decryptedData = BitConverter.ToString(decryptedBuffer, 0, bytesRead);
                            _logger.LogInformation($"Données déchiffrées : {decryptedData}");

                            // Convertir le buffer en MessageMetadata pour le traitement
                            if (bytesRead >= 9) // Vérifier que la taille minimale est respectée
                            {
                                var messageMetadata = new MessageMetadata(
                                    MessageLength: BitConverter.ToInt32(decryptedBuffer, 0),
                                    MessageUserId: BitConverter.ToInt32(decryptedBuffer, 4),
                                    MessageProtocolId: decryptedBuffer[8],
                                    MessagePayload: new ReadOnlyMemory<byte>(decryptedBuffer, 9, bytesRead - 9)
                                );

                                // Traiter le message reçu avec UnifiedProtocolHandler
                                await _protocolHandler.HandleClientProtocolAsync(session, messageMetadata);
                            }
                            else
                            {
                                _logger.LogWarning("Paquet reçu trop petit pour contenir un en-tête valide.");
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Aucune donnée disponible pour le moment, en attente...");
                            await Task.Delay(500);
                        }
                    }
                    catch (IOException ioEx)
                    {
                        _logger.LogWarning(ioEx, "Timeout ou erreur de lecture sur le réseau");
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la gestion du client.");
        }
        finally
        {
            _sessionManager.RemoveSession(session.SessionId);
            session.Client.Close();
            _logger.LogInformation("Connexion client fermée.");
        }
    }

    private byte[] CreateHandshakePacket(int sessionId)
    {
        // Création d'un paquet structuré pour envoyer "HELLO" en tant que handshake
        int messageLength = 9; // Longueur totale du message (int + int + byte)
        int messageUserId = sessionId;
        byte protocolId = 0x01; // Protocole pour "HELLO"

        byte[] packet = new byte[messageLength];
        BitConverter.GetBytes(messageLength).CopyTo(packet, 0);
        BitConverter.GetBytes(messageUserId).CopyTo(packet, 4);
        packet[8] = protocolId;

        return packet;
    }
}