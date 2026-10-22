using CloudLogic.RepoModels;
using CloudLogic.Repositories;
using CloudLogic.Services.Interfaces;
using CloudLogic.Services.Items;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Services
{
    public class BugService : IBugService
    {
        private static readonly string[] AdminPlayerIds =
        {
            "qLhkZZG07ZoOieG0Vqr6XmetCIfu",
            "273SpwBtLflB1Ldq5TMflm0xBEpT"
        };

        private readonly BugReportRepo _bugRepo;
        private readonly IPlayerManagerService _playerManager;
        private readonly IFCMService _fcmService;
        private readonly ILogger _logger;

        public BugService(
            BugReportRepo bugRepo,
            IPlayerManagerService playerManager,
            IFCMService fcmService,
            ILogger logger)
        {
            _bugRepo = bugRepo;
            _playerManager = playerManager;
            _fcmService = fcmService;
            _logger = logger;
        }

        public async Task ReportBug(string playerId, string bugInfo, string bugDescription)
        {
            var report = new BugReport
            {
                PlayerReportId = playerId,
                BugInfo = bugInfo,
                BugDescription = bugDescription,
                DateCreated = Timestamp.FromDateTime(DateTime.UtcNow),
                Solved = false
            };

            await _bugRepo.CreateBugReport(report);

            foreach (var adminId in AdminPlayerIds)
            {
                try
                {
                    var adminData = await _playerManager.GetPlayerDataAsync(adminId);
                    if (adminData?.DeviceId == null)
                    {
                        continue;
                    }

                    await _fcmService.SendNotif(
                        adminData.DeviceId,
                        report.BugInfo,
                        report.BugDescription,
                        $"{bugInfo}",
                        "{}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to notify admin {adminId}: {ex.Message}");
                }
            }
        }
    }
}
