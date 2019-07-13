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
