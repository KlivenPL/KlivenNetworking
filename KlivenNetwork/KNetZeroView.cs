using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public sealed class KNetZeroView : KNetView {
        [KNetBufferedValue]
        public List<KNetPlayer> Players = new List<KNetPlayer>();

       // [KNetBufferedValue]
       // public List<IKNetBufferable<int>> xddddddd;
        [KNetBufferedValue]
        public int[] inty;

        [KNetBufferedValue]
        public float dupaxd = 2137f;

        [KNetBufferedValue]
        public KNetServer powinnoBycFalse;
    }

    public class Chujnia : KNetView {
        // [KNetBufferedValue]
        // public string ooops = "i chuj";

        [KNetBufferedValue]
        public KNetPlayer player = new KNetPlayer(null, "KURWAAAAAAA");
    }
}
