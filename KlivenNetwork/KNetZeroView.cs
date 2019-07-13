using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public sealed class KNetZeroView : KNetView {
        [KNetBufferedValue]
        public List<KNetPlayer> Players = new List<KNetPlayer>();

        [KNetBufferedValue]
        public int dupaxd = 2137;
    }

    public class Chujnia : KNetView {
        // [KNetBufferedValue]
        // public string ooops = "i chuj";

        [KNetBufferedValue]
        public KNetPlayer player = new KNetPlayer(null, "KURWAAAAAAA");
    }
}
