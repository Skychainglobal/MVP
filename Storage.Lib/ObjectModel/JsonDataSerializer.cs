using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Storage.Lib
{
    /// <summary>
    /// Сериализатор данных.
    /// </summary>
    public class JsonDataSerializer
    {
        private JsonDataSerializer() { }

        #region JSON
        /// <summary>
        /// Сериализует объект в поток.
        /// </summary>
        /// <param name="stream">Поток.</param>
        /// <param name="obj">Объект.</param>
        /// <param name="serializerType">Тип сериализации.</param>
        public static void Serialize(Stream stream, object obj, JsonSerializerType serializerType = JsonSerializerType.DataContractJsonSerializer)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (obj == null)
                throw new ArgumentNullException("obj");

            if (!stream.CanWrite)
                throw new Exception(string.Format("Невозможно сериализовать объект в поток, потому что запись в поток запрещена."));

            string json = JsonDataSerializer.SerializeJson(obj, serializerType);
            byte[] data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Десериализует объект из потока.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="stream">Поток.</param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (!stream.CanRead)
                throw new Exception(string.Format("Невозможно десериализовать объект из потока, потому что чтение потока запрещено."));

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            string json = Encoding.UTF8.GetString(data);
            T obj = JsonDataSerializer.GetInstanceJson<T>(json);

            return obj;
        }

        /// <summary>
        /// Сериализует объект в строку в формате Json.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <returns></returns>
        public static string SerializeJson(object obj)
        {
            if (obj == null)
                throw new Exception("obj is null");

            string result = string.Empty;
            Type objType = obj.GetType();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(objType);

            using (Stream st = new MemoryStream())
            {
                serializer.WriteObject(st, obj);
                st.Position = 0;
                byte[] buffer = new byte[(int)st.Length];

                int readBytes = st.Read(buffer, 0, (int)st.Length);
                result = Encoding.UTF8.GetString(buffer);
            }

            return result;
        }

        /// <summary>
        /// Сериализует объект в строку в формате Json.
        /// </summary>
        /// <param name="obj">Объект.</param>
        /// <param name="serializerType">Тип сериализации.</param>
        /// <returns></returns>
        public static string SerializeJson(object obj, JsonSerializerType serializerType = JsonSerializerType.DataContractJsonSerializer)
        {
            if (obj == null)
                throw new Exception("obj is null");

            string result = null;
            if (serializerType == JsonSerializerType.DataContractJsonSerializer)
                result = JsonDataSerializer.SerializeJson(obj);
            else if (serializerType == JsonSerializerType.JavaScriptSerializer)
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = int.MaxValue;
                result = serializer.Serialize(obj);
            }

            return result;
        }

        /// <summary>
        /// Создает объект из строки JSON.
        /// </summary>
        /// <param name="json">Строка JSON, в которой описаны свойства объекта.</param>
        /// <returns></returns>
        public static T GetInstanceJson<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException("json");

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            T obj = jsSerializer.Deserialize<T>(json);

            return obj;
        }
        #endregion
    }

    /// <summary>
    /// Тип сериализатора.
    /// </summary>
    public enum JsonSerializerType
    {
        /// <summary>
        /// Сериализатор, основанный на DataContracts.
        /// </summary>
        DataContractJsonSerializer,
        /// <summary>
        /// Javascript сериализатор.
        /// </summary>
        JavaScriptSerializer
    }
}