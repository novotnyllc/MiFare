using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    /// <summary>
    ///     Handle access condition for a generic datablock of a sector
    /// </summary>
    public class DataAreaAccessCondition : IEquatable<DataAreaAccessCondition>
    {
        /// <summary>
        ///     List of access conditions that may apply to each operation (read, write, inc, dec)
        /// </summary>
        public enum ConditionEnum
        {
            Never,
            KeyA,
            KeyB,
            KeyAOrB
        }

        /// <summary>
        ///     Dictionary that associate an AccessConditionsSet to a bit array of C1-C2-C3 bits
        ///     (see MiFare specs for the meaning of C1-C2-C3)
        /// </summary>
        private static Dictionary<DataAreaAccessCondition, BitArray> _Templates;

        public DataAreaAccessCondition()
        {
            Read = ConditionEnum.KeyAOrB;
            Write = ConditionEnum.KeyAOrB;
            Increment = ConditionEnum.KeyAOrB;
            Decrement = ConditionEnum.KeyAOrB;
        }

        /// Access condition for decrement operations on the data block
        public ConditionEnum Decrement { get; set; }

        /// Access condition for increment operations on the data block
        public ConditionEnum Increment { get; set; }

        /// <summary>
        ///     Access condition for read operations on the data block
        /// </summary>
        public ConditionEnum Read { get; set; }

        /// <summary>
        ///     Access condition for write operations on the data block
        /// </summary>
        public ConditionEnum Write { get; set; }

        public bool Equals(DataAreaAccessCondition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Read == other.Read && Write == other.Write && Increment == other.Increment && Decrement == other.Decrement;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DataAreaAccessCondition)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)Read;
                hashCode = (hashCode*397) ^ (int)Write;
                hashCode = (hashCode*397) ^ (int)Increment;
                hashCode = (hashCode*397) ^ (int)Decrement;
                return hashCode;
            }
        }

        public override string ToString()
        {
            var bits = GetBits();
            if (bits == null)
                return "Invalid";

            return bits.ToString();
        }

        /// <summary>
        ///     convert the object to the corresponding C1-C2-C3 bits
        /// </summary>
        /// <returns>a 3-elements bit array</returns>
        internal BitArray GetBits()
        {
            InitTemplates();

            foreach (var kvp in _Templates)
            {
                if (kvp.Key.Equals(this))
                    return kvp.Value;
            }

            return _Templates.ElementAt(0)
                             .Value;
        }

        /// <summary>
        ///     Initialize the object based on a DataAreaAccessCondition
        /// </summary>
        /// <param name="access">the DataAreaAccessCondition to clone</param>
        internal void Initialize(DataAreaAccessCondition access)
        {
            Read = access.Read;
            Write = access.Write;
            Increment = access.Increment;
            Decrement = access.Decrement;
        }

        /// <summary>
        ///     Initialize object based on a bit array of C1-C2-C3
        /// </summary>
        /// <param name="bits">C1-C2-C3 bit array</param>
        /// <returns></returns>
        internal bool Initialize(BitArray bits)
        {
            InitTemplates();

            foreach (var kvp in _Templates)
            {
                if (kvp.Value.IsEqual(bits))
                {
                    Initialize(kvp.Key);
                    return true;
                }
            }

            return false;
        }

        private void InitTemplates()
        {
            if (_Templates != null)
                return;

            _Templates = new Dictionary<DataAreaAccessCondition, BitArray>
            {
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.KeyAOrB,
                        Write = ConditionEnum.KeyAOrB,
                        Increment = ConditionEnum.KeyAOrB,
                        Decrement = ConditionEnum.KeyAOrB
                    },
                    new BitArray(new[] {false, false, false})
                },
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.KeyAOrB,
                        Write = ConditionEnum.Never,
                        Increment = ConditionEnum.Never,
                        Decrement = ConditionEnum.Never
                    },
                    new BitArray(new[] {false, true, false})
                },
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.KeyAOrB,
                        Write = ConditionEnum.KeyB,
                        Increment = ConditionEnum.Never,
                        Decrement = ConditionEnum.Never
                    },
                    new BitArray(new[] {true, false, false})
                },
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.KeyAOrB,
                        Write = ConditionEnum.KeyB,
                        Increment = ConditionEnum.KeyB,
                        Decrement = ConditionEnum.KeyAOrB
                    },
                    new BitArray(new[] {true, true, false})
                },
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.KeyAOrB,
                        Write = ConditionEnum.Never,
                        Increment = ConditionEnum.Never,
                        Decrement = ConditionEnum.KeyAOrB
                    },
                    new BitArray(new[] {false, false, true})
                },
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.KeyB,
                        Write = ConditionEnum.KeyB,
                        Increment = ConditionEnum.Never,
                        Decrement = ConditionEnum.Never
                    },
                    new BitArray(new[] {false, true, true})
                },
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.KeyB,
                        Write = ConditionEnum.Never,
                        Increment = ConditionEnum.Never,
                        Decrement = ConditionEnum.Never
                    },
                    new BitArray(new[] {true, false, true})
                },
                {
                    new DataAreaAccessCondition
                    {
                        Read = ConditionEnum.Never,
                        Write = ConditionEnum.Never,
                        Increment = ConditionEnum.Never,
                        Decrement = ConditionEnum.Never
                    },
                    new BitArray(new[] {true, true, true})
                }
            };








        }
    }
}