using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkychainAPI
{
	/// <summary>
	/// Контекст выполнения операции с нейросетью.
	/// </summary>
	public interface IOperationContext
	{
		/// <summary>
		/// Saves the state of the neural network.
		/// </summary>
		/// <param name="checkpoint">Model state.</param>
		void SaveModel(byte[] checkpoint);
		
		/// <summary>
		/// Restore model from previous state.
		/// </summary>
		/// <returns></returns>
		byte[] InitialState { get; }

		/// <summary>
		/// Writes log.
		/// </summary>
		/// <param name="message"></param>
		void WriteLog(string message);

		/// <summary>
		/// Opens file stream for file with id equals <paramref name="guid"/>.
		/// </summary>
		/// <param name="guid">File ID.</param>
		/// <returns>File stream.</returns>
		Stream OpenFile(Guid guid);
	}
}
