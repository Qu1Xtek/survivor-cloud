using CloudLogic.DI;
using CloudLogic.Response.DTO;
using CloudLogic.Services.Duel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.CloudCode.Core;

namespace CloudLogic.Modules
{
    public class DuelModule
    {
        private readonly ILogger _logger;

        public DuelModule(ILogger<DuelModule> logger)
        {
            _logger = logger;
        }

        [CloudCodeFunction(nameof(GetDuelNpcs))]
        public Task<List<DuelNpcDTO>> GetDuelNpcs(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<DuelService>().GetNpcs(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(GetDuelLoadout))]
        public Task<DuelLoadoutDTO> GetDuelLoadout(IExecutionContext ctx)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<DuelService>().GetLoadout(ctx.PlayerId));
        }

        [CloudCodeFunction(nameof(SetDuelLoadout))]
        public Task<DuelLoadoutDTO> SetDuelLoadout(IExecutionContext ctx, List<int> spells)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<DuelService>().SetLoadout(ctx.PlayerId, spells));
        }

        [CloudCodeFunction(nameof(SetDuelStance))]
        public Task<DuelLoadoutDTO> SetDuelStance(IExecutionContext ctx, int stance)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<DuelService>().SetStance(ctx.PlayerId, stance));
        }

        [CloudCodeFunction(nameof(FightDuelNpc))]
        public Task<DuelResultDTO> FightDuelNpc(IExecutionContext ctx, string npcId)
        {
            return CloudDi.WithScope(_logger, ctx, sp =>
                sp.GetRequiredService<DuelService>().FightNpc(ctx.PlayerId, npcId));
        }
    }
}
