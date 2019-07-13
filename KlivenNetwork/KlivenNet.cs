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

        private static Dictionary<Type, IKNetBufferable> BufferableTypes = new Dictionary<Type, IKNetBufferable>();

        public static void RegisterBufferable<T>(IKNetBufferable bufferable) {
            BufferableTypes.Add(typeof(T), bufferable);
        }

        public static IKNetBufferable GetBufferable(Type type) {
            return BufferableTypes[type];
        }

        public static KNetZeroView DEBUG_ZERO_VIEW { get => zeroView; }


    }
}
