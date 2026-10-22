using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.PlayerRewards
{
    [FirestoreData]
    public class Condition
    {
        [FirestoreProperty]
        public ConditionType Type { get; set; }

        [FirestoreProperty]
        public ConiditionOperator Operator { get; set; }

        [FirestoreProperty]
        public string Value { get; set; }
    }
}
