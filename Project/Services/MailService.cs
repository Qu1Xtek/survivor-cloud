using CloudLogic.RepoModels.Items;
using CloudLogic.RepoModels.Player;
using CloudLogic.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CloudLogic.Services
{
    public class MailService : IMailService
    {
        private readonly MailRepo _mailRepo;
        private readonly IFCMService _fcmService;
        private readonly PlayerDataRepo _playerDataRepo;

        public MailService(MailRepo repo, IFCMService fcmService, PlayerDataRepo playerDataRepo)
        {
            _mailRepo = repo;
            _fcmService = fcmService;
            _playerDataRepo = playerDataRepo;
        }


        public async Task<List<Mail>> GetPlayerMail(string playerId)
        {
            return await _mailRepo.GetPlayerMail(playerId);
        }

        public async Task ClaimMail(string playerId, string mailId)
        {
            await _mailRepo.RunOurTransactionAsync(async tran =>
            {
                var mail = await _mailRepo.GetMailById(mailId, tran);

                if (mail == null || mail.PlayerId != playerId)
                {
                    return;
                }

                if (mail.Gold > 0)
                {
                    var player = await _playerDataRepo.GetPlayerDataByPlayerId(playerId, tran);
                    await _playerDataRepo.UpsertPlayerData(new PlayerData { Id = player.Id, Gold = player.Gold + mail.Gold }, player, tran);
                }

                await _mailRepo.DeleteMail(mailId, tran);
            });
        }


        public async Task<Mail> CreateMail(MailNotifDto dto)
        {
            var mail = new Mail()
            {
                PlayerId = dto.PlayerId,
                Title = dto.Title,
                Message = dto.Message,
                Read = false,
                Claim = dto.Claim,
                MailData = dto.MailData,
                Gold = dto.Gold,
            };

            var createdMail = await _mailRepo.StoreMail(mail);

            await _fcmService.SendNotif(dto.DeviceId, "mail_received", dto.Title, dto.Message, JsonConvert.SerializeObject(mail));

            return createdMail;
        }

        public async Task<bool> TransferMail(string playerId, string transferTo)
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                throw new ArgumentException("playerId cannot be null or empty.", nameof(playerId));
            }

            if (string.IsNullOrWhiteSpace(transferTo))
            {
                throw new ArgumentException("transferTo cannot be null or empty.", nameof(transferTo));
            }

            if (string.Equals(playerId, transferTo, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var transferredCount = await _mailRepo.TransferPlayerMailAsync(playerId, transferTo);

            return transferredCount > 0;
        }
    }
}
