using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetServer {
        public abstract void OnServerStart();
        public abstract void OnServerStopped();
        public abstract void OnClientConnected(KNetConnection connection);

        public static List<byte[]> SendBufferedValues() {
            List<byte[]> bytes = new List<byte[]>();
            /*for (int i = 0; i < KNetView.InharitedTypes.Length; i++) {
                FieldInfo[] fieldInfos = KNetView.InharitedTypes[i].GetFields().
                    Where(e => e.IsDefined(typeof(KNetBufferedValueAttribute), false)).
                    OrderBy(e=>e.MetadataToken).ToArray();
                for (int j = 0; j < fieldInfos.Length; j++) {
                    var xd = fieldInfos[j].GetValue(kNetView);
                }
            }*/

            /* for (int i = 0; i < KlivenNet.Views.Count; i++) {
                 for (int j = 0; j < KlivenNet.Views[i].BufferedFields.Length; j++) {
                     FieldInfo field = KlivenNet.Views[i].BufferedFields[j];
                     var value = field.GetValue(KlivenNet.Views[i]);
                     var t = field.FieldType;
                     var trueType = t;
                     bool isList = false;
                     if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) {
                         t = t.GetGenericArguments()[0];
                         isList = true;
                         //var interfaces = itemType.GetInterfaces()
                     }
                     var interfaces = t.GetInterfaces();
                     MemoryStream ms = new MemoryStream();
                     BinaryFormatter bf = new BinaryFormatter();
                     bool acceptedType = false;
                     for (int k = 0; k < interfaces.Length; k++) {
                         if (interfaces[k].IsGenericType && interfaces[k].GetGenericTypeDefinition() == typeof(IKNetBufferable<>)) {
                             acceptedType = true;
                             if (!isList) {
                                 Console.WriteLine("XDDDD");
                                 var ciota = trueType.GetMethod("KNetGetBuffer");
                                 var x2 = ciota.GetParameters();
                                 bytes.Add((byte[]) ciota.Invoke(Activator.CreateInstance(typeof(KNetPlayer),new object[] {null, "i chuja" } ), null));
                             }
                             break;
                         }
                     }
                     if (!acceptedType) {
                         acceptedType = t.IsPrimitive || t == typeof(string);
                     }
                     if (acceptedType) {
                         Console.WriteLine(value);
                     }
                     //if (t.inter t.IsPrimitive || t == typeof(Decimal) || t == typeof(String))
                     //  Console.WriteLine(xd);
                 }

             }*/

            foreach (var view in KlivenNet.Views) {
                foreach (var bufferedField in view.BufferedFields) {
                    // bool isBufferable = false;
                    byte bufferable = KNetUtils.IsBufferable(bufferedField.FieldType);
                    Console.WriteLine(bufferedField.Name + " is bufferable: " + bufferable);
                    if (bufferable == 1) {
                        MemoryStream ms = new MemoryStream();
                        BinaryFormatter bf = new BinaryFormatter();
                        var value = bufferedField.GetValue(view);
                        if (value != null) {
                            bf.Serialize(ms, bufferedField.GetValue(view));
                            bytes.Add(ms.GetBuffer());
                        }
                    }else if (bufferable == 2) {
                        var value = typeof(IKNetBufferable).GetMethod("KNetGetBuffer")
                            .Invoke(bufferedField.GetValue(null), null);

                    }

                }
            }
            return bytes;
        }
    }

}
