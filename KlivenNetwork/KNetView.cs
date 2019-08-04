using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetView : IKNetReferenceable {
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

        private KNetRpcInfo[] rpcs = null;
        internal KNetRpcInfo[] Rpcs {
            get {
                if (rpcs != null)
                    return rpcs;
                var tmp = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(e => e.IsDefined(typeof(KNetRPCAttribute), false)).
                    OrderBy(e => e.MetadataToken).ToArray();

                if (VerifyRpcMethods(tmp, out rpcs))
                    return rpcs;

                return null;
            }
        }
        internal bool VerifyRpcMethods(MethodInfo[] tmp, out KNetRpcInfo[] tmpRpcs) {
            tmpRpcs = new KNetRpcInfo[tmp.Length];
            for (int i = 0; i < tmp.Length; i++) {
                var rpcInfo = KNetRpcInfo.CreateRpcInfo((short)i, this, tmp[i]);
                if (rpcInfo == null) {
                    KNetLogger.LogError($"KlivenNetworking: KNetView: {GetType().Name}: incorrect RPC found: {tmp[i].Name}. RPCs on that View will not work. Check if RPC parameters types are supported.");
                    return false;
                }
                tmpRpcs[i] = rpcInfo;
            }
            for (int i = 0; i < tmpRpcs.Length; i++) {
                for (int j = 0; j < tmpRpcs.Length; j++) {
                    if (tmpRpcs[i].Name == tmpRpcs[j].Name && i != j) {
                        KNetLogger.LogError($"KlivenNetworking: KNetView: {GetType().Name}: duplicate RPC name found: {tmp[i].Name}. RPCs on that View will not work. Make sure that all RPCs names in View are unique.");
                        return false;
                    }
                }
            }
            return true;
        }

        public void RPC(string rpcMethodName, KNetConnection[] targets, params object[] args) {
            var rpcInfo = FindRpc(rpcMethodName);
            if (rpcInfo == null) {
                KNetLogger.LogError($"There is no RPC {rpcMethodName} on View of ID {Id} tagged with KNetRPC attribute.");
            }
            if (VerifyRpcArguments(rpcInfo, args) == false) {
                return;
            }
            KNetRpc rpc = new KNetRpc(rpcInfo, args);
            var packet = KNetUtils.ConstructPacket(KNetUtils.PacketType.rpc, rpc.KNetSerialize(), KlivenNet.MyConnection);

            if (KlivenNet.IsServer) {
                if (targets == null) {
                    KNetLogger.LogError($"KNetView: {Id}, RPC:{rpcMethodName}: 'Targets' parameter can not be null on the Server. Use KNetTargets.all to send it to all Clients.");
                    return;
                }
                for (int i = 0; i < targets.Length; i++) {
                    if (targets[i] == KlivenNet.MyConnection)
                        rpc.Execute();
                    else
                        KlivenNet.ServerInstance.SendBytesInternal(targets[i], packet);
                }
            } else {
                if (targets != null) {
                    if (targets[0] == KlivenNet.MyConnection) {
                        rpc.Execute();
                        return;
                    }
                    KNetLogger.LogError($"KNetView: {Id}, RPC:{rpcMethodName}: Only Server can send RPCs to other Clients. Clients can send RPCs to the Server only. Leave 'targets' parameter null or use KNetTargets.server");
                    return;
                }
                KlivenNet.ClientInstnace.SendBytesInternal(packet);
            }
        }

        public void RPC(string rpcMethodName, KNetTargets target, params object[] args) {
            KNetConnection[] targets = null;
            switch (target) {
                case KNetTargets.me:
                    targets = new KNetConnection[] { KlivenNet.MyConnection };
                    break;
                case KNetTargets.server:
                    targets = new KNetConnection[] { KlivenNet.ServerConnection };
                    break;
                case KNetTargets.allClients:
                    targets = KlivenNet.Players.Select(p => p.Connection).ToArray();
                    break;
                case KNetTargets.allClientsAndServer:
                    targets = new KNetConnection[KlivenNet.Players.Count + 1];
                    targets[0] = KlivenNet.ServerConnection;
                    for (int i = 0; i < KlivenNet.Players.Count; i++) {
                        targets[i + 1] = KlivenNet.Players[i].Connection;
                    }
                    break;
            }
            RPC(rpcMethodName, targets, args);
        }

        internal KNetRpcInfo FindRpc(string name) {
            if (Rpcs == null)
                return null;
            for (int i = 0; i < Rpcs.Length; i++) {
                if (Rpcs[i].Name == name)
                    return Rpcs[i];
            }
            return null;
        }

        internal bool VerifyRpcArguments(KNetRpcInfo rpcInfo, params object[] args) {
            if (args.Length != rpcInfo.ArgumentsTypes.Length) {
                return false;
            }
            for (int i = 0; i < args.Length; i++) {
                if (args[i].GetType() != rpcInfo.ArgumentsInfo[i].ParameterType) {
                    KNetLogger.LogError($"KlivenNetworking: KNetView: {GetType().Name}, ID: {Id}: RPC {rpcInfo.Name} incorrect parameters: expected {string.Join(", ", rpcInfo.ArgumentsTypes.Select(e=>e.Name).ToArray())}, recieved {string.Join(", ", args.Select(e=>e.GetType().Name).ToArray())}");
                    return false;
                }
            }
            return true;
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

        public byte[] KNetSerializeReference() {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, Id);
            return ms.GetBuffer();
        }

        public object KNetDeserializeReference(byte[] data) {
            MemoryStream ms = new MemoryStream(data);
            BinaryFormatter bf = new BinaryFormatter();
            var id = (int)bf.Deserialize(ms);
            return KlivenNet.FindView(id);
        }
    }
}
