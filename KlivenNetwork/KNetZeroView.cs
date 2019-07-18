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
        internal void AddNewPlayer(KNetConnection connection) {
            KNetPlayer player = new KNetPlayer(connection);
            Players.Add(player);
        }


    }
}
