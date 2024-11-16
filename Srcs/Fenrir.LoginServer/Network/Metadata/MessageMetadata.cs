namespace Fenrir.LoginServer.Network.Metadata;

public record struct MessageMetadata(
    int Length,
    int SessionId,
    byte ProtocolId,
    ReadOnlyMemory<byte> MessagePayload)
{
    public const int ByteSize = sizeof(int) + sizeof(int) + sizeof(byte);
}