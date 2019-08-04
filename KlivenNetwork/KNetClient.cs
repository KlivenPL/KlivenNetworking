using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetClient {

        /// <summary>
        /// Sends bytes to the Server. Write your own code that manages that.
        /// </summary>
        /// <param name="target">Targeted Client</param>
        /// <param name="bytes">Data to send</param>
        protected abstract void SendBytes(byte[] bytes);
        internal void SendBytesInternal(byte[] bytes) {
            SendBytes(bytes);
        }
        internal static void RecieveBufferedObject(KNetSerializedField serializedField) {
            KNetView view = KlivenNet.FindView(serializedField.viewId);
            if (view == null) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: KNetView of id {serializedField.viewId}: view does not exist.");
                return;
            }
            var field = serializedField.fieldId < view.BufferedFields.Length ? view.BufferedFields[serializedField.fieldId] : null;
            if (field == null) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: KNetView of id {serializedField.viewId}: field id {serializedField.fieldId} is not defined.");
                return;
            }
            field.SetValue(view, KNetUtils.Deserialize(serializedField.data, serializedField.count
                , KNetUtils.IsSerializable(field.FieldType), field.FieldType));

        }

        /// <summary>
        /// Should be executed when bytes are recieved from the Server. You have to call it manually.
        /// </summary>
        /// <param name="bytes">Data recieved</param>
        public void OnBytesRecieved(byte[] bytes) {
            KNetUtils.PacketType packetType;
            KNetUtils.PacketDataType dataType;
            short senderId;
            KNetUtils.DeconstructPacket(ref bytes, out packetType, out dataType, out senderId);

            MemoryStream ms = new MemoryStream(bytes);
            BinaryFormatter bf = new BinaryFormatter();
            //KNetUtils.RecievePacket(packetType)
            switch (packetType) {
                case KNetUtils.PacketType.welcome:
                    RecieveWelcomePacket(bytes);
                    break;
                case KNetUtils.PacketType.bufferedObject:
                    RecieveBufferedObject((KNetSerializedField)bf.Deserialize(ms));
                    break;
                case KNetUtils.PacketType.rpc:
                    KNetRpc rpc = (KNetRpc)new KNetRpc().KNetDeserialize(bytes);
                    rpc.Execute();
                    break;
            }
        }

        internal void RecieveWelcomePacket(byte[] packet) {
            MemoryStream ms = new MemoryStream(packet);
            BinaryFormatter bf = new BinaryFormatter();
            object[] welcomePacket = (object[])bf.Deserialize(ms);

            string[] viewTypeNames = (string[])welcomePacket[0];
            int[] viewIds = (int[])welcomePacket[1];

            for (int i = 0; i < viewIds.Length; i++) {
                var view = (KNetView)Activator.CreateInstance(Type.GetType(viewTypeNames[i]));
                //view.Init(viewIds[i]);
                KlivenNet.AddView(view, viewIds[i]);
            }
            KlivenNet.ServerConnection = new KNetConnection(0);
            //KlivenNet.MyConnection = new KNetConnection((short)welcomePacket[0]);
        }

        internal void SendWelcomeClientInfo() {
            //object[] wci = new object[];
            //byte[] bytes = null;
            //using (MemoryStream ms = new MemoryStream()) {
            //    BinaryFormatter bf = new BinaryFormatter();
            //    bf.Serialize(ms, wci);
            //    bytes = ms.GetBuffer();
            //}
            KlivenNet.ZeroView.RPC()
            
        }

        protected virtual void OnConnectedToServer() {
            if (KlivenNet.IsConnected || KlivenNet.IsServer) {
                //KNetLogger.LogError("KlivenNetworking: Cannot start client instance, because other instance (server or client) is already running.");
                throw new Exception("KlivenNetworking: Cannot start client instance, because other instance (server or client) is already running on that application. Note that KlivenNetworking does not support Host mode (client & server at once)");
            }
            KlivenNet.ClientInstnace = this;
            KNetLogger.Log("KlivenNetwroking: Connected to the server.");
        }
    }
}
