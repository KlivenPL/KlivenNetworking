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

        public static List<KNetSerializedField> DEBUG_SEND_BUFFERED_VALUES() {
            return SendBufferedValues();
        }
        internal static List<KNetSerializedField> SendBufferedValues() {
            List<KNetSerializedField> serializedFields = new List<KNetSerializedField>();
            foreach (var view in KlivenNet.Views) {
                foreach (var bufferedField in view.BufferedFields) {
                    var fieldType = bufferedField.FieldType;
                    byte bufferable = KNetUtils.IsSerializable(fieldType);
                   // Console.WriteLine(bufferedField.Name + " is bufferable: " + bufferable);
                   if (bufferable == 0) {
                        KNetLogger.LogError($"KNetServer: could not serialize field {bufferedField.Name} on KNetView {view.Id}: does {bufferedField.DeclaringType.Name} implement KNetSerializable interface?");
                        continue;
                    }
                    var fieldValue = bufferedField.GetValue(view);
                    if (fieldValue == null) {
                       // Console.WriteLine($"{fieldType} is null, not buffering.");
                        continue;
                    }
                    if (bufferable == 1) {
                        MemoryStream ms = new MemoryStream();
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(ms, fieldValue);
                        serializedFields.Add(new KNetSerializedField(view.Id, bufferedField.Name, ms.GetBuffer()));
                    } else if (bufferable == 2) {
                        byte[] buffer = null;
                        if (fieldType.IsArray || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))) {
                            List<byte[]> serialized = new List<byte[]>();
                            foreach (var element in (IEnumerable<IKNetSerializable>)fieldValue) {
                                if (element != null)
                                    serialized.Add(element.KNetSerialize());
                            }

                            MemoryStream ms = new MemoryStream();
                            BinaryFormatter bf = new BinaryFormatter();
                            bf.Serialize(ms, serialized);
                            buffer = ms.GetBuffer();
                            if (buffer != null && buffer.Length > 0) {
                                var sf = new KNetSerializedField(view.Id, bufferedField.Name, buffer);
                                sf.count = serialized.Count;
                                serializedFields.Add(sf);
                            }
                        } else {
                            /*buffer = (byte[])typeof(IKNetSerializable).GetMethod("KNetSerialize")
                                .Invoke(fieldValue, null); spedzilem nad tymi dwoma linijkami 17 godzin, a potem znalazlem latwijszy sposob :c*/
                            buffer = ((IKNetSerializable)fieldValue).KNetSerialize();
                            if (buffer != null && buffer.Length > 0)
                                serializedFields.Add(new KNetSerializedField(view.Id, bufferedField.Name, buffer));
                        }
                    }

                }
            }
            return serializedFields;
        }
    }

}
