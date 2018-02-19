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
	/// Представляет контекст работы с сохраняемыми данными.
	/// </summary>
	[DbConfigurationType(typeof(SkyEntityContextConfiguration))]
	public class SkyEntityContext : DbContext
    {
        internal SkyEntityContext()
            : base("DefaultConnection")
        {
        }

        /// <summary>
        /// Инициализрует базу данных и таблицы.
        /// </summary>
        /// <param name="modelBuilder">Построитель модели.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<SkyEntityContext>(null);
			base.OnModelCreating(modelBuilder);
        }
		
		/// <summary>
		/// Данные профилей.
		/// </summary>
		public DbSet<SkyProfileEntity> Profiles { get; set; }

        /// <summary>
        /// Данные наборов данных нейросетей.
        /// </summary>
        public DbSet<SkyDataSetEntity> DataSets { get; set; }

		/// <summary>
		/// Данные нейросетей.
		/// </summary>
		public DbSet<SkyNetworkEntity> Networks { get; set; }

		/// <summary>
		/// Данные запросов к нейросетям.
		/// </summary>
		public DbSet<SkyNetworkRequestEntity> NetworksRequests { get; set; }

		/// <summary>
		/// Данные схем тренировок.
		/// </summary>
		public DbSet<SkyTrainSchemeEntity> TrainSchemes { get; set; }

		/// <summary>
		/// Данные параметров эпох.
		/// </summary>
		public DbSet<SkyTrainEpochParamsEntity> TrainEpochParams { get; set; }

        /// <summary>
        /// Данные запросов тренировки нейросети.
        /// </summary>
        public DbSet<SkyTrainRequestEntity> TrainRequests { get; set; }

        /// <summary>
        /// Данные версий нейросетей.
        /// </summary>
        public DbSet<SkyNetworkVersionEntity> NetworksVersions { get; set; }

		/// <summary>
		/// Данные состояний сетей.
		/// </summary>
		public DbSet<SkyNetworkStateEntity> NetworksStates { get; set; }
    }
}
