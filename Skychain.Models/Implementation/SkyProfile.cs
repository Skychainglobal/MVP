using Skychain.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет профиль пользователя.
    /// </summary>
    public class SkyProfile : SkyObject<SkyProfile, SkyProfileEntity, ISkyProfile>, ISkyProfile
    {
        internal SkyProfile(SkyProfileEntity entity, SkyObjectAdapter<SkyProfile, SkyProfileEntity, ISkyProfile> adapter)
            : base(entity, adapter)
        {
        }

        /// <summary>
        /// Идентификатор пользователя, к которому относится профиль.
        /// </summary>
        internal Guid UserID
        {
            get { return this.Entity.UserID; }
            set
            {
                this.Entity.UserID = value;
                this.__init_User = false;
            }
        }


        private bool __init_User = false;
        private SkyUser _User;
        /// <summary>
        /// Пользователь, к которому относится профиль.
        /// </summary>
        public SkyUser User
        {
            get
            {
                if (!__init_User)
                {
                    _User = this.Context.GetUser(this.UserID);
                    __init_User = true;
                }
                return _User;
            }
        }


        /// <summary>
        /// Адрес электронной почты профиля.
        /// </summary>
        public string Email
        {
            get { return this.Entity.Email; }
            set { this.Entity.Email = value; }
        }

        /// <summary>
        /// Определяет, отображать ли адрес электронной почты для всех пользователей системы.
        /// </summary>
        public bool ShowEmailForEveryone
        {
            get { return this.Entity.ShowEmailForEveryone; }
            set { this.Entity.ShowEmailForEveryone = value; }
        }

        /// <summary>
        /// Имя профиля.
        /// </summary>
        public string Name
        {
            get { return this.Entity.Name; }
            set { this.Entity.Name = value; }
        }

        /// <summary>
        /// Общая информация о профиле.
        /// </summary>
        public string Bio
        {
            get { return this.Entity.Bio; }
            set { this.Entity.Bio = value; }
        }

        /// <summary>
        /// Страна.
        /// </summary>
        public string Country
        {
            get { return this.Entity.Country; }
            set { this.Entity.Country = value; }
        }

        /// <summary>
        /// Город.
        /// </summary>
        public string City
        {
            get { return this.Entity.City; }
            set { this.Entity.City = value; }
        }

        /// <summary>
        /// Изображение, соответствующее профилю.
        /// </summary>
        public byte[] Picture
        {
            get { return this.Entity.Picture; }
            set { this.Entity.Picture = value; }
        }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address
        {
            get { return this.Entity.Address; }
            set { this.Entity.Address = value; }
        }

        /// <summary>
        /// Тип профиля.
        /// </summary>
        public SkyProfileType Type
        {
            get { return this.Entity.Type; }
            set { this.Entity.Type = value; }
        }


        private bool __init_DataSets = false;
        private IEnumerable<SkyDataSet> _DataSets;
        /// <summary>
        /// Наборы данных, принадлежащие профилю.
        /// </summary>
        public IEnumerable<SkyDataSet> DataSets
        {
            get
            {
                if (!__init_DataSets)
                {
                    this.CheckExists();
                    _DataSets = this.Context.ObjectAdapters.DataSets.GetObjects(dbSet => dbSet
                        .Where(x => x.ProfileID == this.ID)
                        .OrderBy(x => x.ID));

                    __init_DataSets = true;
                }
                return _DataSets;
            }
        }

        internal void ResetDataSets()
        {
            this.__init_DataSets = false;
        }

        /// <summary>
        /// Создаёт новый экземпляр набора данных для использования в нейросети.
        /// </summary>
        public SkyDataSet CreateDataSet()
        {
            this.CheckExists();
            SkyDataSet dataSet = this.Context.ObjectAdapters.DataSets.CreateObject();
            dataSet.ProfileID = this.ID;
            return dataSet;
        }


        private bool __init_Networks = false;
        private IEnumerable<SkyNetwork> _Networks;
        /// <summary>
        /// Нейросети, принадлежащие профилю.
        /// </summary>
        public IEnumerable<SkyNetwork> Networks
        {
            get
            {
                if (!__init_Networks)
                {
                    this.CheckExists();
                    _Networks = this.Context.ObjectAdapters.Networks.GetObjects(dbSet => dbSet
                        .Where(x => x.ProfileID == this.ID)
                        .OrderBy(x => x.ID));

                    __init_Networks = true;
                }
                return _Networks;
            }
        }

        internal void ResetNetworks()
        {
            this.__init_Networks = false;
        }

        /// <summary>
        /// Создаёт новый экземпляр нейросети, без сохранения в базу данных.
        /// </summary>
        public SkyNetwork CreateNetwork()
        {
            this.CheckExists();
            SkyNetwork network = this.Context.ObjectAdapters.Networks.CreateObject();
            network.ProfileID = this.ID;
            return network;
        }


        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //проверяем наличие профиля.
            if (this.UserID == Guid.Empty)
                throw new Exception("UserID is undefined.");

            //проверяем отсутствие существующего профиля для пользователя.
            if (this.IsNew)
            {
                if (this.User.HasProfile)
                    throw new Exception(string.Format("Profile with UserID={0} is already exists.", this.UserID));
            }

            //обновляем объект.
            base.Update();

            //сбрасываем флаг инициализации профиля у пользователя.
            if (this.JustCreated)
                this.User.ResetProfile();
        }

        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            //удаляем датасеты.
            this.DeleteChildren(this.DataSets);

            //удаляем нейросети.
            this.DeleteChildren(this.Networks);

            //удаляем объект из базы данных.
            base.Delete();
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


        ISkyUser ISkyProfile.User => this.User;

        IEnumerable<ISkyDataSet> ISkyProfile.DataSets => this.DataSets;

        ISkyDataSet ISkyProfile.CreateDataSet()
        {
            return this.CreateDataSet();
        }

        IEnumerable<ISkyNetwork> ISkyProfile.Networks => this.Networks;

        ISkyNetwork ISkyProfile.CreateNetwork()
        {
            return this.CreateNetwork();
        }

    }
}
