using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace VDV.Spline
{
    public static class SerializationHelper
    {
        internal static IEnumerable<FieldInfo> GetSerializableFields(Type type)
        {
            IEnumerable<FieldInfo> fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            while (type.BaseType != null)
            {
                type = type.BaseType;
                fields = fields.Concat(type.GetFields(BindingFlags.NonPublic |
                                                      BindingFlags.Instance | BindingFlags.DeclaredOnly));
            }

            return fields.Where(field => field.IsPublic || field.IsDefined(typeof(SerializeField), false));
        }

        public static void SerializeMonobehaviourContents(MonoBehaviour obj, string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            using (var writer = new BinaryWriter(File.OpenWrite(path)))
            {
                var formatter = new BinaryFormatter { SurrogateSelector = new UnitySelector() };
                Type type = obj.GetType();
                writer.Write(type.FullName);
                Dictionary<string, object> members =
                    GetSerializableFields(type)
                        .ToDictionary(field => field.Name, field => field.GetValue(obj));
                formatter.Serialize(writer.BaseStream, members);
            }
        }

        public static void DeserializeMonobehaviourContents(MonoBehaviour obj, string path)
        {
            if(string.IsNullOrEmpty(path)) return;

            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                Type type = obj.GetType();
                string fileFullName = reader.ReadString();
                string trueFullName = type.FullName;
                if (fileFullName != trueFullName)
                {
                    Debug.LogError(string.Format("Invalid File, expected class {0} but got {1}", trueFullName, fileFullName));
                    return;
                }
                var formatter = new BinaryFormatter { SurrogateSelector = new UnitySelector() };
                var fileMembers = formatter.Deserialize(reader.BaseStream) as Dictionary<string, object>;
                if (fileMembers == null)
                {
                    Debug.LogError("Invalid File Format");
                    return;
                }

                foreach (FieldInfo field in GetSerializableFields(type))
                {
                    object value;
                    string fieldName = field.Name;
                    if (fileMembers.TryGetValue(fieldName, out value))
                    {
                        field.SetValue(obj, value);
                        fileMembers.Remove(fieldName);
                    }
                    else
                    {
                        Debug.LogWarning(string.Format("Field not saved to file: {0}", fieldName));
                    }
                }

                foreach (KeyValuePair<string, object> member in fileMembers)
                {
                    Debug.LogWarning(string.Format("Unexpected field: {0}", member.Key));
                }
            }
        }
    }

    internal class UnitySelector : SurrogateSelector
    {
        public override ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            ISerializationSurrogate surrogate = base.GetSurrogate(type, context, out selector);
            if (surrogate != null)
            {
                return surrogate;
            }

            if (type.IsSerializable)
            {
                return null;
            }

            // Cannot currently (de)serialize monobehaviours using BinaryFormatter
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return new UnitySurrogate();
            }

            return null;
        }
    }

    internal class UnitySurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            foreach (FieldInfo field in SerializationHelper.GetSerializableFields(obj.GetType()))
            {
                info.AddValue(field.Name, field.GetValue(obj));
            }
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            foreach (FieldInfo field in SerializationHelper.GetSerializableFields(obj.GetType()))
            {
                field.SetValue(obj, info.GetValue(field.Name, field.FieldType));
            }

            return obj;
        }
    }
}
