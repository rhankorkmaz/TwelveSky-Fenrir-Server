namespace Fenrir.GameServer.Metadata;

public abstract class Message
{
    public const PacketType Type = PacketType.Unknown;

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
            MessageLength: (int)ms.Length + 9, // Taille totale du message (inclus en-tête)
            MessageUserId: 0, // À définir selon la logique
            MessageProtocolId: (byte)Type,
            MessagePayload: ms.ToArray()
        );
    }
}