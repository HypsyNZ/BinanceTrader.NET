//******************************************************************************************************
//  Copyright © 2022, S. Christison. No Rights Reserved.
//
//  Licensed to [You] under one or more License Agreements.
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//
//******************************************************************************************************

using System.Runtime.InteropServices;

namespace BTNET.BVVM.HELPERS
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DecimalHelper
    {
        private const byte k_SignBit = 1 << 7;

        [FieldOffset(0)]
        public decimal Value;

        [FieldOffset(0)]
        public readonly uint Flags;

        [FieldOffset(0)]
        public readonly ushort Reserved;

        [FieldOffset(2)]
        private byte m_Scale;

        public byte Scale
        {
            get => m_Scale;
            set
            {
                if (value > 28)
                    throw new System.ArgumentOutOfRangeException("value", "Scale can't be bigger than 28!");
                m_Scale = value;
            }
        }

        [FieldOffset(3)]
        private byte m_SignByte;

        public int Sign => m_SignByte > 0 ? -1 : 1;

        public bool Positive
        {
            get => (m_SignByte & k_SignBit) > 0;
            set => m_SignByte = value ? (byte)0 : k_SignBit;
        }

        [FieldOffset(4)]
        public uint Hi;

        [FieldOffset(8)]
        public uint Lo;

        [FieldOffset(12)]
        public uint Mid;

        public DecimalHelper(decimal value) : this()
        {
            Value = value;
        }

        public static implicit operator DecimalHelper(decimal value)
        {
            return new DecimalHelper(value);
        }

        public static implicit operator decimal(DecimalHelper value)
        {
            return value.Value;
        }
    }
}