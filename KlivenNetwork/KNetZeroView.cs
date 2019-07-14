using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public sealed class KNetZeroView : KNetView {
        [KNetBufferedObject]
        public List<int> intyLista;

        [KNetBufferedObject]
        public List<KNetPlayer> Players = new List<KNetPlayer>();

       
        [KNetBufferedObject]
        public int[] inty = new int[]{2, 1, 3, 7 };

        [KNetBufferedObject]
        public KNetPlayer[] graczeArray = new KNetPlayer[3];

        [KNetBufferedObject]
        public float dupaxd = 2137f;

        [KNetBufferedObject]
        public KNetServer powinnoBycFalse;
    }

    public class Chujnia : KNetView {
        // [KNetBufferedValue]
        // public string ooops = "i chuj";

        [KNetBufferedObject]
        public KNetPlayer player = new KNetPlayer(null, "KURWAAAAAAA");
    }
}
