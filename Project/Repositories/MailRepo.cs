using CloudLogic.RepoModels.Items;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class MailRepo : BaseFireStoreService
    {
        public MailRepo(ILogger logger) : base("Mail", logger)
        {
        }

        public async Task<Mail> GetMailById(string id, Transaction tran = null)
        {
            return await GetRecordById<Mail>(id, tran);
        }

        public async Task<List<Mail>> GetPlayerMail(string playerId)
        {
            var collection = GetCollectionRef();
            Query query = collection.WhereEqualTo("PlayerId", playerId);

            return await GetRecordsByQuery<Mail>(query);
        }

        public async Task<Mail> StoreMail(Mail mail)
        {
            return await StoreRecord(mail);
        }


        public async Task DeleteMail(string mailId, Transaction tran = null)
        {
            await DeleteRecordById(mailId, tran);
        }

        public async Task<int> TransferPlayerMailAsync(string fromPlayerId, string toPlayerId)
        {
            if (string.IsNullOrWhiteSpace(fromPlayerId))
            {
                throw new ArgumentException("Source player id cannot be null or empty.", nameof(fromPlayerId));
            }

            if (string.IsNullOrWhiteSpace(toPlayerId))
            {
                throw new ArgumentException("Target player id cannot be null or empty.", nameof(toPlayerId));
            }

            if (string.Equals(fromPlayerId, toPlayerId, StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            var collection = GetCollectionRef();

                var query = collection.WhereEqualTo("PlayerId", fromPlayerId);
                var snapshot = await query.GetSnapshotAsync();
                var documents = snapshot.Documents;

                if (documents.Count == 0)
                {
                    return 0;
                }

                var batch = _firestoreDb.StartBatch();

                foreach (var document in documents)
                {
                    batch.Update(document.Reference, new Dictionary<string, object>
                    {
                        { "PlayerId", toPlayerId }
                    });
                }

                await batch.CommitAsync();

                return documents.Count;
            
        }



    }
}
