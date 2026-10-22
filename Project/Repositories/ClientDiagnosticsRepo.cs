using CloudLogic.RepoModels.Diagnostics;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class ClientDiagnosticsRepo : BaseFireStoreService
    {
        public ClientDiagnosticsRepo(ILogger logger) : base("ClientDiagnostics", logger)
        {
        }

        public async Task LogEvent(string playerId, string stage, string reason, string platform, string appVersion, float time = -1)
        {
            try
            {
                var entry = new ClientDiagnosticEntry
                {
                    PlayerId = playerId,
                    Stage = stage,
                    Reason = reason,
                    Platform = platform,
                    AppVersion = appVersion,
                    Time = time,
                    CreatedAt = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await CreateRecord(entry);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning($"ClientDiagnosticsRepo: failed to log '{stage}': {ex.Message}");
            }
        }
    }
}
