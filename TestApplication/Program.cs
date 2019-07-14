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
            KlivenNet.AddView(KlivenNet.DEBUG_ZERO_VIEW);
            KlivenNet.AddView(new Chujnia());
            KlivenNet.AddView(new Chujnia());
            //KlivenNet.AddView(new Chujnia { ooops = "plosze dzialaj"});
            KlivenNet.DEBUG_ZERO_VIEW.graczeArray[0] = new KNetPlayer(new KNetConnection(), "widzmin");
            KlivenNet.DEBUG_ZERO_VIEW.graczeArray[1] = new KNetPlayer(new KNetConnection(), "to chuj");
            KlivenNet.DEBUG_ZERO_VIEW.graczeArray[2] = new KNetPlayer(new KNetConnection(), "123");
            var bytes = KNetServer.DEBUG_SEND_BUFFERED_VALUES();
            KlivenNet.DEBUG_ZERO_VIEW.graczeArray = new KNetPlayer[1];
            //var bytes = "s";
            KlivenNet.Players.Clear();
            KlivenNet.DEBUG_ZERO_VIEW.inty = new int[] { 0, 0, -1, 0, 9 };

            Console.WriteLine(  KlivenNet.DEBUG_VIEWS.Count);
            foreach (var xd in bytes) {
                KNetClient.RecieveSerializedObject(xd);

            }

            //           var gracz = new KNetPlayer(null, "").KNetGetObject(bytes[0]);
        }
    }
}
