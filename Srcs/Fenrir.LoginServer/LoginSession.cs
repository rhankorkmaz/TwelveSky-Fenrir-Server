using System.IO.Pipelines;
using System.Net.Sockets;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Framing;
using Fenrir.Network.Options;
using Fenrir.Network.Transport;
using Microsoft.Extensions.Logging;

namespace Fenrir.LoginServer;

public sealed class LoginSession(
    Socket socket,
    IMessageParser<MessageMetadata> messageParser,
    IMessageDispatcher<MessageMetadata> messageDispatcher,
    ILogger logger,
    FenrirServerOptions options)
    : FenrirSession<MessageMetadata>(socket, messageParser, messageDispatcher, logger, options)
{
    public int SessionId { get; set; }
    
    private const byte XorKey = 0x5A; // Clé XOR utilisée pour le chiffrement/déchiffrement

    public ValueTask SendAsync<TMessage>()
        where TMessage : Message, new()
    {
        return SendAsync(new TMessage());
    }
    
    public ValueTask SendAsync(Message message)
    {
        using var writer = new BinaryWriter(new MemoryStream());
        message.Serialize(writer);
        
        var payload = ((MemoryStream)writer.BaseStream).ToArray();
        
        for (var i = 0; i < payload.Length; i++)
            payload[i] ^= XorKey;

        var metadata = new MessageMetadata(payload.Length, SessionId, (byte)message.PacketType, payload);
        
        var buffer = _messageEncoder.EncodeMessage(metadata, true);
        
        var vt = _pipe.Output.WriteAsync(buffer, SessionClosed);
        
        return vt.IsCompletedSuccessfully
            ? ValueTask.CompletedTask
            : WaitAndFlush(vt);

        static async ValueTask WaitAndFlush(ValueTask<FlushResult> vt)
        {
            try
            {
                await vt.ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }
        }
    }
}