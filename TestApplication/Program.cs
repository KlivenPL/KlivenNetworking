using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlivenNetworking;

namespace TestApplication {
    class Program {


        static void Main(string[] args) {
            KlivenNet.Players.Add(new KNetPlayer(new KNetConnection(), "jacek"));
            KlivenNet.Players.Add(new KNetPlayer(new KNetConnection(), "Placek"));
            KlivenNet.Views.Add(KlivenNet.DEBUG_ZERO_VIEW);
            KlivenNet.Views.Add(new Chujnia());
          //  KlivenNet.Views.Add(new Chujnia());
            //KlivenNet.Views.Add(new Chujnia { ooops = "plosze dzialaj"});
            var bytes = KNetServer.SendBufferedValues();


 //           var gracz = new KNetPlayer(null, "").KNetGetObject(bytes[0]);
        }
    }
}
