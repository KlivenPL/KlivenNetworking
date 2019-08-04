using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    internal sealed class KNetZeroView : KNetView {

        [KNetBufferedObject]
        public List<KNetPlayer> Players = new List<KNetPlayer>();

        [KNetRPC]
        void JebnijSobie(string monsterka, int[] raz, KNetPlayer dwa, int trzy) {
            KNetLogger.Log($"jebnij sobie {monsterka}, {raz.Length}, {dwa}, {trzy}");
            KNetLogger.Log(string.Join(", ", raz));
        }

        internal void RecieveClientInfo(string name) {


        }
        [KNetRPC]
        internal void AddNewPlayer(KNetConnection connection) {
             KNetPlayer player = new KNetPlayer(connection);
             Players.Add(player);
         }
    }
}
