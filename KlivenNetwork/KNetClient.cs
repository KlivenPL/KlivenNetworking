using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KlivenNetworking {
    public abstract class KNetClient {
        public static void RecieveSerializedObject(KNetSerializedField serializedField) {
            KNetView view = KlivenNet.FindView(serializedField.viewId);
            if (view == null) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: KNetView of id {serializedField.viewId}: view does not exist.");
                return;
            }
            var fields = view.BufferedFields.Where(e => e.Name == serializedField.fieldName).ToArray();
            if (fields.Length != 1) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: KNetView of id {serializedField.viewId}: field name {serializedField.fieldName} is ambiguous.");
                return;
            }
            var field = fields[0];
            var fieldType = field.FieldType;

            /* if (fieldType.IsPrimitive || fieldType == typeof(string)) {

             }else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>)) {
                 if ()
             }else if (fieldType.GetInterfaces().Where(e=>e == typeof(IKNetSerializable)).Any()) {

             }*/

            byte bufferable = KNetUtils.IsSerializable(fieldType);
            if (bufferable == 0) {
                KNetLogger.LogError($"KNetClient: could not Recieve buffered object: could not serialize field {field.Name} on KNetView {view.Id}: does {field.DeclaringType.Name} implement KNetSerializable interface?");
                return;
            }
            if (bufferable == 1) {
                MemoryStream ms = new MemoryStream(serializedField.data);
                BinaryFormatter bf = new BinaryFormatter();
                field.SetValue(view, bf.Deserialize(ms));
            } else if (bufferable == 2) {
                if (fieldType.IsArray || (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))) {
                    MemoryStream ms = new MemoryStream(serializedField.data);
                    BinaryFormatter bf = new BinaryFormatter();
                    List<byte[]> serialized = (List<byte[]>)bf.Deserialize(ms);
                   
                    if (fieldType.IsArray) {
                        var elemType = fieldType.GetElementType();
                        var deserialized = Array.CreateInstance(elemType, serializedField.count);
                        int index = 0;
                        foreach (var element in serialized) {
                            object deserializedObject = typeof(IKNetSerializable).GetMethod("KNetDeserialize")
                                .Invoke(Activator.CreateInstance(elemType), new object[] { element });

                            deserialized.SetValue(deserializedObject, index);
                            index++;
                        }
                        field.SetValue(view, deserialized);
                    } else {
                        var genArgType = fieldType.GetGenericArguments()[0];
                        var deserialized = Activator.CreateInstance(typeof(List<>).MakeGenericType(genArgType), serializedField.count);
                        var listType = deserialized.GetType();
                        foreach (var element in serialized) {
                            object deserializedObject = typeof(IKNetSerializable).GetMethod("KNetDeserialize")
                                .Invoke(Activator.CreateInstance(genArgType), new object[] { element });

                            listType.GetMethod("Add").Invoke(deserialized, new object[] { deserializedObject });
                        }
                        field.SetValue(view, deserialized);
                    }
                } else {
                    // IKNetSerializable
                    field.SetValue(view, typeof(IKNetSerializable).GetMethod("KNetDeserialize")
                            .Invoke(Activator.CreateInstance(fieldType), new object[] { serializedField.data }));
                }
            }
        }
    }
}
