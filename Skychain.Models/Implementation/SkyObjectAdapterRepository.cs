using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Содержит адаптеры всех типов объектов системы.
    /// </summary>
    public class SkyObjectAdapterRepository : ISkyObjectAdapterRepository
    {
        internal SkyObjectAdapterRepository(SkyContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.Context = context;
        }

        /// <summary>
        /// Контекст системы.
        /// </summary>
        public SkyContext Context { get; private set; }


        private bool __init_ObjectAdaptersByType = false;
        private Dictionary<string, object> _ObjectAdaptersByType;
        private Dictionary<string, object> ObjectAdaptersByType
        {
            get
            {
                if (!__init_ObjectAdaptersByType)
                {
                    _ObjectAdaptersByType = new Dictionary<string, object>();
                    __init_ObjectAdaptersByType = true;
                }
                return _ObjectAdaptersByType;
            }
        }


        /// <summary>
        /// Возвращает адаптер объектов системы.
        /// </summary>
        /// <typeparam name="TObject">Тип объекта системы.</typeparam>
        /// <typeparam name="TEntity">Тип сохраняемого объекта.</typeparam>
        /// <typeparam name="IObject">Тип интерфейса объекта.</typeparam>
        private SkyObjectAdapter<TObject, TEntity, IObject> GetObjectAdapter<TObject, TEntity, IObject>()
            where TEntity : SkyEntity
            where TObject : SkyObject<TObject, TEntity, IObject>, IObject
            where IObject : ISkyObject
        {
            SkyObjectAdapter<TObject, TEntity, IObject> adapter = null;
            Type type = typeof(TObject);
            string typeKey = type.AssemblyQualifiedName;
            if (!this.ObjectAdaptersByType.ContainsKey(typeKey))
            {
                adapter = new SkyObjectAdapter<TObject, TEntity, IObject>(this.Context);
                this.ObjectAdaptersByType.Add(typeKey, adapter);
            }
            else
            {
                adapter = (SkyObjectAdapter<TObject, TEntity, IObject>)this.ObjectAdaptersByType[typeKey];
            }

            if (adapter == null)
                throw new Exception(string.Format("Не удалось получить адаптера объектов системы SkyObjectAdapter по ключу {0}.", typeKey));

            return adapter;
        }


        /// <summary>
        /// Выполняет метод в контексте подключения к базе данных.
        /// </summary>
        /// <param name="action">Выполняемое действие.</param>
        internal void ExecuteQuery(Action<SkyEntityContext> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            using (SkyEntityContext entityContext = new SkyEntityContext())
            {
                action(entityContext);
            }
        }


        private bool __init_ProfileAdapter = false;
        private SkyObjectAdapter<SkyProfile, SkyProfileEntity, ISkyProfile> _Profiles;
        /// <summary>
        /// Адаптер профилей.
        /// </summary>
        public SkyObjectAdapter<SkyProfile, SkyProfileEntity, ISkyProfile> Profiles
        {
            get
            {
                if (!__init_ProfileAdapter)
                {
                    _Profiles = this.GetObjectAdapter<SkyProfile, SkyProfileEntity, ISkyProfile>();
                    __init_ProfileAdapter = true;
                }
                return _Profiles;
            }
        }


        private bool __init_DataSets = false;
        private SkyObjectAdapter<SkyDataSet, SkyDataSetEntity, ISkyDataSet> _DataSets;
        /// <summary>
        /// Адаптер наборов данных.
        /// </summary>
        public SkyObjectAdapter<SkyDataSet, SkyDataSetEntity, ISkyDataSet> DataSets
        {
            get
            {
                if (!__init_DataSets)
                {
                    _DataSets = this.GetObjectAdapter<SkyDataSet, SkyDataSetEntity, ISkyDataSet>();
                    __init_DataSets = true;
                }
                return _DataSets;
            }
        }


        private bool __init_Networks = false;
        private SkyObjectAdapter<SkyNetwork, SkyNetworkEntity, ISkyNetwork> _Networks;
        /// <summary>
        /// Адаптер нейросетей.
        /// </summary>
        public SkyObjectAdapter<SkyNetwork, SkyNetworkEntity, ISkyNetwork> Networks
        {
            get
            {
                if (!__init_Networks)
                {
                    _Networks = this.GetObjectAdapter<SkyNetwork, SkyNetworkEntity, ISkyNetwork>();
                    __init_Networks = true;
                }
                return _Networks;
            }
        }


        private bool __init_NetworkVersions = false;
        private SkyObjectAdapter<SkyNetworkVersion, SkyNetworkVersionEntity, ISkyNetworkVersion> _NetworkVersions;
        /// <summary>
        /// Адаптер версий нейросетей.
        /// </summary>
        public SkyObjectAdapter<SkyNetworkVersion, SkyNetworkVersionEntity, ISkyNetworkVersion> NetworkVersions
        {
            get
            {
                if (!__init_NetworkVersions)
                {
                    _NetworkVersions = this.GetObjectAdapter<SkyNetworkVersion, SkyNetworkVersionEntity, ISkyNetworkVersion>();
                      __init_NetworkVersions = true;
                }
                return _NetworkVersions;
            }
        }


        private bool __init_TrainSchemes = false;
        private SkyObjectAdapter<SkyTrainScheme, SkyTrainSchemeEntity, ISkyTrainScheme> _TrainSchemes;
        /// <summary>
        /// Адаптер схем тренировок нейросетей.
        /// </summary>
        public SkyObjectAdapter<SkyTrainScheme, SkyTrainSchemeEntity, ISkyTrainScheme> TrainSchemes
        {
            get
            {
                if (!__init_TrainSchemes)
                {
                    _TrainSchemes = this.GetObjectAdapter<SkyTrainScheme, SkyTrainSchemeEntity, ISkyTrainScheme>();
                     __init_TrainSchemes = true;
                }
                return _TrainSchemes;
            }
        }


        private bool __init_TrainEpochParams = false;
        private SkyObjectAdapter<SkyTrainEpochParams, SkyTrainEpochParamsEntity, ISkyTrainEpochParams> _TrainEpochParams;
        /// <summary>
        /// Адаптер параметров прохождения тренировочного сета для набора эпох.
        /// </summary>
        public SkyObjectAdapter<SkyTrainEpochParams, SkyTrainEpochParamsEntity, ISkyTrainEpochParams> TrainEpochParams
        {
            get
            {
                if (!__init_TrainEpochParams)
                {
                    _TrainEpochParams = this.GetObjectAdapter<SkyTrainEpochParams, SkyTrainEpochParamsEntity, ISkyTrainEpochParams>();
                     __init_TrainEpochParams = true;
                }
                return _TrainEpochParams;
            }
        }


        private bool __init_TrainRequests = false;
        private SkyObjectAdapter<SkyTrainRequest, SkyTrainRequestEntity, ISkyTrainRequest> _TrainRequests;
        /// <summary>
        /// Адаптер запросов тренировки нейросети.
        /// </summary>
        public SkyObjectAdapter<SkyTrainRequest, SkyTrainRequestEntity, ISkyTrainRequest> TrainRequests
        {
            get
            {
                if (!__init_TrainRequests)
                {
                    _TrainRequests = this.GetObjectAdapter<SkyTrainRequest, SkyTrainRequestEntity, ISkyTrainRequest>();
                     __init_TrainRequests = true;
                }
                return _TrainRequests;
            }
        }


        private bool __init_NetworkStates = false;
        private SkyObjectAdapter<SkyNetworkState, SkyNetworkStateEntity, ISkyNetworkState> _NetworkStates;
        /// <summary>
        /// Адаптер состояний нейросети, сформированных в результате тренировки.
        /// </summary>
        public SkyObjectAdapter<SkyNetworkState, SkyNetworkStateEntity, ISkyNetworkState> NetworkStates
        {
            get
            {
                if (!__init_NetworkStates)
                {
                    _NetworkStates = this.GetObjectAdapter<SkyNetworkState, SkyNetworkStateEntity, ISkyNetworkState>();
                     __init_NetworkStates = true;
                }
                return _NetworkStates;
            }
        }


        private bool __init_NetworkRequests = false;
        private SkyObjectAdapter<SkyNetworkRequest, SkyNetworkRequestEntity, ISkyNetworkRequest> _NetworkRequests;
        /// <summary>
        /// Адаптер запросов пользователей к нейросетям.
        /// </summary>
        public SkyObjectAdapter<SkyNetworkRequest, SkyNetworkRequestEntity, ISkyNetworkRequest> NetworkRequests
        {
            get
            {
                if (!__init_NetworkRequests)
                {
                    _NetworkRequests = this.GetObjectAdapter<SkyNetworkRequest, SkyNetworkRequestEntity, ISkyNetworkRequest>();
                     __init_NetworkRequests = true;
                }
                return _NetworkRequests;
            }
        }


        ISkyContext ISkyObjectAdapterRepository.Context => this.Context;

        ISkyObjectAdapter<ISkyProfile, SkyProfileEntity> ISkyObjectAdapterRepository.Profiles => this.Profiles;

        ISkyObjectAdapter<ISkyDataSet, SkyDataSetEntity> ISkyObjectAdapterRepository.DataSets => this.DataSets;

        ISkyObjectAdapter<ISkyNetwork, SkyNetworkEntity> ISkyObjectAdapterRepository.Networks => this.Networks;

        ISkyObjectAdapter<ISkyNetworkVersion, SkyNetworkVersionEntity> ISkyObjectAdapterRepository.NetworkVersions => this.NetworkVersions;

        ISkyObjectAdapter<ISkyTrainScheme, SkyTrainSchemeEntity> ISkyObjectAdapterRepository.TrainSchemes => this.TrainSchemes;

        ISkyObjectAdapter<ISkyTrainEpochParams, SkyTrainEpochParamsEntity> ISkyObjectAdapterRepository.TrainEpochParams => this.TrainEpochParams;

        ISkyObjectAdapter<ISkyTrainRequest, SkyTrainRequestEntity> ISkyObjectAdapterRepository.TrainRequests => this.TrainRequests;

        ISkyObjectAdapter<ISkyNetworkState, SkyNetworkStateEntity> ISkyObjectAdapterRepository.NetworkStates => this.NetworkStates;

        ISkyObjectAdapter<ISkyNetworkRequest, SkyNetworkRequestEntity> ISkyObjectAdapterRepository.NetworkRequests => this.NetworkRequests;
    }
}
