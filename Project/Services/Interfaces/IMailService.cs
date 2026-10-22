using CloudLogic.RepoModels.Items;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services
{
    public interface IMailService
    {
        Task<List<Mail>> GetPlayerMail(string playerId);
        Task ClaimMail(string playerId, string mailId);
        Task<Mail> CreateMail(MailNotifDto dto);
        Task<bool> TransferMail(string playerId, string transferTo);
    }
}
