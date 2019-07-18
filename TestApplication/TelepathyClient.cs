using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlivenNetworking;
namespace TestApplication {

    class TelepathyClient : KNetClient {
        Telepathy.Client client = new Telepathy.Client();
        public bool Connected { get; private set; }

        public void Connect(string ip, int port) {
            client.Connect(ip, port);
            Chat.FixedUpdate += FixedUpdate;
        }

        void FixedUpdate() {
            Telepathy.Message msg;
            while (client.GetNextMessage(out msg)) {
                switch (msg.eventType) {
                    case Telepathy.EventType.Connected:
                        OnConnectedToServer();
                        Connected = true;
                        Chat.FixedUpdate += Chat.Instance.ClientUpdate;
                        break;
                    case Telepathy.EventType.Data:
                        OnBytesRecieved(msg.data);
                        break;
                    case Telepathy.EventType.Disconnected:
                        if (Connected) {
                            Connected = false;
                            Console.WriteLine("Disconnected from server");
                            Console.ReadLine();
                            Environment.Exit(0x0);
                        } else {
                            Console.WriteLine("Could not connect to server, starting a new one...");
                            Chat.server = new TelepathyServer();
                            Chat.server.StartServer(2137);
                            Chat.client = null;
                        }
                        Chat.FixedUpdate -= FixedUpdate;
                        break;
                }
            }
        }

    }
}
