using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLogic.RepoModels.Gem
{
    [FirestoreData]
    public class PlayerGems : FireBaseEntity
    {

        [FirestoreProperty]
        public Dictionary<string,PlayerGem> Gems { get; set; } = new Dictionary<string, PlayerGem>();
    }
}
