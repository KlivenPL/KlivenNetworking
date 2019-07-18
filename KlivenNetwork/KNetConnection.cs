using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
namespace KlivenNetworking {
    [Serializable]
    public class KNetConnection : IKNetReferenceable, ISerializable {
        public IPEndPoint Ip { get; private set; } = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 0);
        public short Id { get; private set; } = -1;

        public object KNetDeserializeReference(byte[] data) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(data);
            return bf.Deserialize(ms);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("ip", Ip.Address.ToString(), typeof(string));
            info.AddValue("port", Ip.Port, typeof(Int32));
            info.AddValue("id", Id, typeof(Int16));
        }
        public KNetConnection(SerializationInfo info, StreamingContext context) {
            Ip = new IPEndPoint(IPAddress.Parse(info.GetString("ip")), info.GetInt32("port"));
            Id = info.GetInt16("id");
        }

        public KNetConnection(short connId) {
            Id = connId;
        }

        public byte[] KNetSerializeReference() {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            return ms.GetBuffer();
        }

        public KNetConnection() { }
    }
}
