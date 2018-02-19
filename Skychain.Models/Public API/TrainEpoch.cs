using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkychainAPI;
using Skychain.Models;

namespace Skychain.Models.PublicAPI
{
	/// <summary>
	/// Тренировочная эпоха.
	/// </summary>
	internal class TrainEpoch : IEpoch
	{
		/// <summary>
		/// Тренировочная эпоха.
		/// </summary>
		public TrainEpoch(ISkyTrainEpoch epoch)
		{
			Epoch = epoch;
			_dataset = new DataSet(Epoch.Params.DataSet);
		}

		/// <summary>
		/// Эпоха.
		/// </summary>
		public ISkyTrainEpoch Epoch { get; private set; }

		/// <summary>
		/// Датасет.
		/// </summary>
		private DataSet _dataset;

		#region IEpoch
		/// <summary>
		/// Кастомные параметры.
		/// </summary>
		public string CustomParams => Epoch.Params.CustomParameters;

		/// <summary>
		/// Датасет.
		/// </summary>
		public IDataSet Dataset => _dataset;

		/// <summary>
		/// Порядковый номер эпохи.
		/// </summary>
		public int Number => Epoch.EpochNumber;
		#endregion
	}
}