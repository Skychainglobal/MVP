using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkychainAPI
{
	/// <summary>
	/// Итерация цикла обучения нейросети.
	/// </summary>
	public interface IEpoch
	{
		/// <summary>
		/// Номер текущей эпохи обучения.
		/// </summary>
		int Number { get; }
				
		/// <summary>
		/// Dataset.
		/// </summary>
		IDataSet Dataset { get; }

		/// <summary>
		/// Кастомные параметры итерации цикла обучения.
		/// </summary>
		string CustomParams { get; }
	}
}
