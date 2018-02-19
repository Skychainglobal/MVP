using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkychainAPI
{
	/// <summary>
	/// Интерфейс нейросети.
	/// </summary>
	public interface INeuralNetwork
	{
		/// <summary>
		/// Training process.
		/// </summary>
		/// <param name="operationContext">Operation context.</param>
		/// <param name="scheme">Train scheme.</param>
		void Train(IOperationContext operationContext, ITrainScheme scheme);

		/// <summary>
		/// Валидирует результат работы нейросети.
		/// </summary>
		/// <param name="operationContext">Контекст выполнения операции.</param>
		/// <param name="dataset">Датасет.</param>
		/// <returns></returns>
		//string Validate(IOperationContext operationContext, IDataSet dataset);

		/// <summary>
		/// ???
		/// </summary>
		/// <param name="operationContext">Контекст выполнения операции.</param>
		/// <param name="input">Входные данные.</param>
		/// <returns></returns>
		string Inference(IOperationContext operationContext, Dictionary<string, Dictionary<string, object>> input);
	}
}
