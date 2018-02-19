using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models
{
    /// <summary>
    /// Содержит адаптеры всех типов объектов системы.
    /// </summary>
    public interface ISkyObjectAdapterRepository
    {
        /// <summary>
        /// Контекст системы.
        /// </summary>
        ISkyContext Context { get; }

        /// <summary>
        /// Адаптер профилей.
        /// </summary>
        ISkyObjectAdapter<ISkyProfile, SkyProfileEntity> Profiles { get; }

        /// <summary>
        /// Адаптер наборов данных.
        /// </summary>
        ISkyObjectAdapter<ISkyDataSet, SkyDataSetEntity> DataSets { get; }

        /// <summary>
        /// Адаптер нейросетей.
        /// </summary>
        ISkyObjectAdapter<ISkyNetwork, SkyNetworkEntity> Networks { get; }

        /// <summary>
        /// Адаптер версий нейросетей.
        /// </summary>
        ISkyObjectAdapter<ISkyNetworkVersion, SkyNetworkVersionEntity> NetworkVersions { get; }

        /// <summary>
        /// Адаптер схем тренировок нейросетей.
        /// </summary>
        ISkyObjectAdapter<ISkyTrainScheme, SkyTrainSchemeEntity> TrainSchemes { get; }

        /// <summary>
        /// Адаптер параметров прохождения тренировочного сета для набора эпох.
        /// </summary>
        ISkyObjectAdapter<ISkyTrainEpochParams, SkyTrainEpochParamsEntity> TrainEpochParams { get; }

        /// <summary>
        /// Адаптер запросов тренировки нейросети.
        /// </summary>
        ISkyObjectAdapter<ISkyTrainRequest, SkyTrainRequestEntity> TrainRequests { get; }

        /// <summary>
        /// Адаптер состояний нейросети, сформированных в результате тренировки.
        /// </summary>
        ISkyObjectAdapter<ISkyNetworkState, SkyNetworkStateEntity> NetworkStates { get; }

        /// <summary>
        /// Адаптер запросов пользователей к нейросетям.
        /// </summary>
        ISkyObjectAdapter<ISkyNetworkRequest, SkyNetworkRequestEntity> NetworkRequests { get; }
    }
}
