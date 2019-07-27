using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetServer {
        /// <summary>
        /// Should be executed when Server starts. You have to call it manually.
        /// </summary>
        protected virtual void OnServerStarted() {
            if (KlivenNet.IsConnected || KlivenNet.IsServer) {
                throw new Exception("KlivenNetworking: Cannot start server instance, because other instance (server or client) is already running on that application. Note that KlivenNetworking does not support Host mode (client & server at once)");
            }
            KlivenNet.MyConnection = new KNetConnection(0);
            KlivenNet.ServerConnection = KlivenNet.MyConnection;
            KlivenNet.AddView(new KNetZeroView());
        }
        /// <summary>
        /// (Optional) Should be executed when Server stops (or just before). You have to call it manually. Without overriding it does nothing.
        /// </summary>
        protected virtual void OnServerStopped() {

        }
        protected virtual void OnClientConnected(KNetConnection newClient) {
            // we have to send welcome packet to the client.
            SendWelcomePacket(newClient);
        }
        /// <summary>
        /// Sends bytes to targeted Client. Write your own code that manages that.
        /// </summary>
        /// <param name="target">Targeted Client</param>
        /// <param name="bytes">Data to send</param>
        protected abstract void SendBytes(KNetConnection target, byte[] bytes);
        /// <summary>
        /// Should be executed when bytes are recieved from any client. You have to call it manually.
        /// </summary>
        /// <param name="bytes">Data recieved</param>
        protected virtual void OnBytesRecieved(byte[] bytes) {
            KNetUtils.DeconstructPacket(ref bytes, out KNetUtils.PacketType packetType, out KNetUtils.PacketDataType dataType, out short senderId);
            //KNetUtils.RecievePacket(packetType)
            switch (packetType) {
                case KNetUtils.PacketType.rpc:
                    break;
            }
        }

        internal void SendWelcomePacket(KNetConnection newClient) {
            object[] welcomePacket = new object[2];
            string[] viewTypeNames = new string[KlivenNet.Views.Count];
            int[] viewIds = new int[KlivenNet.Views.Count];
            // byte[][] viewOwnersData = new byte[KlivenNet.Views.Count][];
            for (int i = 0; i < KlivenNet.Views.Count; i++) {
                var view = KlivenNet.Views[i];
                viewTypeNames[i] = view.GetType().FullName;
                viewIds[i] = view.Id;
                // viewOwnersData[i] = view.Owner.KNetSerializeReference();
            }
            //  welcomePacket[0] = KlivenNet.NextPlayerId;
            welcomePacket[0] = viewTypeNames;
            welcomePacket[1] = viewIds;
            // welcomePacket[2] = viewOwnersData;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, welcomePacket);
            var packet = KNetUtils.ConstructPacket(KNetUtils.PacketType.welcome, ms.GetBuffer());
            SendBytes(newClient, packet);
            KlivenNet.Players.Add(new KNetPlayer { Name = "dupa 1" });
            KlivenNet.Players.Add(new KNetPlayer { Name = "dupa 2" });
            KlivenNet.Players.Add(new KNetPlayer { Name = "dupa 3" });
            KlivenNet.Players.Add(new KNetPlayer { Name = "dupa 4" });
            KlivenNet.Players.Add(new KNetPlayer { Name = "dupa 5" });
            SendBufferedValues(newClient);
        }

        //public static List<KNetSerializedField> DEBUG_SEND_BUFFERED_VALUES() {
        //    return SendBufferedValues();
        //}
        internal void SendBufferedValues(KNetConnection newClient) {
            //List<KNetSerializedField> serializedFields = new List<KNetSerializedField>();
            foreach (var view in KlivenNet.Views) {
                int buffFieldId = -1;
                foreach (var bufferedField in view.BufferedFields) {
                    buffFieldId++;
                    var fieldType = bufferedField.FieldType;
                    var bufferable = KNetUtils.IsSerializable(fieldType);
                    if (bufferable == SerializableType.nonSerializable) {
                        KNetLogger.LogError($"KNetServer: could not serialize field {bufferedField.Name} on KNetView {view.Id}: does {bufferedField.DeclaringType.Name} implement KNetSerializable interface?");
                        continue;
                    }
                    // Console.WriteLine(bufferedField.Name + " is bufferable: " + bufferable);
                    var bytes = KNetUtils.Serialize(bufferedField.GetValue(view), bufferable, out int count);
                    if (bytes == null) {
                        continue;
                    }
                    var serializedField = new KNetSerializedField(view.Id, buffFieldId, bytes);
                    serializedField.count = count;
                    var packet = KNetUtils.ConstructPacket(KNetUtils.PacketType.bufferedObject, serializedField);
                    SendBytes(newClient, packet);
                }
            }
        }
    }

}
