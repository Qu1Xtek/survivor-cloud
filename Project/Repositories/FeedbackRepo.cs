using CloudLogic.RepoModels;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CloudLogic.Repositories
{
    public class FeedbackRepo : BaseFireStoreService
    {
        public FeedbackRepo(ILogger logger) : base("Feedback", logger)
        {
        }

        public async Task<Feedback> CreateFeedback(Feedback feedback)
        {
            if (feedback.DateCreated == default)
            {
                feedback.DateCreated = Timestamp.FromDateTime(DateTime.UtcNow);
            }

            return await StoreRecord(feedback);
        }
    }
}
