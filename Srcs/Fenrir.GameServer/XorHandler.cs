namespace Fenrir.GameServer;

public static class XorHandler
{
    private const byte XorKey = 0x5A; // Clé XOR utilisée pour le chiffrement/déchiffrement

    public static byte[] ApplyXor(byte[] data, int length)
    {
        var result = new byte[length];
        for (var i = 0; i < length; i++) result[i] = (byte)(data[i] ^ XorKey);
        return result;
    }

    public static byte[] ApplyXor(ReadOnlyMemory<byte> responseMetadataMessagePayload, int byteSize)
    {
        // Récupérer le buffer à partir de ReadOnlyMemory<byte>
        var dataBuffer = responseMetadataMessagePayload.ToArray();

        // Appliquer le chiffrement XOR en utilisant la méthode existante
        return ApplyXor(dataBuffer, byteSize);
    }
}