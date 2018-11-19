using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Utilities.Serialization.Options;

namespace Utilities.Serialization
{
    using Newtonsoft.Json.Bson;

    /// <summary>
    /// Serialization extension
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Serialize object to default serialization type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be serialized</param>
        /// <returns>Serialized string</returns>
        public static string Serialize<T>(this T value)
        {
            return value.Serialize(SerializationTypes.Default);
        }

        /// <summary>
        /// Serialize object to selected serialization type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be serialized</param>
        /// <param name="serializationType">Serialization type</param>
        /// <returns>Serialized string</returns>
        public static string Serialize<T>(this T value, SerializationTypes serializationType)
        {
            switch (serializationType)
            {
                case SerializationTypes.Default:
                case SerializationTypes.Json:
                    return value.SerializeToJson();
                case SerializationTypes.Xml:
                    return value.SerializeToXml();
                case SerializationTypes.Bson:
                    return value.SerializeToBson();
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Serialize object to json string
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be serialized</param>
        /// <returns>Serialized json string</returns>
        private static string SerializeToJson<T>(this T value)
        {
            if (value == null) return string.Empty;

            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Serialize object to xml string
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be serialized</param>
        /// <returns>Serialized xml string</returns>
        private static string SerializeToXml<T>(this T value)
        {
            if (value == null) return string.Empty;
            
            using (var sw = new StringWriter())
            {
                using (var writer = XmlWriter.Create(sw))
                {
                    var xmlSerializer = new XmlSerializer(typeof(T));
                    xmlSerializer.Serialize(writer, value);
                    return sw.ToString();
                }
            }
        }

        /// <summary>
        /// Serialize object to bson string
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be serialized</param>
        /// <returns>Serialized bson string</returns>
        private static string SerializeToBson<T>(this T value)
        {
            if (value == null) return string.Empty;

            using (var ms = new MemoryStream())
            {
                using (var writer = new BsonDataWriter(ms))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(writer, value);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Deserialize string to given object type by default serialization type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be deserialized</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(this string value)
        {
            return value.Deserialize<T>(SerializationTypes.Default);
        }

        /// <summary>
        /// Deserialize string to given object type by selected serialization type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be deserialized</param>
        /// <param name="serializationType">Serialization type</param>
        /// <returns>Deserialized object</returns>
        public static T Deserialize<T>(this string value, SerializationTypes serializationType)
        {
            switch (serializationType)
            {
                case SerializationTypes.Default:
                case SerializationTypes.Json:
                    return value.DeserializeFromJson<T>();
                case SerializationTypes.Xml:
                    return value.DeserializeFromXml<T>();
                case SerializationTypes.Bson:
                    return value.DeserializeFromBson<T>();
                default:
                    return default(T);
            }
        }

        /// <summary>
        /// Deserialize json string to given object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be deserialized</param>
        /// <returns>Deserialized object</returns>
        private static T DeserializeFromJson<T>(this string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);

            try
            {
                //Try to deserialize
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception)
            {
                //Change type to string if there is an exception and type of T is string
                if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(value, typeof(T));
                //Otherwise return default of T
                else return default(T);
            }
        }

        /// <summary>
        /// Deserialize xml string to given object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be serialized</param>
        /// <returns>Serialized xml string</returns>
        private static T DeserializeFromXml<T>(this string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);

            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var sr = new StringReader(value))
            {
                using (var reader = XmlReader.Create(sr))
                {
                    var deserialized = xmlSerializer.Deserialize(reader);
                    return (T)Convert.ChangeType(deserialized, typeof(T));
                }
            }
        }

        /// <summary>
        /// Deserialize bson string to given object type
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="value">Value will be deserialized</param>
        /// <returns>Deserialized object</returns>
        private static T DeserializeFromBson<T>(this string value)
        {
            if (string.IsNullOrEmpty(value)) return default(T);

            try
            {
                var data = Convert.FromBase64String(value);
                using (var ms = new MemoryStream(data))
                {
                    using (var reader = new BsonDataReader(ms))
                    {
                        var serializer = new JsonSerializer();
                        return serializer.Deserialize<T>(reader);
                    }
                }
            }
            catch (Exception)
            {
                //Change type to string if there is an exception and type of T is string
                if (typeof(T) == typeof(string)) return (T)Convert.ChangeType(value, typeof(T));
                //Otherwise return default of T
                else return default(T);
            }
        }
    }
}
