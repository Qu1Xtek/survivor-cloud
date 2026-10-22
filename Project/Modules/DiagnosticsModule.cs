using CloudLogic.DI;
using CloudLogic.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class DiagnosticsModule
    {
        private readonly ILogger _logger;

        public DiagnosticsModule(ILogger<DiagnosticsModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(LogClientEvent))]
        public async Task LogClientEvent(IExecutionContext ctx, string stage, string reason, string platform, string appVersion, float time = -1)
        {
            await CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<ClientDiagnosticsRepo>().LogEvent(ctx.PlayerId, stage, reason, platform, appVersion, time));
        }
    }
}
