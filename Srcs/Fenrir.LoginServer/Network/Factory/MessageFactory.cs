using System.Diagnostics.CodeAnalysis;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.LoginServer.Network.Protocol;
using Fenrir.Network.Factory;

namespace Fenrir.LoginServer.Network.Factory;

public class MessageFactory : IMessageFactory<PacketType, Message>
{
    private readonly Dictionary<PacketType, Func<Message>> _messages = new()
    {
        [PacketType.ServerZoneInfo] = () => new ServerZoneInfoRequestMessage()
    };

    public bool TryGetMessage(PacketType key, [NotNullWhen(true)] out Message? message)
    {
        message = null;

        if (!_messages.TryGetValue(key, out var factory))
            return false;

        message = factory();
        return false;
    }
}