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
        /// Returns 0 when buffType is not bufferable, 1 if it is a Primitive or a string and 2 if it implememnts IKNetBufferable<> and is serializable
        /// </summary>
        public static byte IsBufferable(Type buffType, bool IKNetBufferableFound = false) {
            if (buffType.IsPrimitive || buffType == typeof(string))
                return IKNetBufferableFound ? (byte)2 : (byte)1;
            if (buffType.IsArray) {
                return IsBufferable(buffType.GetElementType(), IKNetBufferableFound);
            }
            if (buffType.IsGenericType) {
                var genTypeDef = buffType.GetGenericTypeDefinition();
                if (genTypeDef == typeof(List<>)) {
                    //if (genTypeDef == typeof(IKNetBufferable<>))
                    //    IKNetBufferableFound = true;
                    var finalTypes = buffType.GetGenericArguments();
                    if (finalTypes.Length > 1)
                        return 0;
                    return IsBufferable(finalTypes[0], IKNetBufferableFound);
                }
            }

            return (buffType.GetInterfaces()
                .Where(
                e => e == typeof(IKNetBufferable)
                ).Count() == 1) ? (byte)2 : (byte)0;
        }
    }

   
    //public static T Cast<T>(object o) {
    //    return (T)o;
    //}

    //private static MethodInfo castMethodInfo {
    //    get {
    //        return this.GetType().GetMethod("Cast").MakeGenericMethod(t);
    //        object castedObject = castMethod.Invoke(null, new object[] { obj });
    //    }
}
