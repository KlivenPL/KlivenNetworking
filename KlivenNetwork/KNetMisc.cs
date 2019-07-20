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

    internal class KNetRpcInfo {
        public MethodInfo methodInfo;
        public string Name { get; private set; }
        public Type[] ArgumentsTypes { get; private set; }

        public KNetRpcInfo(MethodInfo methodInfo) {

            this.methodInfo = methodInfo;
            Name = methodInfo.Name;
            ArgumentsTypes = methodInfo.GetParameters().Select(e => e.ParameterType).ToArray();
        }
    }
}

