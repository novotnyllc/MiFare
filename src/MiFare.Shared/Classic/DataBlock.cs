using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    /// <summary>
    ///     Class that handles the datablocks in a sector
    /// </summary>
    internal class DataBlock
    {
        public const int Length = 16;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private byte[] data;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private byte[] origData;

        public DataBlock(int number, byte[] data)
        {
            this.Number = number;
            this.data = data;
            IsTrailer = false;

            origData = new byte[this.data.Length];
            Array.Copy(this.data, origData, this.data.Length);
        }

        public DataBlock(int number, byte[] data, bool isTrailer)
        {
            Number = number;
            this.data = data;
            IsTrailer = isTrailer;

            origData = new byte[this.data.Length];
            Array.Copy(this.data, origData, this.data.Length);
        }

        public int Number { get; }

        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public byte[] Data => data;

        public bool IsTrailer { get; }

        public bool IsChanged => (!data.Equals(origData));
    }
}