namespace CloudLogic.Services.Security
{
    public interface ISecureDataService
    {
        SecureDataPackage ProtectData<T>(T data);
        T UnprotectData<T>(SecureDataPackage package);
        string EncryptString(string value);
        string DecryptString(string encryptedValue);
    }
}
