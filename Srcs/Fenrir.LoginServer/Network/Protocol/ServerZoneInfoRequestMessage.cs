using Fenrir.LoginServer.Network.Metadata;

namespace Fenrir.LoginServer.Network.Protocol;

public sealed class ServerZoneInfoRequestMessage : Message
{
    public new const PacketType Type = PacketType.ServerZoneInfo;
    
    public override PacketType PacketType => Type;

    public ServerZoneInfoRequestMessage()
    {
    }

    public ServerZoneInfoRequestMessage(int avatarPost)
    {
        AvatarPost = avatarPost;
    }

    public int AvatarPost { get; set; }


    public override void Deserialize(BinaryReader reader)
    {
        AvatarPost = reader.ReadInt32();
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(AvatarPost);
    }
}