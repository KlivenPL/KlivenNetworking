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
    public interface IKNetReferenceable {
        byte[] KNetSerializeReference();
        object KNetDeserializeReference(byte[] data);
    }

    public interface IKNetSerializable {
        byte[] KNetSerialize();
        object KNetDeserialize(byte[] data);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class KNetBufferedObjectAttribute : Attribute {

    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class KNetRPCAttribute : Attribute {

    }

    //TODO CHANGE THAT TO INTERNAL!
    [Serializable]
    internal class KNetSerializedField : IKNetSerializable {
        public int viewId, fieldId, count = 1;
        public byte[] data;

        //TODO CHANGE THAT TO INTERNAL!
        public KNetSerializedField(int viewId, int fieldId, byte[] data) {
            this.viewId = viewId;
            this.fieldId = fieldId;
            this.data = data;
        }

        public object KNetDeserialize(byte[] data) {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();
            return bf.Deserialize(ms);
        }

        public byte[] KNetSerialize() {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, this);
            return ms.GetBuffer();
        }
    }

    internal class KNetRpcInfo : IKNetReferenceable {
        public MethodInfo methodInfo;
        public short Id { get; private set; } = -1;
        public KNetView kNetView { get; private set; }
        public string Name { get; private set; }
        public ParameterInfo[] Arguments { get; private set; }
        public Type[] ArgumentsTypes { get; private set; }

        public static KNetRpcInfo CreateRpcInfo (short id, KNetView view, MethodInfo methodInfo) {
            KNetRpcInfo info = new KNetRpcInfo();
            var args = methodInfo.GetParameters().OrderBy(e=>e.Position);
            if (args.Select (e => KNetUtils.IsSerializable(e.ParameterType) != 0).Any()) {
                return null;
            }
            info.methodInfo = methodInfo;
            info.Name = methodInfo.Name;
            info.Arguments = args.ToArray();
            info.ArgumentsTypes = args.Select(e => e.ParameterType).ToArray();
            info.Id = id;
            info.kNetView = view;
            return info;
        }

        public object KNetDeserializeReference(byte[] data) {
            using (MemoryStream ms = new MemoryStream(data)) {
                BinaryFormatter bf = new BinaryFormatter();
                var refer = bf.Deserialize(ms);

            }

        }

        public byte[] KNetSerializeReference() {
            byte[] refer = null;
            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                object[] data = new object[2];
                data[0] = kNetView.KNetSerializeReference();
                data[1] = Id;
                //TODO: SERIALIZOWAC ARGUMENTY KURWA
                bf.Serialize(ms, data);
                refer = ms.GetBuffer();
            }
            return refer;
        }
    }
}

