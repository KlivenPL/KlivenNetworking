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
        public string Name { get; internal set; }

        public KNetPlayer(KNetConnection connection) {
            Connection = connection;
//            Name = name;
        }

        public KNetPlayer() {
            Connection = new KNetConnection();
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
            return KlivenNet.FindPlayer((int)bf.Deserialize(ms));
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
