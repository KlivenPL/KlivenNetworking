using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace KlivenNetworking {
    [Serializable]
    public class KNetPlayer : IKNetReferenceable, IKNetSerializable, ISerializable {
        public KNetConnection Connection { get; private set; }
        public string Name { get; private set; }

        public KNetPlayer(KNetConnection connection, string name) {
            Connection = connection;
            Name = name;
        }

        public KNetPlayer() { }
        public static KNetPlayer Find(int connectionId) {
            for (int i = 0; i < KlivenNet.Players.Count; i++) {
                if (KlivenNet.Players[i].Connection.Id == connectionId)
                    return KlivenNet.Players[i];
            }
            KNetLogger.LogWarning($"No KNetPlayer of connectionId = {connectionId} was found.");
            return null;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("connection", Connection);
            info.AddValue("name", Name);
        }

        public KNetPlayer(SerializationInfo info, StreamingContext context) {
            Connection = (KNetConnection)info.GetValue("connection", typeof(KNetConnection));
            Name = (string)info.GetValue("name", typeof(string));
        }

        public object KNetDeserializeReference(byte[] data) {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();
            return Find((int)bf.Deserialize(ms));
        }

        public byte[] KNetSerializeReference() {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, Connection.Id);
            return ms.GetBuffer();
        }

        public byte[] KNetSerialize() {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            return ms.GetBuffer();
        }

        public object KNetDeserialize(byte[] data) {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();
            return bf.Deserialize(ms);
        }

    }
}
