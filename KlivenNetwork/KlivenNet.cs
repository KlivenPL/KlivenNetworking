using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking
{
    public static class KlivenNet {

        internal static List<KNetView> Views { get; } = new List<KNetView>();
        internal static KNetZeroView ZeroView {
            get {
                return (KNetZeroView)Views[0];
            }
        }
        public static List<KNetPlayer> Players {
            get {
                return ZeroView.Players;
            }
        }

        internal static KNetServer ServerInstance { get; set; }
        internal static KNetClient ClientInstnace { get; set; }
        private static short nextPlayerId = 1;
        internal static short NextPlayerId {
            get {
                return nextPlayerId++;
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

        public static KNetPlayer FindPlayer(int connectionId) {
            for (int i = 0; i < Players.Count; i++) {
                if (Players[i].Connection.Id == connectionId)
                    return Players[i];
            }
            KNetLogger.LogWarning($"No KNetPlayer of connectionId = {connectionId} was found.");
            return null;
        }

        /// <summary>
        /// Checks if the server instance is running.
        /// </summary>
        public static bool IsServer {
            get {
                return IsConnected && MyConnection == ServerConnection;
            }
        }

        /// <summary>
        /// Checks if connected and in ready state. Use this whenever you want to check if connecting process has been successfully finished.
        /// </summary>
        public static bool IsConnectedAndReady {
            get {
                return IsConnected && ServerConnection != null;
            }
        }

        /// <summary>
        /// Checks only if connected. Use IsConnectedAndReady instead.
        /// </summary>
        public static bool IsConnected {
            get {
                return MyConnection != null;
            }
        }

        public static int AddView(KNetView view) {
            if (IsServer == false) {
                KNetLogger.LogError("KlivenNet.AddView() is only available on Server. On clients Views are synchronized automatically.");
                return -1;
            }
            int id = Views.Count;
            view.Init(id);
            Views.Add(view);
            return id;
        }

        internal static void AddView(KNetView view, int id) {
            view.Init(id);
            Views.Add(view);
        }

        public static KNetConnection MyConnection { internal set; get; }
        public static KNetConnection ServerConnection { internal set; get; }

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

       // public static KNetZeroView DEBUG_ZERO_VIEW { get => zeroView; }
        public static List<KNetView> DEBUG_VIEWS { get => Views; }


    }
}
