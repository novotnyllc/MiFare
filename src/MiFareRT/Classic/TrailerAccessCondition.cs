using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiFare.Classic
{
    public class TrailerAccessCondition : IEquatable<TrailerAccessCondition>
    {
        public enum ConditionEnum
        {
            Never,
            KeyA,
            KeyB,
            KeyAOrB
        }

        private static Dictionary<TrailerAccessCondition, BitArray> templates;
        public ConditionEnum AccessBitsRead { get; set; }
        public ConditionEnum AccessBitsWrite { get; set; }
        public ConditionEnum KeyARead { get; set; }
        public ConditionEnum KeyAWrite { get; set; }
        public ConditionEnum KeyBRead { get; set; }
        public ConditionEnum KeyBWrite { get; set; }

        public bool Equals(TrailerAccessCondition other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return KeyARead == other.KeyARead &&
                   KeyAWrite == other.KeyAWrite &&
                   KeyBRead == other.KeyBRead &&
                   KeyBWrite == other.KeyBWrite &&
                   AccessBitsRead == other.AccessBitsRead &&
                   AccessBitsWrite == other.AccessBitsWrite;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TrailerAccessCondition)obj);
        }

        public BitArray GetBits()
        {
            InitTemplates();

            foreach (var kvp in templates)
            {
                if (kvp.Key.Equals(this))
                    return kvp.Value;
            }

            return templates.ElementAt(4)
                             .Value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)KeyARead;
                hashCode = (hashCode*397) ^ (int)KeyAWrite;
                hashCode = (hashCode*397) ^ (int)KeyBRead;
                hashCode = (hashCode*397) ^ (int)KeyBWrite;
                hashCode = (hashCode*397) ^ (int)AccessBitsRead;
                hashCode = (hashCode*397) ^ (int)AccessBitsWrite;
                return hashCode;
            }
        }

        public void Initialize(TrailerAccessCondition access)
        {
            KeyARead = access.KeyARead;
            KeyAWrite = access.KeyAWrite;
            KeyBRead = access.KeyBRead;
            KeyBWrite = access.KeyBWrite;
            AccessBitsRead = access.AccessBitsRead;
            AccessBitsWrite = access.AccessBitsWrite;
        }

        public bool Initialize(BitArray bits)
        {
            InitTemplates();

            foreach (var kvp in templates)
            {
                if (kvp.Value.IsEqual(bits))
                {
                    Initialize(kvp.Key);
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            var bits = GetBits();
            if (bits == null)
                return "Invalid";

            return bits.ToString();
        }

        private void InitTemplates()
        {
            if (templates != null)
                return;

            templates = new Dictionary<TrailerAccessCondition, BitArray>
            {
                {
                    new TrailerAccessCondition
                    {
                        KeyARead = ConditionEnum.Never,
                        KeyAWrite = ConditionEnum.KeyA,
                        AccessBitsRead = ConditionEnum.KeyA,
                        AccessBitsWrite = ConditionEnum.Never,
                        KeyBRead = ConditionEnum.KeyA,
                        KeyBWrite = ConditionEnum.KeyA
                    },
                    new BitArray(new[] {false, false, false})
                },
                {
                    new TrailerAccessCondition
                    {
                        KeyARead = ConditionEnum.Never,
                        KeyAWrite = ConditionEnum.Never,
                        AccessBitsRead = ConditionEnum.KeyA,
                        AccessBitsWrite = ConditionEnum.Never,
                        KeyBRead = ConditionEnum.KeyA,
                        KeyBWrite = ConditionEnum.Never
                    },
                    new BitArray(new[] {false, true, false})
                },
                {
                    new TrailerAccessCondition
                    {
                        KeyARead = ConditionEnum.Never,
                        KeyAWrite = ConditionEnum.KeyB,
                        AccessBitsRead = ConditionEnum.KeyAOrB,
                        AccessBitsWrite = ConditionEnum.Never,
                        KeyBRead = ConditionEnum.Never,
                        KeyBWrite = ConditionEnum.KeyB
                    },
                    new BitArray(new[] {true, false, false})
                },
                {
                    new TrailerAccessCondition
                    {
                        KeyARead = ConditionEnum.Never,
                        KeyAWrite = ConditionEnum.KeyA,
                        AccessBitsRead = ConditionEnum.KeyA,
                        AccessBitsWrite = ConditionEnum.KeyA,
                        KeyBRead = ConditionEnum.KeyA,
                        KeyBWrite = ConditionEnum.KeyA
                    },
                    new BitArray(new[] {false, false, true})
                },
                {
                    new TrailerAccessCondition
                    {
                        KeyARead = ConditionEnum.Never,
                        KeyAWrite = ConditionEnum.KeyB,
                        AccessBitsRead = ConditionEnum.KeyAOrB,
                        AccessBitsWrite = ConditionEnum.KeyB,
                        KeyBRead = ConditionEnum.Never,
                        KeyBWrite = ConditionEnum.KeyB
                    },
                    new BitArray(new[] {false, true, true})
                },
                {
                    new TrailerAccessCondition
                    {
                        KeyARead = ConditionEnum.Never,
                        KeyAWrite = ConditionEnum.Never,
                        AccessBitsRead = ConditionEnum.KeyAOrB,
                        AccessBitsWrite = ConditionEnum.KeyB,
                        KeyBRead = ConditionEnum.Never,
                        KeyBWrite = ConditionEnum.Never
                    },
                    new BitArray(new[] {true, false, true})
                },
                {
                    new TrailerAccessCondition
                    {
                        KeyARead = ConditionEnum.Never,
                        KeyAWrite = ConditionEnum.Never,
                        AccessBitsRead = ConditionEnum.KeyAOrB,
                        AccessBitsWrite = ConditionEnum.Never,
                        KeyBRead = ConditionEnum.Never,
                        KeyBWrite = ConditionEnum.Never
                    },
                    new BitArray(new[] {true, true, true})
                }
            };


            // This is commented out as there are two sections defined
            // with the same conditions but different access bits in the spec
            // See 8.7.2 in MF1S50YYX.pdf
            // It's unclear if 1 1 0 and 1 1 1 have any difference 

            //_Templates.Add(new TrailerAccessCondition()
            //{
            //    KeyARead = ConditionEnum.Never,
            //    KeyAWrite = ConditionEnum.Never,
            //    AccessBitsRead = ConditionEnum.KeyAOrB,
            //    AccessBitsWrite = ConditionEnum.Never,
            //    KeyBRead = ConditionEnum.Never,
            //    KeyBWrite = ConditionEnum.Never
            //},
            //               new BitArray(new bool[] {true, true, false}));
        }
    }
}