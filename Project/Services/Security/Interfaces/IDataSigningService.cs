using System;

namespace CloudLogic.Services.Security
{
    public interface IDataSigningService
    {
        string SignData(string data);
        bool VerifySignature(string data, string signature);
        string SignDataWithTimestamp(string data, DateTime timestamp);
        bool VerifySignatureWithTimestamp(string data, string signature, DateTime timestamp, TimeSpan validWindow);
    }
}
