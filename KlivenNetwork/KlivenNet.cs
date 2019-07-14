using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking
{
    public static class KlivenNet {

        public static List<KNetView> Views { get; private set; } = new List<KNetView>();
        private static KNetZeroView zeroView = new KNetZeroView();
        public static List<KNetPlayer> Players {
            get {
                return zeroView.Players;
            }
        }

        //private static Dictionary<Type, IKNetBufferable> BufferableTypes = new Dictionary<Type, IKNetBufferable>();

        //public static void RegisterBufferable<T>() where T :  new() {
        //    if (BufferableTypes.ContainsKey(typeof(T)) == false)
        //        BufferableTypes.Add(typeof(T), (IKNetBufferable)(object)new T());
        //}

        //public static T GetBufferable<T>() where T :  new() {
        //    if (BufferableTypes.ContainsKey(typeof(T)) == false)
        //        BufferableTypes.Add(typeof(T), (IKNetBufferable)(object)new T());
        //    return (T)BufferableTypes[typeof(T)];
        //}

        public static KNetZeroView DEBUG_ZERO_VIEW { get => zeroView; }


    }
}
