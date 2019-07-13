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
    public class KNetConnection : IKNetSerializable<KNetConnection>, ISerializable { 
        public IPEndPoint Ip { get; private set; }
        public int Id { get; private set; }

        public KNetConnection KNetDeserialize(byte[] data) {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(data);
            return (KNetConnection)bf.Deserialize(ms);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("ip", Ip.Address.ToString(), typeof(string));
            info.AddValue("port", Ip.Port, typeof(Int32));
            info.AddValue("id", Id, typeof(Int32));
        }
        public KNetConnection(SerializationInfo info, StreamingContext context) {
            Ip = new IPEndPoint(IPAddress.Parse(info.GetString("ip")), info.GetInt32("port"));
            Id = info.GetInt32("id");
        }

        public KNetConnection() { }

        public byte[] KNetSerialize() {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            return ms.GetBuffer();
        }
    }
}
