using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Dispatcher;
using Fenrir.Network.Transport;

namespace Fenrir.LoginServer.Network.Dispatcher;

public class MessageDispatcher : IMessageDispatcher<Message>
{
    private readonly Dictionary<PacketType, Func<FenrirSession<MessageMetadata>, Message, Task>> _handlers =
        new();

    public async Task<DispatchResults> DispatchAsync(FenrirSession<MessageMetadata> session, Message message)
    {
        if (!_handlers.TryGetValue((PacketType)message.ProtocolId, out var handler))
            return DispatchResults.NotMapped;
        await handler(session, message);
        return DispatchResults.Succeeded;
    }

    public void RegisterHandler(PacketType packetType,
        Func<FenrirSession<MessageMetadata>, MessageMetadata, Task> handler)
    {
        _handlers[packetType] = handler;
    }
}