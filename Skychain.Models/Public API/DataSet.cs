using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkychainAPI;
using Skychain.Models;
using System.IO;

namespace Skychain.Models.PublicAPI
{
	/// <summary>
	/// Датасет.
	/// </summary>
	internal class DataSet : IDataSet
	{
		/// <summary>
		/// Датасет.
		/// </summary>
		/// <param name="dataset"></param>
		public DataSet(ISkyDataSet dataset)
		{
			Dataset = dataset;
		}

		/// <summary>
		/// Датасет.
		/// </summary>
		public ISkyDataSet Dataset { get; }

		/// <summary>
		/// Открывает поток для чтения датасета.
		/// </summary>
		/// <returns></returns>
		public Stream Open()
		{
			return this.Dataset.TrainSet.Open();
		}
	}
}