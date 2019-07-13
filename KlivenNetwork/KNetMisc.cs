using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public interface IKNetSerializable {
        byte[] KNetSerialize();
        T KNetDeserialize<T>(byte[] data);
    }

    public interface IKNetBufferable {
        byte[] KNetGetBuffer();
        T KNetGetObject<T>(byte[] data);
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class KNetBufferedValueAttribute : Attribute {

    }

    //public class Chujnia : KNetView {
    //    [KNetBufferedValue]
    //    public string ooops = "i chuj";
    //}
}
