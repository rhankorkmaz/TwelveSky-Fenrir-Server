namespace Fenrir.LoginServer.Network.Metadata;

public abstract class Message
{
    public const PacketType Type = PacketType.Unknown;
    
    public abstract PacketType PacketType { get; }

    public abstract void Deserialize(BinaryReader reader);

    public abstract void Serialize(BinaryWriter writer);

    public MessageMetadata ToMetadata()
    {
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms))
        {
            Serialize(writer);
        }

        return new MessageMetadata(
            (int)ms.Length + 9,
            0,
            (byte)Type,
            ms.ToArray()
        );
    }
}