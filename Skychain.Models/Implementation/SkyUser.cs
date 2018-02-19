using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет пользователя системы.
    /// </summary>
    public class SkyUser : ISkyUser
    {
        internal SkyUser(Guid id, SkyContext context)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException("id");
            if (context == null)
                throw new ArgumentNullException("context");

            this.ID = id;
            this.Context = context;
        }

        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public Guid ID { get; private set; }

        /// <summary>
        /// Контекст системы.
        /// </summary>
        public SkyContext Context { get; private set; }


        private bool __init_ProfileInternal = false;
        private SkyProfile _ProfileInternal;
        /// <summary>
        /// Профиль пользователя.
        /// </summary>
        private SkyProfile ProfileInternal
        {
            get
            {
                if (!__init_ProfileInternal)
                {
                    _ProfileInternal = this.Context.ObjectAdapters.Profiles.GetObject(dbSet => dbSet.FirstOrDefault(x => x.UserID == this.ID));
                    __init_ProfileInternal = true;
                }
                return _ProfileInternal;
            }
        }
        internal void ResetProfile()
        {
            this.__init_ProfileInternal = false;
        }

        /// <summary>
        /// Профиль пользователя.
        /// Генерирует исключение в случае отсутствия профиля.
        /// </summary>
        public SkyProfile Profile
        {
            get
            {
                this.CheckProfile();
                return this.ProfileInternal;
            }
        }

        /// <summary>
        /// Возвращает true, если для пользователя создан профиль.
        /// </summary>
        public bool HasProfile
        {
            get { return this.ProfileInternal != null; }
        }

        /// <summary>
        /// Проверяет наличие профиля.
        /// Генерирует исключение в случае отсутствия профиля.
        /// </summary>
        public void CheckProfile()
        {
            if (this.ProfileInternal == null)
                throw new Exception(string.Format("No profile for user with ID={0}", this.ID));
        }

        /// <summary>
        /// Создаёт новый экземпляр профиля, соответствующего пользователю системы, без сохранения в базу данных.
        /// </summary>
        public SkyProfile CreateProfile()
        {
            SkyProfile profile = this.Context.ObjectAdapters.Profiles.CreateObject();
            profile.UserID = this.ID;
            return profile;
        }


        private bool __init_RecentRequests = false;
        private IEnumerable<SkyNetworkRequest> _RecentRequests;
        /// <summary>
        /// Запросы пользователей к нейросетям.
        /// </summary>
        public IEnumerable<SkyNetworkRequest> RecentRequests
        {
            get
            {
                if (!__init_RecentRequests)
                {
                    _RecentRequests = this.Context.ObjectAdapters.NetworkRequests.GetObjects(dbSet => dbSet
                        .Where(x => x.UserID == this.ID)
                        .OrderByDescending(x => x.ID));

                    __init_RecentRequests = true;
                }
                return _RecentRequests;
            }
        }


        /// <summary>
        /// Текстовое представление экземпляра класса.
        /// </summary>
        public override string ToString()
        {
            if (this.ID != Guid.Empty)
                return this.ID.ToString();
            return base.ToString();
        }


        ISkyContext ISkyUser.Context => this.Context;

        ISkyProfile ISkyUser.Profile => this.Profile;

        void ISkyUser.CheckProfile()
        {
            this.CheckProfile();
        }

        ISkyProfile ISkyUser.CreateProfile()
        {
            return this.CreateProfile();
        }

        IEnumerable<ISkyNetworkRequest> ISkyUser.RecentRequests => this.RecentRequests;
    }
}
