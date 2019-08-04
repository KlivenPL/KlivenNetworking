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

    internal enum SerializableType {
        nonSerializable,
        primitive,
        kNetSerializable,
        array,
        list
    }

    public enum KNetTargets {
        me,
        server,
        allClients,
        allClientsAndServer,
    }

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
        public MethodInfo methodInfo { get; private set; }
        public short Id { get; private set; } = -1;
        public KNetView kNetView { get; private set; }
        // public object[] Arguments { get; private set; }
        public string Name { get; private set; }
        public ParameterInfo[] ArgumentsInfo { get; private set; }
        public Type[] ArgumentsTypes { get; private set; }
        public SerializableType[] ArgSerializationTypes { get; private set; }

        public static KNetRpcInfo CreateRpcInfo(short id, KNetView view, MethodInfo methodInfo) {
            KNetRpcInfo info = new KNetRpcInfo();
            var args = methodInfo.GetParameters().OrderBy(e => e.Position);
            if (args.Where(e => KNetUtils.IsSerializable(e.ParameterType) == SerializableType.nonSerializable).Any()) {
                return null;
            }
            info.methodInfo = methodInfo;
            info.Name = methodInfo.Name;
            info.ArgumentsInfo = args.ToArray();
            info.ArgumentsTypes = new Type[info.ArgumentsInfo.Length];
            info.ArgSerializationTypes = new SerializableType[info.ArgumentsInfo.Length];
            for (int i = 0; i < info.ArgumentsInfo.Length; i++) {
                info.ArgumentsTypes[i] = info.ArgumentsInfo[i].ParameterType;
                info.ArgSerializationTypes[i] = KNetUtils.IsSerializable(info.ArgumentsTypes[i]);
            }
            info.Id = id;
            info.kNetView = view;
            return info;
        }

        public object KNetDeserializeReference(byte[] data) {
            KNetRpcInfo res = null;
            using (MemoryStream ms = new MemoryStream(data)) {
                BinaryFormatter bf = new BinaryFormatter();
                object[] refer = (object[])bf.Deserialize(ms);
                var kNetView = (KNetView)KlivenNet.ZeroView.KNetDeserializeReference((byte[])refer[0]);
                res = kNetView.Rpcs[(short)refer[1]];
            }
            return res;

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

    [Serializable]
    internal class KNetRpc : IKNetSerializable, ISerializable{
        public KNetRpcInfo RpcInfo { get; private set; }
        public object[] Arguments { get; private set; }
        public int[] Counts { get; private set; }

        public void Execute() {
            if (RpcInfo == null) {
                KNetLogger.LogError("KNetRpc: Cound not execute: RpcInfo is null.");
            }
            RpcInfo.methodInfo.Invoke(RpcInfo.kNetView, Arguments);
        }

        public KNetRpc() { }

        public KNetRpc(KNetRpcInfo rpcInfo, params object[] arguments) {
            this.RpcInfo = rpcInfo;
            this.Arguments = arguments;
            this.Counts = new int[arguments.Length];
        }

        public object KNetDeserialize(byte[] data) {
            object res = null;
            using (MemoryStream ms = new MemoryStream(data)) {
                BinaryFormatter bf = new BinaryFormatter();
                res = bf.Deserialize(ms);
            }
            return res;

        }

        public byte[] KNetSerialize() {
            byte[] res = null;
            using (MemoryStream ms = new MemoryStream()) {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                res = ms.GetBuffer();
            }
            return res;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            info.AddValue("counts", Counts);
            info.AddValue("rpcInfo", RpcInfo.KNetSerializeReference());
            byte[][] serializedArgs = new byte[Arguments.Length][];
            for (int i = 0; i < serializedArgs.Length; i++) {
                serializedArgs[i] = KNetUtils.Serialize(Arguments[i], RpcInfo.ArgSerializationTypes[i], out Counts[i]);
            }
            info.AddValue("args", serializedArgs);
        }

        public KNetRpc(SerializationInfo info, StreamingContext context) {
            Counts = (int[])info.GetValue("counts", typeof(int[]));
            RpcInfo = (KNetRpcInfo)new KNetRpcInfo().KNetDeserializeReference((byte[])info.GetValue("rpcInfo", typeof(byte[])));
            Arguments = new object[RpcInfo.ArgumentsTypes.Length];

            byte[][] serializedArgs = (byte[][])info.GetValue("args", typeof(byte[][]));
            for (int i = 0; i < serializedArgs.Length; i++) {
                Arguments[i] = KNetUtils.Deserialize(serializedArgs[i], Counts[i], RpcInfo.ArgSerializationTypes[i], RpcInfo.ArgumentsTypes[i]);
            }
        }
    }
}

