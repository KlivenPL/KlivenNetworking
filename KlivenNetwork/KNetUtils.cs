using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Returns 0 when buffType is not bufferable, 1 if it is a Primitive or a string and 2 if it implememnts IKNetSerializable
        /// </summary>
        public static byte IsSerializable(Type type/*, bool IKNetBufferableFound = false*/) {
            if (type.IsPrimitive || type == typeof(string))
                return /*IKNetBufferableFound ? (byte)2 :*/ (byte)1;
            if (type.IsArray) {
                return IsSerializable(type.GetElementType()/*, IKNetBufferableFound*/);
            }
            if (type.IsGenericType) {
                var genTypeDef = type.GetGenericTypeDefinition();
                if (genTypeDef == typeof(List<>)) {
                    //if (genTypeDef == typeof(IKNetBufferable<>))
                    //    IKNetBufferableFound = true;
                    var finalTypes = type.GetGenericArguments();
                    if (finalTypes.Length > 1)
                        return 0;
                    return IsSerializable(finalTypes[0]/*, IKNetBufferableFound*/);
                }
            }

            return (type.GetInterfaces()
                .Where(
                e => e == typeof(IKNetSerializable)
                ).Count() == 1) ? (byte)2 : (byte)0;
        }
        static short BytesToShort(params byte[] bytes) {
            return (short)((bytes[1] << 8) + bytes[0]);
        }

        static byte[] BytesFromShort(short number) {
            var byte2 = (byte)(number >> 8);
            var byte1 = (byte)(number & 255);
            return new byte[] { byte1, byte2 };
        }
        internal enum PacketType { welcome, bufferedObject, rpc,  }
        internal enum PacketDataType {bytes, IKNetSerializable}
        internal static byte[] ConstructPacket(PacketType packetType, IKNetSerializable obj, KNetConnection overrideSender = null){
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
