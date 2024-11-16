using System.Buffers.Binary;
using Fenrir.GameServer.Metadata;
using Fenrir.Network.Framing;

namespace Fenrir.GameServer.Framing;

public sealed class MessageEncoder : IMessageEncoder<MessageMetadata>
{
    public ReadOnlyMemory<byte> EncodeMessage(MessageMetadata message, bool isWrittenByServer)
    {
        var buffer = new Memory<byte>(new byte[MessageMetadata.ByteSize + message.MessagePayload.Length]);

        BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[..4], message.MessageLength);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[4..8], message.MessageUserId);
        buffer.Span[8] = message.MessageProtocolId;
        message.MessagePayload.Span.CopyTo(buffer.Span[9..]);

        return buffer;
    }
}