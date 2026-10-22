namespace CloudLogic.Services.Security
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        byte[] EncryptBytes(byte[] plainBytes);
        byte[] DecryptBytes(byte[] cipherBytes);
    }
}
