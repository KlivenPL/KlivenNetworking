using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlivenNetworking;
using Telepathy;

namespace TestApplication {
    public class TelepathyServer : KNetServer {
        Telepathy.Server server = new Telepathy.Server();
        protected override void SendBytes(KNetConnection target, byte[] bytes) {
            server.Send(target.Id, bytes);
        }

       public void StartServer(int port) {
            server.Start(port);
            OnServerStarted();
            Chat.FixedUpdate += FixedUpdate;
        }

        void FixedUpdate() {
            Telepathy.Message msg;
            while (server.GetNextMessage(out msg)) {
                switch (msg.eventType) {
                    case Telepathy.EventType.Connected:
                        OnClientConnected(new KNetConnection((short)msg.connectionId));
                        break;
                    case Telepathy.EventType.Data:
                        OnBytesRecieved(msg.data);
                        break;
                    case Telepathy.EventType.Disconnected:
                        //OnClientDisconnected();
                        break;
                }
            }
        }

        void Stop() {
            server.Stop();
            OnServerStopped();
        }


    }
}
