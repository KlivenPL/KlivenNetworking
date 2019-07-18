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
        public FieldInfo[] BufferedFields {
            get {
                if (bufferedFields != null)
                    return bufferedFields;
                return bufferedFields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(e => e.IsDefined(typeof(KNetBufferedObjectAttribute), false)).
                    OrderBy(e => e.MetadataToken).ToArray();
            }
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
