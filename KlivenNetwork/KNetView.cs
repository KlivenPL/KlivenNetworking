using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetView {
        public int Id { get; set; } = -1;

        //public static KNetView Find(int id) {
        //    for (int i = 0; i < KlivenNet.Views.Count; i++) {
        //        if (KlivenNet.Views[i].Id == id)
        //            return KlivenNet.Views[i];
        //    }
        //    KNetLogger.LogWarning($"No KNetView of id = {id} was found");
        //    return null;
        //}

        private static Type[] inharitedTypes = null;
        public static Type[] InharitedTypes {
            get {
                if (inharitedTypes != null)
                    return inharitedTypes;
                return inharitedTypes = KNetUtils.GetEnumerableOfType<KNetView>().OrderBy(e=>e.MetadataToken).ToArray();
            }
        }

        private FieldInfo[] bufferedFields = null;
        public FieldInfo[] BufferedFields {
            get {
                if (bufferedFields != null)
                    return bufferedFields;
                return bufferedFields = GetType().GetFields().Where(e => e.IsDefined(typeof(KNetBufferedObjectAttribute), false)).
                    OrderBy(e => e.MetadataToken).ToArray();
            }
        }

        internal void Init(int id) {
            Id = id;
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
