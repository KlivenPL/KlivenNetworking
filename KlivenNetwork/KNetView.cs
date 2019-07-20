using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetView {
        public int Id { get; set; } = -1;
        [KNetBufferedObject]
        private int ownerConnId;
        public KNetConnection Owner { get => ownerConnId == 0 ? KlivenNet.ServerConnection : KlivenNet.FindPlayer(ownerConnId).Connection; }


        private FieldInfo[] bufferedFields = null;
        internal FieldInfo[] BufferedFields {
            get {
                if (bufferedFields != null)
                    return bufferedFields;
                return bufferedFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(e => e.IsDefined(typeof(KNetBufferedObjectAttribute), false)).
                    OrderBy(e => e.MetadataToken).ToArray();
            }
        }

        private MethodInfo[] rpcs = null;
        internal MethodInfo[] Rpcs {
            get {
                if (rpcs != null)
                    return rpcs;
                var tmp = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(e => e.IsDefined(typeof(KNetRPCAttribute), false)).
                    OrderBy(e => e.MetadataToken).ToArray();
                if (VerifyRpcMethods(tmp, out string methodName)) {
                    return rpcs = tmp;
                } else {
                    KNetLogger.LogError($"KlivenNetworking: KNetView: {GetType().Name}: incorrect RPC found: {methodName}. RPCs on that View will not work. Check if RPC parameters types are supported.");
                    return null;
                }
            }
        }

        private bool VerifyRpcMethods(MethodInfo[] rpcs, out string methodName) {
            methodName = "(empty)";
            foreach (var rpc in rpcs) {
                var args = rpc.GetParameters();
                methodName = rpc.Name;
                foreach (var arg in args) {
                    if (KNetUtils.IsSerializable(arg.GetType()) == 0) {
                        return false;
                    }
                }
            }
            return true;
        }

        public void RPC(string rpcMethodName, params object[] args) {

        }


        //public abstract KNetView CreateEmpty();

        protected virtual void OnServerUpdateSend(List<object> objectsToSend) {

        }

        protected virtual void OnServerUpdateRecieved(List<object> recievedObjects, KNetConnection sender) {

        }

        protected virtual void OnClientUpdateSend(List<object> objectsToSend) {

        }

        protected virtual void OnClientUpdateRecieved(List<object> recievedObjects, KNetConnection sender) {

        }

        internal void Init(int id) {
            Id = id;
            Console.WriteLine($"DEBUG: VIEW ID {id} HAS BEEN INITALIZED");
        }




        public void RequestSpawn() {

        }

        /// <summary>
        /// Launched locally when spawned.
        /// </summary>
        public virtual void OnSpawn() {
            KNetLogger.Log($"KNetView spawned with id {Id}");
        }
    }
}
