using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public interface IKNetReferenceable {
        byte[] KNetSerializeReference();
        object KNetDeserializeReference(byte[] data);
    }

    public interface IKNetSerializable {
        byte[] KNetSerialize();
        object KNetDeserialize(byte[] data);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class KNetBufferedObjectAttribute : Attribute {

    }
    //TODO CHANGE THAT TO INTERNAL!
    public class KNetSerializedField {
        public int viewId, count = 1;
        public string fieldName;
        public byte[] data;

        //TODO CHANGE THAT TO INTERNAL!
        public KNetSerializedField(int viewId, string fieldName, byte[] data) {
            this.viewId = viewId;
            this.fieldName = fieldName;
            this.data = data;
        }
    }
}
