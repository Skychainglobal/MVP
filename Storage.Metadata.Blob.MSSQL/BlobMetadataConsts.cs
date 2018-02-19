using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Metadata.Blob.MSSQL
{
    public class BlobMetadataConsts
    {
        /// <summary>
        /// Scope-ы для логирования.
        /// </summary>
        public static class Scopes
        {
            /// <summary>
            /// Scope работы с метаданными blob и файлов.
            /// </summary>
            public const string BlobMetadataAdapter = "Blob.MSSQL.BlobMetadataAdapter";

            /// <summary>
            /// Scope работы с токенами файлов.
            /// </summary>
            public const string FileTokenAdapter = "Blob.MSSQL.FileTokenAdapter";
        }
    }
}
