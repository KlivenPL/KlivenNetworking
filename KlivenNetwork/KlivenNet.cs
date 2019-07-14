using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking
{
    public static class KlivenNet {

        internal static List<KNetView> Views { get; } = new List<KNetView>();
        private static KNetZeroView zeroView = new KNetZeroView();
        public static List<KNetPlayer> Players {
            get {
                return zeroView.Players;
            }
        }

        public static KNetView FindView(int id) {
            for (int i = 0; i < Views.Count; i++) {
                if (Views[i].Id == id)
                    return Views[i];
            }
            KNetLogger.LogWarning($"No KNetView of id {id} was found.");
            return null;
        }

        public static int AddView(KNetView view) {
            int id = Views.Count;
            view.Init(id);
            Views.Add(view);
            return id;
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
        public static List<KNetView> DEBUG_VIEWS { get => Views; }


    }
}
