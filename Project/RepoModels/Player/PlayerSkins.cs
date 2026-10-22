using Google.Cloud.Firestore;

namespace CloudLogic.RepoModels.Player
{
    [FirestoreData]
    public class PlayerSkins : FireBaseEntity
    {
        [FirestoreProperty]
        public EquippedSkins EquippedSkins { get; set; } = new EquippedSkins();
    }
}
