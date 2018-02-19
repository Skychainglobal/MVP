using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SkychainAPI;
using Skychain.Models;

namespace Skychain.Models.PublicAPI
{
	/// <summary>
	/// Тренировочная схема.
	/// </summary>
	internal class TrainScheme : ITrainScheme
	{
		/// <summary>
		/// Тренировочная схема.
		/// </summary>
		public TrainScheme(ISkyTrainScheme scheme)
		{
			Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
		}

		private bool __init_Epochs;
		private IEnumerable<IEpoch> _Epochs;
		/// <summary>
		/// Эпохи.
		/// </summary>
		public IEnumerable<IEpoch> Epochs
		{
			get
			{
				if (!__init_Epochs)
				{
					_Epochs = this.Scheme.Epochs.Select(x => new TrainEpoch(x)).ToList();
					__init_Epochs = true;
				}
				return _Epochs;
			}
		}

		/// <summary>
		/// Схема.
		/// </summary>
		public ISkyTrainScheme Scheme { get; }
	}
}