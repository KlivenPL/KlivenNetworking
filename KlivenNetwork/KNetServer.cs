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
            KlivenNet.Players.Add(new KNetPlayer(newClient));
            KlivenNet.ZeroView.Players[0].Name = "Jakis pajac xd";
            KlivenNet.Players.Add(new KNetPlayer(newClient));
            KlivenNet.ZeroView.Players[1].Name = "Jakis pajac2 xd";
            KlivenNet.Players.Add(new KNetPlayer(newClient));
            KlivenNet.ZeroView.Players[2].Name = "Jakis pajac3 xd";
            KlivenNet.Players.Add(new KNetPlayer(newClient));
            KlivenNet.ZeroView.Players[3].Name = "Jakis pajac4 xd";
            KlivenNet.Players.Add(new KNetPlayer(newClient));
            KlivenNet.ZeroView.Players[4].Name = "Jakis pajac5 xd";
            SendBufferedValues(newClient);
        }

        //public static List<KNetSerializedField> DEBUG_SEND_BUFFERED_VALUES() {
        //    return SendBufferedValues();
        //}
        internal void SendBufferedValues(KNetConnection newClient) {
            //List<KNetSerializedField> serializedFields = new List<KNetSerializedField>();
            foreach (var view in KlivenNet.Views) {
                foreach (var bufferedField in view.BufferedFields) {
                    KNetSerializedField serializedField = null;
                    var fieldType = bufferedField.FieldType;
                    byte bufferable = KNetUtils.IsSerializable(fieldType);
                   // Console.WriteLine(bufferedField.Name + " is bufferable: " + bufferable);
                   if (bufferable == 0) {
                        KNetLogger.LogError($"KNetServer: could not serialize field {bufferedField.Name} on KNetView {view.Id}: does {bufferedField.DeclaringType.Name} implement KNetSerializable interface?");
                        continue;
                    }
                    var fieldValue = bufferedField.GetValue(view);
                    if (fieldValue == null) {
                       // Console.WriteLine($"{fieldType} is null, not buffering.");
                        continue;
                    }
                    if (bufferable == 1) {
                        MemoryStream ms = new MemoryStream();
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, fieldValue);
                        serializedField = new KNetSerializedField(view.Id, bufferedField.Name, ms.GetBuffer());
                    } else if (bufferable == 2) {
                        byte[] buffer = null;
                        if (fieldType.IsArray || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))) {
                            List<byte[]> serialized = new List<byte[]>();
                            foreach (var element in (IEnumerable<IKNetSerializable>)fieldValue) {
                                if (element != null)
                                    serialized.Add(element.KNetSerialize());
                            }

                            MemoryStream ms = new MemoryStream();
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Serialize(ms, serialized);
                            buffer = ms.GetBuffer();
                            if (buffer != null && buffer.Length > 0) {
                                var sf = new KNetSerializedField(view.Id, bufferedField.Name, buffer);
                                sf.count = serialized.Count;
                                serializedField = sf;
                            }
                        } else {
                            /*buffer = (byte[])typeof(IKNetSerializable).GetMethod("KNetSerialize")
                                .Invoke(fieldValue, null); spedzilem nad tymi dwoma linijkami 17 godzin, a potem znalazlem latwijszy sposob :c*/
                            buffer = ((IKNetSerializable)fieldValue).KNetSerialize();
                            if (buffer != null && buffer.Length > 0)
                                serializedField = new KNetSerializedField(view.Id, bufferedField.Name, buffer);
                        }
                    }

                    var packet = KNetUtils.ConstructPacket(KNetUtils.PacketType.bufferedObject, serializedField);
                    SendBytes(newClient, packet);
                }
            }
        }
    }

}
