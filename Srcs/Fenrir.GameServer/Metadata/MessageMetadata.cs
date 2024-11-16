namespace Fenrir.GameServer.Metadata;

public record struct MessageMetadata(
    int MessageLength,
    int MessageUserId,
    byte MessageProtocolId,
    ReadOnlyMemory<byte> MessagePayload)
{
    public const int ByteSize = sizeof(int) + sizeof(int) + sizeof(byte);
}