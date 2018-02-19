using Skychain.Models.Entity;
using Storage.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skychain.Models.Implementation
{
    /// <summary>
    /// Представляет набор данных для обучения и тренировки сети.
    /// </summary>
    public class SkyDataSet : SkyObject<SkyDataSet, SkyDataSetEntity, ISkyDataSet>, ISkyDataSet
    {
        internal SkyDataSet(SkyDataSetEntity entity, SkyObjectAdapter<SkyDataSet, SkyDataSetEntity, ISkyDataSet> adapter)
            : base(entity, adapter)
        {

        }


        /// <summary>
        /// Название набора данных.
        /// </summary>
        public string Name
        {
            get { return this.Entity.Name; }
            set { this.Entity.Name = value; }
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
        /// Стоимость использования набора данных.
        /// </summary>
        public decimal Cost
        {
            get { return this.Entity.Cost; }
            set { this.Entity.Cost = value; }
        }

        /// <summary>
        /// Рейтинг данных.
        /// </summary>
        public double Rating
        {
            get { return this.Entity.Rating; }
            set { this.Entity.Rating = value; }
        }

        /// <summary>
        /// Описание набора данных.
        /// </summary>
        public string Description
        {
            get { return this.Entity.Description; }
            set { this.Entity.Description = value; }
        }

        /// <summary>
        /// Описание данных, хранящихся в наборе.
        /// </summary>
        public string DataDescription
        {
            get { return this.Entity.DataDescription; }
            set { this.Entity.DataDescription = value; }
        }


        private bool __init_TrainSet = false;
        private SkyFile _TrainSet;
        /// <summary>
        /// Тренировочный набор данных.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        public SkyFile TrainSet
        {
            get
            {
                if (!__init_TrainSet)
                {
                    _TrainSet = new SkyFile(this.Entity.TrainSetID, "DataSets", false, this.Context);
                    __init_TrainSet = true;
                }
                return _TrainSet;
            }
        }


        private bool __init_TestSet = false;
        private SkyFile _TestSet;
        /// <summary>
        /// Набор данных для тестирования нейросети.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        public SkyFile TestSet
        {
            get
            {
                if (!__init_TestSet)
                {
                    _TestSet = new SkyFile(this.Entity.TestSetID, "DataSets", false, this.Context);
                    __init_TestSet = true;
                }
                return _TestSet;
            }
        }


        private bool __init_SampleSet = false;
        private SkyFile _SampleSet;
        /// <summary>
        /// Образец набора данных.
        /// Свойство всегда возвращает существующий экземпляр класса.
        /// </summary>
        public SkyFile SampleSet
        {
            get
            {
                if (!__init_SampleSet)
                {
                    _SampleSet = new SkyFile(this.Entity.SampleSetID, "DataSets", false, this.Context);
                    __init_SampleSet = true;
                }
                return _SampleSet;
            }
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
                throw new Exception(string.Format("DataSet [{0}] is not published.", this.Name));
        }

        /// <summary>
        /// Обновляет объект в базе данных.
        /// </summary>
        public override void Update()
        {
            //проверяем наличие профиля.
            this.Profile.CheckExists();

            //проставляем ссылки на файлы.
            if (this.__init_TrainSet)
                this.Entity.TrainSetID = this.TrainSet.ID;

            if (this.__init_TestSet)
                this.Entity.TestSetID = this.TestSet.ID;

            if (this.__init_SampleSet)
                this.Entity.SampleSetID = this.SampleSet.ID;

            //обновляем объект.
            base.Update();

            //сбрасываем флаг инициализации коллекции датасетов у профиля.
            if (this.JustCreated)
                this.Profile.ResetDataSets();
        }


        /// <summary>
        /// Удаляет объект и все связанные с ним дочерние объекты.
        /// </summary>
        public override void Delete()
        {
            //удаляем наборы данных.
            using (OperationContext deletingContext = this.Context.RunDeletingContext())
            {
                if (this.TrainSet.Exists)
                    this.TrainSet.Delete();
                if (this.TestSet.Exists)
                    this.TestSet.Delete();
                if (this.SampleSet.Exists)
                    this.SampleSet.Delete();
            }

            //удаляем объект из базы данных.
            base.Delete();

            //сбрасываем флаг инициализации коллекции датасетов у профиля.
            this.Profile.ResetDataSets();
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


        ISkyProfile ISkyDataSet.Profile => this.Profile;

        ISkyFile ISkyDataSet.TrainSet => this.TrainSet;

        ISkyFile ISkyDataSet.TestSet => this.TestSet;

        ISkyFile ISkyDataSet.SampleSet => this.SampleSet;
    }
}
