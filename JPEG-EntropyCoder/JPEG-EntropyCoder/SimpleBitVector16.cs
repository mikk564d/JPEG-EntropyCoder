using System;

namespace JPEG_EntropyCoder {
    public class SimpleBitVector16 : ICloneable{
        private short _data;
        public byte Length { get; set; }

        public SimpleBitVector16() : this(0, 0)
        { }

        public SimpleBitVector16(short data, byte length) {
            _data = data;
            Length = length;
        }

        public SimpleBitVector16(bool[] data) {
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
            return (_data & (1 << index)) != 0;
        }

        public void Set(byte index, bool value) {
            if (value) {
                _data |= (short)(1 << index);
            } else {
                _data &= (short)~(1 << index);
            }
        }

        protected bool Equals(SimpleBitVector16 other) {
            return _data == other._data && Length == other.Length;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SimpleBitVector16) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (_data.GetHashCode()*397) ^ Length.GetHashCode();
            }
        }

        public object Clone() {
            return new SimpleBitVector16(_data, Length);
        }
    }
}
