using System.Buffers;
using Fenrir.Framework.Extensions;
using Fenrir.LoginServer.Network.Metadata;
using Fenrir.Network.Framing;

namespace Fenrir.LoginServer.Network.Framing;

public sealed class MessageDecoder : IMessageDecoder<MessageMetadata>
{
    public IEnumerable<MessageMetadata> DecodeMessages(ReadOnlySequence<byte> sequence, bool isReadByServer)
    {
        var reader = new BinaryReader(new MemoryStream(sequence.ToArray()));

        while (reader.BaseStream.Remaining() >= MessageMetadata.ByteSize)
        {
            if (!reader.BaseStream.TryRead(() => reader.ReadInt32(), out var messageLength))
                yield break;

            if (!reader.BaseStream.TryRead(() => reader.ReadInt32(), out var messageUserId))
                yield break;

            if (!reader.BaseStream.TryRead(() => reader.ReadByte(), out var messageProtocolId))
                yield break;

            var messagePayload = reader.ReadBytes(reader.BaseStream.Remaining());

            yield return new MessageMetadata(messageLength, messageUserId, messageProtocolId, messagePayload);
        }
    }
}