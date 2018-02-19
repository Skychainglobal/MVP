using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Skychain.Models;
using Skychain.Models.Implementation;
using SkychainAPI;

namespace Skychain.Models.PublicAPI
{
	/// <summary>
	/// Контекст запроса к сети.
	/// </summary>
	internal class InferenceOperationContext : IOperationContext
	{
		public InferenceOperationContext(SkyNetworkRequest skyNetworkRequest)
		{
			this.NetworkRequest = skyNetworkRequest;
		}

		private bool __init_InitialState;
		private byte[] _InitialState;
		private SkyNetworkRequest NetworkRequest;

		/// <summary>
		/// Начальное состояние.
		/// </summary>
		public byte[] InitialState
		{
			get
			{
				if (!__init_InitialState)
				{
					using (MemoryStream stream = new MemoryStream())
					using (Stream st = this.NetworkRequest.NetworkState.StateData.Open())
					{
						byte[] buffer = new byte[2048];
						int read;
						while (st.CanRead && (read = st.Read(buffer, 0, buffer.Length)) > 0)
						{
							stream.Write(buffer, 0, read);
						}
						_InitialState = stream.ToArray();
					}
					__init_InitialState = true;
				}
				return _InitialState;
			}
		}

		/// <summary>
		/// Сохранение модели.
		/// </summary>
		/// <param name="checkpoint"></param>
		public void SaveModel(byte[] checkpoint)
		{
			throw new Exception("Not allowed in this context");
		}

		/// <summary>
		/// Запись лога.
		/// </summary>
		/// <param name="message"></param>
		public void WriteLog(string message)
		{
			throw new Exception("Not allowed in this context");
		}

		/// <summary>
		/// Преобразует данные из JSON`а в объект для Public API.
		/// </summary>
		/// <param name="jsonValue"></param>
		/// <returns></returns>
		internal static Dictionary<string, Dictionary<string, object>> CreateInputData(string jsonValue)
		{
			var temp = new Dictionary<string, Dictionary<string, object>>();
			if (String.IsNullOrEmpty(jsonValue))
				return temp;

			using (var stringReader = new StringReader(jsonValue))
			using (var jsonReader = new JsonTextReader(stringReader))
			{
				Stack<string> names = new Stack<string>();
				string currentObjName;

				bool f = true;
				while (jsonReader.Read())
				{
					switch (jsonReader.TokenType)
					{
						case JsonToken.StartObject:
							if (f)
							{
								f = false;
								continue;
							}
							jsonReader.Read();
							currentObjName = jsonReader.Value.ToString();
							names.Push(currentObjName);
							if (!temp.ContainsKey(currentObjName))
								temp.Add(currentObjName, new Dictionary<string, object>());


							break;
						case JsonToken.PropertyName:
							string propName = jsonReader.Value.ToString();
							jsonReader.Read();
							if (jsonReader.TokenType == JsonToken.StartObject)
							{
								names.Push(propName);
								if (!temp.ContainsKey(propName))
									temp.Add(propName, new Dictionary<string, object>());
							}
							else
							{
								temp[names.Peek()].Add(propName, jsonReader.Value);
							}
							break;
						case JsonToken.EndObject:
							if (names.Count > 0)
								names.Pop();
							break;
					}
				}
			}
			return temp;
		}

		/// <summary>
		/// Opens file stream.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public Stream OpenFile(Guid guid)
		{
			var file = this.NetworkRequest.GetInputFile(guid);
			if (file == null)
				return null;

			return file.Open();
		}
	}
}