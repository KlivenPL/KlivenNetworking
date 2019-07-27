using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LZ4;

namespace KlivenNetworking {
    public static class KNetUtils {

        public static IEnumerable<Type> GetEnumerableOfType<T>(params object[] constructorArgs) where T : class {
            List<T> objects = new List<T>();
            return Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));
        }

        internal static SerializableType IsSerializable(Type type) {
            if (type.IsPrimitive || type == typeof(string))
                return SerializableType.primitive;
            if (type.IsArray) {
                var elemenType = type.GetElementType();
                if (elemenType.IsGenericType)
                    return 0;
                if (elemenType.IsArray)
                    return 0;
                var serializable = IsSerializable(elemenType);
                if (serializable == SerializableType.kNetSerializable)
                    return SerializableType.array;
                else
                    return serializable;
            }
            if (type.IsGenericType) {
                var genTypeDef = type.GetGenericTypeDefinition();
                if (genTypeDef == typeof(List<>)) {
                    var finalTypes = type.GetGenericArguments();
                    if (finalTypes.Length > 1)
                        return SerializableType.nonSerializable;
                    if (finalTypes[0].IsGenericType)
                        return SerializableType.nonSerializable;
                    if (finalTypes[0].IsArray)
                        return SerializableType.nonSerializable;

                    var serializable = IsSerializable(finalTypes[0]);
                    if (serializable == SerializableType.kNetSerializable)
                        return SerializableType.list;
                    else
                        return serializable;
                }
                return 0;
            }

            return (type.GetInterfaces()
                .Where(
                e => e == typeof(IKNetSerializable)
                ).Count() == 1) ? SerializableType.kNetSerializable : SerializableType.nonSerializable;
        }

        internal static byte[] Serialize(object fieldValue, SerializableType serializableType, out int count) {
            count = 1;
            var bufferable = (int)serializableType;
            if (bufferable == 0) {
                KNetLogger.LogWarning($"KNetUtils: could not serialize {fieldValue} : Non supported type");
                return null;
            }
            if (fieldValue == null) {
                // Console.WriteLine($"{fieldType} is null, not buffering.");
                return null;
            }
            byte[] buffer = null;
            if (bufferable == 1) {
                MemoryStream ms = new MemoryStream();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, fieldValue);
                return ms.GetBuffer();
            } else if (bufferable == 2) {
                buffer = ((IKNetSerializable)fieldValue).KNetSerialize();
                if (buffer != null && buffer.Length > 0)
                    return buffer;
            } else if (bufferable == 3 || bufferable == 4) {
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
                    count = serialized.Count;
                    return buffer;
                }
            }
            return null;
        }

        static short BytesToShort(params byte[] bytes) {
            return (short)((bytes[1] << 8) + bytes[0]);
        }

        static byte[] BytesFromShort(short number) {
            var byte2 = (byte)(number >> 8);
            var byte1 = (byte)(number & 255);
            return new byte[] { byte1, byte2 };
        }
        internal enum PacketType { welcome, bufferedObject, rpc, }
        internal enum PacketDataType { bytes, IKNetSerializable }
        internal static byte[] ConstructPacket(PacketType packetType, IKNetSerializable obj, KNetConnection overrideSender = null) {
            var connIdBytes = BytesFromShort(overrideSender != null && KlivenNet.IsServer ? overrideSender.Id : KlivenNet.MyConnection.Id);
            return LZ4Codec.Wrap(new byte[] { (byte)packetType, (byte)PacketDataType.IKNetSerializable, connIdBytes[0], connIdBytes[1] }.Concat(obj.KNetSerialize()).ToArray());
        }
        internal static byte[] ConstructPacket(PacketType packetType, byte[] bytes, KNetConnection overrideSender = null) {
            var connIdBytes = BytesFromShort(overrideSender != null && KlivenNet.IsServer ? overrideSender.Id : KlivenNet.MyConnection.Id);
            return LZ4Codec.Wrap(new byte[] { (byte)packetType, (byte)PacketDataType.bytes, connIdBytes[0], connIdBytes[1] }.Concat(bytes).ToArray());
        }

        internal static void DeconstructPacket(ref byte[] bytes, out PacketType packetType, out PacketDataType dataType, out short senderId) {
            bytes = LZ4Codec.Unwrap(bytes);
            MemoryStream ms = new MemoryStream(bytes);
            packetType = (PacketType)bytes[0];
            dataType = (PacketDataType)bytes[1];
            senderId = BytesToShort(bytes[2], bytes[3]);
            bytes = bytes.Skip(4).ToArray();
        }
    }
}
