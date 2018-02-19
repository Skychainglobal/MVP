using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Класс для работы с единичным файлом в блобе.
    /// </summary>
    internal class FileStructure
    {
        /*
        
         Схема единичного файла:
         
         [SystemHeader][FileHeader][Content]
            - [SystemHeader] = [{FixBytes}{FileHeaderSize}{HeaderHash}{FileContentHash}{HeaderVersion}{ContentLength}{92}]
            - [FileHeader] = [JsonHeaderString]
            - [Content] = byte[];
         
        */

        /// <summary>
        /// Возвращает абсолютную позицию байтов содержимого файла в системном заголовке.
        /// </summary>
        /// <param name="offset">Позиция начала файла относительно всего блоба.</param>
        /// <returns></returns>
        public static long GetContentHashBytesAbsolutePosition(long offset = 0)
        {
            /*хеш содержимого 2ой после хеша заголовка, поэтому учитываем позицию только одного хеша*/
            long contentHashBytesAbsolutePosition = offset + BlobStreamAdapter.SystemHeaderFixedBytes.Length
                    + BlobConsts.BlobFile.HeaderSizeBytesLength + (BlobConsts.BlobFile.AllHashBytesLength / 2);

            return contentHashBytesAbsolutePosition;
        }

        /// <summary>
        /// Возвращает абсолютную позицию байтов размера содержимого файла в системном заголовке.
        /// </summary>
        /// <param name="offset">Позиция начала файла относительно всего блоба.</param>
        /// <returns></returns>
        public static long GetContentLengBytesAbsolutePosition(long offset = 0)
        {
            /*хеш содержимого 2ой после хеша заголовка, поэтому учитываем позицию только одного хеша*/
            long contentLengBytesAbsolutePosition = offset + BlobStreamAdapter.SystemHeaderFixedBytes.Length
                    + BlobConsts.BlobFile.HeaderSizeBytesLength + BlobConsts.BlobFile.AllHashBytesLength
                    + BlobConsts.BlobFile.HeaderVersionBytesLength;

            return contentLengBytesAbsolutePosition;
        }
    }
}