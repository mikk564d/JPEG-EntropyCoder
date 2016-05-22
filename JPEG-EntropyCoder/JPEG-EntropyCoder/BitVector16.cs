using System;

namespace JPEG_EntropyCoder {
    /// <summary>
    /// A small class to contain up to 16 bits. The length is specified by a properpty.
    /// </summary>
    public class BitVector16 : ICloneable{
        /// <summary>
        /// The property which contains the bits.
        /// </summary>
        private short Data { get; set; }
        /// <summary>
        /// The number of bits in the bit array.
        /// </summary>
        public byte Length { get; set; }

        /// <summary>
        /// Chain constructor to <see cref="BitVector16(short, byte)"/> with 0, 0 as parameter
        /// </summary>
        public BitVector16() : this(0, 0)
        { }

        /// <summary>
        /// Initialize the <see cref="BitVector16"/> with the specified
        /// <paramref name="data"/> as the bit sequence with the specified <paramref name="length"/>.
        /// </summary>
        /// <param name="data">Data to set.</param>
        /// <param name="length">Self specified length.</param>
        public BitVector16(short data, byte length) {
            Data = data;
            Length = length;
        }

        /// <summary>
        /// Initialize the <see cref="BitVector16"/> with the specified 
        /// <paramref name="data"/>. Each <see cref="bool"/> is a bit in the vector.
        /// The <see cref="Length"/> is set to the number of <see cref="bool"/>s in the
        /// <paramref name="data"/> (maximum 16).
        /// </summary>
        /// <param name="data">Input bits to initialize it with.</param>
        public BitVector16(bool[] data) {
            byte i;
            for (i = 0; i < data.Length && i < 16; i++) {
                Set(i, data[i]);
            }

            Length = i;
        }

        /// <summary>
        /// Implements the [] operator to get and set
        /// bits in the vector.
        /// </summary>
        /// <param name="index">The index of the vector to get and set.</param>
        /// <returns>A <see cref="bool"/> value at the specified <paramref name="index"/>.</returns>
        public bool this[byte index] {
            get {
                return Get(index);
            }
            set {
                Set(index, value);
            }
        }

        /// <summary>
        /// Gets a <see cref="bool"/> value at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the vector to get.</param>
        /// <returns>A <see cref="bool"/> value at the specified <paramref name="index"/>.</returns>
        public bool Get(byte index) {
            return (Data & (1 << index)) != 0;
        }

        /// <summary>
        /// Sets a <see cref="bool"/> value at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the vector to set.</param>
        /// <param name="value">The new value to set.</param>
        public void Set(byte index, bool value) {
            if (value) {
                Data |= (short)(1 << index);
            } else {
                Data &= (short)~(1 << index);
            }
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equals to <paramref name="other"/>
        /// </summary>
        /// <param name="other">Object to compare</param>
        /// <returns>Returns true if equals</returns>
        protected bool Equals(BitVector16 other) {
            return Data == other.Data && Length == other.Length;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <filterpriority>2</filterpriority>
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BitVector16) obj);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode() {
            unchecked {
                return (Data.GetHashCode()*397) ^ Length.GetHashCode();
            }
        }

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        /// <filterpriority>2</filterpriority>
        public object Clone() {
            return new BitVector16(Data, Length);
        }
    }
}
