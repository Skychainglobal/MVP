using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Skychain.Models;
using SkychainAPI;

namespace Skychain.Models.PublicAPI
{
	/// <summary>
	/// Контекст обучения сети.
	/// </summary>
	internal class TrainOperationContext : IOperationContext
	{
		/// <summary>
		/// Контекст обучения сети.
		/// </summary>
		/// <param name="request"></param>
		public TrainOperationContext(ISkyTrainRequest request)
		{
			Request = request ?? throw new ArgumentNullException(nameof(request));
		}
		
		/// <summary>
		/// Запрос.
		/// </summary>
		public ISkyTrainRequest Request { get; }

		private bool __init_InitialState;
		private byte[] _InitialState;
		/// <summary>
		/// Начальное состояние.
		/// </summary>
		public byte[] InitialState
		{
			get
			{
				if (!__init_InitialState)
				{
					if (this.Request.InitialState!= null)
					{
						using (MemoryStream stream = new MemoryStream())
						using (Stream st = this.Request.InitialState.StateData.Open())
						{
							byte[] buffer = new byte[2048];
							int read;
							while (st.CanRead && (read = st.Read(buffer, 0, buffer.Length)) > 0)
							{
								stream.Write(buffer, 0, read);
							}
							_InitialState = stream.ToArray();
						}
					}
					__init_InitialState = true;
				}
				return _InitialState;
			}
		}

		/// <summary>
		/// Сохранённое состояние.
		/// </summary>
		internal byte[] SavedState = null;

		/// <summary>
		/// Сохраняет состояние.
		/// </summary>
		/// <param name="checkpoint"></param>
		public void SaveModel(byte[] checkpoint)
		{
			SavedState = checkpoint;
		}
		
		/// <summary>
		/// Записывает лог.
		/// </summary>
		/// <param name="message"></param>
		public void WriteLog(string message)
		{
			this.Request.WriteTrainLog(message);
		}

		/// <summary>
		/// Открывает файл для чтения.
		/// </summary>
		/// <param name="guid"></param>
		/// <returns></returns>
		public Stream OpenFile(Guid guid)
		{
			throw new Exception("Not allowed in this context");
		}
	}
}