using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет нейросеть.
    /// </summary>
    public class SkyNetwork : SkyObject<SkyNetwork, SkyNetworkEntity, ISkyNetwork>, ISkyNetwork
    {
        internal SkyNetwork(SkyNetworkEntity entity, SkyObjectAdapter<SkyNetwork, SkyNetworkEntity, ISkyNetwork> adapter)
            : base(entity, adapter)
        {
        }


        /// <summary>
        /// Идентификатор профиля.
        /// </summary>
        internal int ProfileID
        {
            get { return this.Entity.ProfileID; }
            set
            {
                this.Entity.ProfileID = value;
                this.__init_Profile = false;
            }
        }


        private bool __init_Profile = false;
        private SkyProfile _Profile;
        /// <summary>
        /// Профиль, которому принадлежит набор данных.
        /// </summary>
        public SkyProfile Profile
        {
            get
            {
                if (!__init_Profile)
                {
                    _Profile = this.Context.ObjectAdapters.Profiles.GetObject(this.ProfileID, true);
                    __init_Profile = true;
                }
                return _Profile;
            }
        }


        /// <summary>
        /// Название нейросети.
        /// </summary>
        public string Name
        {
            get { return this.Entity.Name; }
            set { this.Entity.Name = value; }
        }


        /// <summary>
        /// Описание нейросети.
        /// </summary>
        public string Description
        {
            get { return this.Entity.Description; }
            set { this.Entity.Description = value; }
        }


        /// <summary>
        /// Стоимость использования нейросети.
        /// </summary>
        public decimal Cost
        {
            get { return this.Entity.Cost; }
            set { this.Entity.Cost = value; }
        }


        /// <summary>
        /// Возвращает true, если набор данных опубликован.
        /// </summary>
        public bool Published
        {
            get { return this.Entity.Published; }
            set { this.Entity.Published = value; }
        }


        /// <summary>
        /// Проверяет, является ли набор данных опубликованным, в ином случае генерирует исключение.
        /// </summary>
        public void CheckPublished()
        {
            if (!this.Published)
                throw new Exception(string.Format("Network [{0}] is not published.", this.Name));
        }


        /// <summary>
        /// Данные текущего состояния нейросети.
        /// Изменяются после выполнения тренировки сети.
        /// </summary>
        public byte[] StateData
        {
            get { return this.Entity.StateData; }
            set { this.Entity.StateData = value; }
        }


        /// <summary>
        /// Рейтинг нейросети.
        /// </summary>
        public double Rating
        {
            get { return this.Entity.Rating; }
            set { this.Entity.Rating = value; }
        }


        private bool __init_UsagesCount = false;
        private int _UsagesCount;
        /// <summary>
        /// Количество использований сети.
        /// </summary>
        public int UsagesCount
        {
            get
            {
                if (!__init_UsagesCount)
                {
                    _UsagesCount = this.IsNew ?
                        0 : this.Context.ObjectAdapters.NetworkRequests.ExecuteQuery<int>(dbSet => dbSet.Count(x => x.NetworkID == this.ID));
                    __init_UsagesCount = true;
                }
                return _UsagesCount;
            }
        }


        private bool __init_ActiveVersion = false;
        private SkyNetworkVersion _ActiveVersion;
        /// <summary>
        /// Активная версия нейросети.
        /// </summary>
        public SkyNetworkVersion ActiveVersion
        {
            get
            {
                if (!__init_ActiveVersion)
                {
                    _ActiveVersion = this.Context.ObjectAdapters.NetworkVersions.GetObject(dbSet =>
                        dbSet.FirstOrDefault(x => x.IsActive));
                    __init_ActiveVersion = true;
                }
                return _ActiveVersion;
            }
        }

        internal void ResetActiveVersion()
        {
            this.__init_ActiveVersion = false;
        }


        /// <summary>
        /// Проверяет наличие активной версии нейросети.
        /// Генерирует исключение в случае отсутствия активной версии.
        /// </summary>
        public void CheckActiveVersion()
        {
            if (this.ActiveVersion == null)
                throw new Exception(string.Format("Neural network with ID={0} has no active version.", this.ID));
        }


        /// <summary>
        /// Создаёт новый экземляр запроса к активному состоянию активной версии данной нейросети.
        /// </summary>
        /// <param name="user">Пользователь, формирующий запрос.</param>
        public SkyNetworkRequest CreateRequest(ISkyUser user)
        {
            this.CheckActiveVersion();
            return this.ActiveVersion.CreateRequest(user);
        }


        private bool __init_Versions = false;
        private IEnumerable<SkyNetworkVersion> _Versions;
        /// <summary>
        /// Версии нейросети.
        /// </summary>
        public IEnumerable<SkyNetworkVersion> Versions
        {
            get
            {
                if (!__init_Versions)
                {
                    _Versions = this.Context.ObjectAdapters.NetworkVersions.GetObjects(dbSet => dbSet
                        .Where(x => x.NetworkID == this.ID)
                        .OrderBy(x => x.ID));

                    __init_Versions = true;
                }
                return _Versions;
            }
        }

        internal void ResetVersions()
        {
            this.__init_Versions = false;
        }

        /// <summary>
        /// Создаёт новый экземпляр версии нейросети, без сохранения в базу данных.
        /// </summary>
        public SkyNetworkVersion CreateVersion()
        {
            SkyNetworkVersion networkVersion = this.Context.ObjectAdapters.NetworkVersions.CreateObject();
            networkVersion.NetworkID = this.ID;
            return networkVersion;
        }


        /// <summary>
        /// Возвращает новый идентификатор версии нейросети в рамках нейросети.
        /// </summary>
        internal int GetVersionsInternalID()
        {
            this.CheckExists();
            this.Entity.VersionsInternalIDCounter++;
            this.Update();
            return this.Entity.VersionsInternalIDCounter;
        }


        private bool __init_VersionsByInternalID = false;
        private Dictionary<int, SkyNetworkVersion> _VersionsByInternalID;
        /// <summary>
        /// Словарь версий нейросети, сформированный по ключу в виде идентификатора нейросети.
        /// </summary>
        private Dictionary<int, SkyNetworkVersion> VersionsByInternalID
        {
            get
            {
                if (!__init_VersionsByInternalID)
                {
                    _VersionsByInternalID = new Dictionary<int, SkyNetworkVersion>();
                    __init_VersionsByInternalID = true;
                }
                return _VersionsByInternalID;
            }
        }

        
        /// <summary>
        /// Возвращает версию нейросети по идентификатору версии в рамках нейросети.
        /// </summary>
        /// <param name="versionInternalID">Идентификатор версии в рамках нейсросети.</param>
        /// <param name="throwNotFoundException">Генерирует исключение в случае отсутствия версии с заданным идентификатором в рамках нейросети.</param>
        public SkyNetworkVersion GetVersion(int versionInternalID, bool throwNotFoundException)
        {
            if (versionInternalID == 0)
                throw new ArgumentNullException("versionInternalID");

            SkyNetworkVersion version = null;
            if (!this.VersionsByInternalID.ContainsKey(versionInternalID))
            {
                //получаем версию по идентификатору в рамках нейросети.
                version = this.Context.ObjectAdapters.NetworkVersions.GetObject(dbSet => dbSet.FirstOrDefault(x =>
                    x.NetworkID == this.ID && x.InternalID == versionInternalID));

                //добавляем версию в словарь.
                if (version != null)
                    this.VersionsByInternalID.Add(versionInternalID, version);
            }
            else
                version = this.VersionsByInternalID[versionInternalID];

            //ругаемся при отсутствии версии.
            if (throwNotFoundException && version == null)
                throw new Exception(string.Format("Network version with InternalID={0} for network [{1}] is not exists.", versionInternalID, this.Name));

            //возвращаем версию.
            return version;
        }


        /// <summary>
        /// Добавляет созданную версию в словарь версий по внутренним идентификаторам.
        /// </summary>
        /// <param name="createdVersion">Созданная версия нейросети.</param>
        internal void AddCreatedVersion(SkyNetworkVersion createdVersion)
        {
            if (createdVersion == null)
                throw new ArgumentNullException("createdVersion");

            //проверяем, что версия создана.
            createdVersion.CheckExists();

            //проверяем наличие внутреннего идентификатора.
            createdVersion.CheckInternalID();

            //добавляем версию в словарь.
            if (!this.VersionsByInternalID.ContainsKey(createdVersion.InternalID))
                this.VersionsByInternalID.Add(createdVersion.InternalID, createdVersion);
        }

        private bool __init_TrainSchemes = false;
        private IEnumerable<SkyTrainScheme> _TrainSchemes;
        /// <summary>
        /// Схемы тренировки нейросети.
        /// </summary>
        public IEnumerable<SkyTrainScheme> TrainSchemes
        {
            get
            {
                if (!__init_TrainSchemes)
                {
                    _TrainSchemes = this.Context.ObjectAdapters.TrainSchemes.GetObjects(dbSet => dbSet
                        .Where(x => x.NetworkID == this.ID)
                        .OrderBy(x => x.ID));

                    __init_TrainSchemes = true;
                }
                return _TrainSchemes;
            }
        }

        internal void ResetTrainSchemes()
        {
            this.__init_TrainSchemes = false;
        }

        /// <summary>
        /// Создаёт новый экземпляр схемы тренировки нейросети, без сохранения в базу данных.
        /// </summary>
        public SkyTrainScheme CreateTrainScheme()
        {
            SkyTrainScheme trainScheme = this.Context.ObjectAdapters.TrainSchemes.CreateObject();
            trainScheme.NetworkID = this.ID;
            return trainScheme;
        }


        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //проверяем наличие профиля.
            this.Profile.CheckExists();

            //обновляем объект.
            base.Update();

            //сбрасываем флаг инициализации коллекции нейросетей у профиля.
            if (this.JustCreated)
                this.Profile.ResetNetworks();
        }


        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            //удаляем схемы тренировок.
            this.DeleteChildren(this.TrainSchemes);

            //удаляем версии нейросети.
            this.DeleteChildren(this.Versions);
            this.__init_ActiveVersion = false;

            //удаляем объект из базы данных.
            base.Delete();

            //сбрасываем флаг инициализации коллекции нейросетей у профиля.
            this.Profile.ResetNetworks();
        }


        /// <summary>
        /// Текстовое представление экземпляра класса.
        /// </summary>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.Name))
                return this.Name;
            return base.ToString();
        }


        ISkyProfile ISkyNetwork.Profile => this.Profile;

        ISkyNetworkVersion ISkyNetwork.ActiveVersion => this.ActiveVersion;

        ISkyNetworkRequest ISkyNetwork.CreateRequest(ISkyUser user)
        {
            return this.CreateRequest(user);
        }

        IEnumerable<ISkyNetworkVersion> ISkyNetwork.Versions => this.Versions;

        ISkyNetworkVersion ISkyNetwork.CreateVersion()
        {
            return this.CreateVersion();
        }

        ISkyNetworkVersion ISkyNetwork.GetVersion(int versionInternalID)
        {
            return this.GetVersion(versionInternalID, false);
        }

        ISkyNetworkVersion ISkyNetwork.GetVersion(int versionInternalID, bool throwNotFoundException)
        {
            return this.GetVersion(versionInternalID, throwNotFoundException);
        }

        IEnumerable<ISkyTrainScheme> ISkyNetwork.TrainSchemes => this.TrainSchemes;

        ISkyTrainScheme ISkyNetwork.CreateTrainScheme()
        {
            return this.CreateTrainScheme();
        }
    }
}
