namespace Fenrir.Framework.Extensions;

public static class XorEncryption
{
    private const byte XorKey = 0x5A; // Clé XOR utilisée pour le chiffrement/déchiffrement

    public static byte[] Encrypt(byte[] data)
    {
        for (int i = 0; i < data.Length; i++)
        {
            data[i] ^= XorKey;
        }
        return data;
    }

    public static byte[] Decrypt(byte[] data)
    {
        return Encrypt(data); // XOR est son propre inverse
    }
}