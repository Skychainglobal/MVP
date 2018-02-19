using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Entity
{
	/// <summary>
	/// Конфигурация контекста SkyEntity.
	/// </summary>
	internal class SkyEntityContextConfiguration : DbConfiguration
	{
		/// <summary>
		/// Конфигурация контекста SkyEntity.
		/// </summary>
		protected internal SkyEntityContextConfiguration()
		{
			//Устанавливаем фабрику контекста
			SetContextFactory<SkyEntityContext>(() => new SkyEntityContext());
		}
	}

}

