using System;

namespace JPEG_EntropyCoder {
    public class BitVector16 : ICloneable{
        private short Data { get; set; }
        public byte Length { get; set; }

        public BitVector16() : this(0, 0)
        { }

        public BitVector16(short data, byte length) {
            Data = data;
            Length = length;
        }

        public BitVector16(bool[] data) {
            byte i;
            for (i = 0; i < data.Length && i < 16; i++) {
                Set(i, data[i]);
            }

            Length = i;
        }

        public bool this[byte index] {
            get {
                return Get(index);
            }
            set {
                Set(index, value);
            }
        }

        public bool Get(byte index) {
            return (Data & (1 << index)) != 0;
        }

        public void Set(byte index, bool value) {
            if (value) {
                Data |= (short)(1 << index);
            } else {
                Data &= (short)~(1 << index);
            }
        }

        protected bool Equals(BitVector16 other) {
            return Data == other.Data && Length == other.Length;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BitVector16) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (Data.GetHashCode()*397) ^ Length.GetHashCode();
            }
        }

        public object Clone() {
            return new BitVector16(Data, Length);
        }
    }
}
