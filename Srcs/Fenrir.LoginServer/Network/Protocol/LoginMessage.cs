using Fenrir.LoginServer.Network.Metadata;

namespace Fenrir.LoginServer.Network.Protocol;

public class LoginMessage : Message
{
    // TODO: Implement PacketType enum
    public override PacketType PacketType { get; }
    
    public string Username { get; set; }
    public string Password { get; set; }

    public override void Deserialize(BinaryReader reader)
    {
        Username = reader.ReadString();
        Password = reader.ReadString();
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(Username);
        writer.Write(Password);
    }
}