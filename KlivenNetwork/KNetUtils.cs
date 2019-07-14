using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
    }
}
