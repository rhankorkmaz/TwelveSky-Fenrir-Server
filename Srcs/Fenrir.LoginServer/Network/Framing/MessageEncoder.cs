using System.Buffers.Binary;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Framing;

namespace Fenrir.LoginServer.Network.Framing;

public sealed class MessageEncoder : IMessageEncoder<MessageMetadata>
{
    public ReadOnlyMemory<byte> EncodeMessage(MessageMetadata message, bool isWrittenByServer)
    {
        var buffer = new Memory<byte>(new byte[MessageMetadata.ByteSize + message.MessagePayload.Length]);

        BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[..4], message.Length);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Span[4..8], message.SessionId);
        buffer.Span[8] = message.ProtocolId;
        message.MessagePayload.Span.CopyTo(buffer.Span[9..]);

        return buffer;
    }
}