using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetClient {
        public static void RecieveBufferedObject(KNetSerializedField serializedField) {
            KNetView view = KlivenNet.FindView(serializedField.viewId);
            if (view == null) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: KNetView of id {serializedField.viewId}: view does not exist.");
                return;
            }
            /*var fields = view.BufferedFields.Where(e => e.Name == serializedField.fieldName).ToArray();
            if (fields.Length != 1) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: KNetView of id {serializedField.viewId}: field name {serializedField.fieldName} is ambiguous.");
                return;
            }*/
            //var field = fields[0];
            var field = serializedField.fieldId < view.BufferedFields.Length ? view.BufferedFields[serializedField.fieldId] : null;
            if (field == null) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: KNetView of id {serializedField.viewId}: field id {serializedField.fieldId} is not defined.");
                return;
            }
            var fieldType = field.FieldType;

            byte bufferable = KNetUtils.IsSerializable(fieldType);
            if (bufferable == 0) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: could not serialize field {field.Name} on KNetView {view.Id}: does {field.DeclaringType.Name} implement KNetSerializable interface?");
                return;
            }
            if (bufferable == 1) {
                MemoryStream ms = new MemoryStream(serializedField.data);
                BinaryFormatter bf = new BinaryFormatter();
                field.SetValue(view, bf.Deserialize(ms));
            } else if (bufferable == 2) {
                if (fieldType.IsArray || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))) {
                    MemoryStream ms = new MemoryStream(serializedField.data);
                    BinaryFormatter bf = new BinaryFormatter();
                    List<byte[]> serialized = (List<byte[]>)bf.Deserialize(ms);
                   
                    if (fieldType.IsArray) {
                        var elemType = fieldType.GetElementType();
                        var deserialized = Array.CreateInstance(elemType, serializedField.count);
                        int index = 0;
                        for(int i = 0; i < serializedField.count; i++) { 
                            object deserializedObject = typeof(IKNetSerializable).GetMethod("KNetDeserialize")
                                .Invoke(Activator.CreateInstance(elemType), new object[] { serialized[i] });

                            deserialized.SetValue(deserializedObject, index);
                            index++;
                        }
                        field.SetValue(view, deserialized);
                    } else {
                        var genArgType = fieldType.GetGenericArguments()[0];
                        var deserialized = Activator.CreateInstance(typeof(List<>).MakeGenericType(genArgType), serializedField.count);
                        var listType = deserialized.GetType();
                        for (int i = 0; i < serializedField.count; i++) {
                            object deserializedObject = typeof(IKNetSerializable).GetMethod("KNetDeserialize")
                                .Invoke(Activator.CreateInstance(genArgType), new object[] { serialized[i] });

                            listType.GetMethod("Add").Invoke(deserialized, new object[] { deserializedObject });
                        }
                        field.SetValue(view, deserialized);
                    }
                } else {
                    // IKNetSerializable
                    field.SetValue(view, typeof(IKNetSerializable).GetMethod("KNetDeserialize")
                            .Invoke(Activator.CreateInstance(fieldType), new object[] { serializedField.data }));
                }
            }
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

        protected virtual void OnConnectedToServer() {
            if (KlivenNet.IsConnected || KlivenNet.IsServer) {
                //KNetLogger.LogError("KlivenNetworking: Cannot start client instance, because other instance (server or client) is already running.");
                throw new Exception("KlivenNetworking: Cannot start client instance, because other instance (server or client) is already running on that application. Note that KlivenNetworking does not support Host mode (client & server at once)");
            }
            KNetLogger.Log("KlivenNetwroking: Connected to the server.");
        }
    }
}
