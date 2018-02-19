using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.MSSQL
{
    /// <summary>
    /// Константы модуля работы с метаданными.
    /// </summary>
    public class MetadataConsts
    {
        /// <summary>
        /// Таймаут выполнения SQL команды по умолчанию.
        /// </summary>
        public const int CommandTimeout = 72000;

        /// <summary>
        /// Sql-контексты выполнения триггеров.
        /// </summary>
        public static class TriggerContexts
        {
            /// <summary>
            /// Контекст дизэйбла триггеров.
            /// CTX.DTR
            /// </summary>
            public static class DisableTriggers
            {
                //Двоичный код контекста.
                public const string BinaryCode = "0x4354582E445452";

                //Название контекста.
                public const string Name = "CTX.DTR";
            }

            /// <summary>
            /// Контекст дизэйбла всех триггеров.
            /// CTX.DTRA
            /// </summary>
            public static class DisableAllTriggers
            {
                //Двоичный код контекста.
                public const string BinaryCode = "0x4354582E44545241";

                //Название контекста.
                public const string Name = "CTX.DTRA";
            }
        }

        /// <summary>
        /// Scope-ы для логирования.
        /// </summary>
        public static class Scopes
        {
            /// <summary>
            /// Scope работы с метаданными.
            /// </summary>
            public const string MetadataAdapter = "MSSQL.MetadataAdapter";

            /// <summary>
            /// Scope создания/обновления распределенных таблиц.
            /// </summary>
            public const string TableActivator = "MSSQL.TableActivator";

            /// <summary>
            /// Scope работы с типами метаданных.
            /// </summary>
            public const string TypeProvider = "MSSQL.TypeProvider";

            /// <summary>
            /// Scope работы со схемой таблиц в БД.
            /// </summary>
            public const string DBSchema = "MSSQL.DBSchema";

            /// <summary>
            /// Scope работы с токенами.
            /// </summary>
            public const string TokenAdapter = "MSSQL.TokenAdapter";
        }

        /// <summary>
        /// Названия индексов по умолчанию.
        /// </summary>
        public static class DefaultIndexNames
        {
            /// <summary>
            /// Название индекска первичного ключа.
            /// </summary>
            public const string PrimaryKey = "PrimaryKey";
        }

        public class Replication
        {
            public const int BatchSize = 1000;
        }
    }
}
