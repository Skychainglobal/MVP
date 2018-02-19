using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Storage.Data.Blob
{
    /// <summary>
    /// Класс представляющий собой поток единичного файла в блобе.
    /// Необходим для потоковой передачи. В случае потоковой передачи считывается не весь файл блоба, 
    /// а только часть байтов самого файла.
    /// </summary>
    internal class PartitionStream : Stream
    {
        internal PartitionStream(Stream baseStream, long startPosition, long length)
        {
            if (baseStream == null)
                throw new ArgumentNullException("baseStream");

            if (startPosition < 0)
                throw new ArgumentNullException("startPosition");

            this.BaseStream = baseStream;
            this.StartPosition = startPosition;
            this.DataLength = length;

            //установка позиции на начало файла в блобе
            if (this.BaseStream.Position != startPosition)
                this.BaseStream.Seek(startPosition, SeekOrigin.Begin);
        }

        /// <summary>
        /// Общее кол-во считанных байт.
        /// </summary>
        public long TotalBytesRead { get; private set; }

        /// <summary>
        /// Базовый поток.
        /// </summary>
        internal Stream BaseStream { get; private set; }

        /// <summary>
        /// Начальная позиция для чтения части потока.
        /// </summary>
        public long StartPosition { get; private set; }

        /// <summary>
        /// Размер данных для чтения.
        /// </summary>
        public long DataLength { get; private set; }

        public override bool CanRead
        {
            get { return this.BaseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            this.ThrowNotSupportedException();
        }

        public override long Length
        {
            get { this.ThrowNotSupportedException(); return 0; }
        }

        public override long Position
        {
            get
            {
                return this.BaseStream.Position;
            }
            set
            {
                this.ThrowNotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //если уже считали все данные файла из блоба, то дальше не читаем
            if (this.TotalBytesRead == this.DataLength)
                return 0;

            int countRead = this.BaseStream.Read(buffer, offset, count);
            if (countRead > 0)
            {
                long totalRead = this.TotalBytesRead + countRead;
                if (totalRead > this.DataLength)
                {
                    if (this.TotalBytesRead > 0)
                    {
                        //в последнюю итерацию чтения количество считанных байт вместе
                        //с количеством ранее считанных байт превысило общий размер файла
                        //=> обрезаем количество считанных байт за последнюю итерацию
                        countRead = (int)(this.DataLength - this.TotalBytesRead);
                    }
                    else
                    {
                        //сразу за первое считывание превысили размер файла
                        //=> количество урезаем до размера файла
                        countRead = (int)this.DataLength;
                    }
                }

                this.TotalBytesRead += countRead;
            }

            return countRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.ThrowNotSupportedException();
            return 0;
        }

        public override void SetLength(long value)
        {
            this.ThrowNotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.ThrowNotSupportedException();
        }

        public override void Close()
        {
            this.BaseStream.Close();
            base.Close();
        }
        protected override void Dispose(bool disposing)
        {
            this.BaseStream.Dispose();
            base.Dispose(disposing);
        }

        private void ThrowNotSupportedException()
        {
            throw new Exception("Поток предназначен только для буферного чтения с заданной позиции.");
        }
    }
}