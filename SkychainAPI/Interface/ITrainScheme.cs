using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkychainAPI
{
	/// <summary>
	/// 
	/// </summary>
	public interface ITrainScheme 
	{
		IEnumerable<IEpoch> Epochs  { get; }
	}
}
