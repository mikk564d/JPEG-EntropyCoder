using System;

namespace JPEG_EntropyCoder.Components {
    abstract class ValueComponent : EntropyComponent{
        protected ValueComponent(string huffmanTreePath, string huffmanLeafHexValue, string amplitude) 
            : base(huffmanTreePath, huffmanLeafHexValue, amplitude) {}

        public int LSB {
            get { return int.Parse(Amplitude.Substring(Amplitude.Length - 1)); }

            set { Amplitude = Amplitude.Remove(Amplitude.Length - 1, 1) + value.ToString(); }
        }

        /// <summary>
        /// Gets Decimal Value.
        /// </summary>
        public int getDecimalValue() {
            int decimalValue;

            if (Amplitude[0] == '0') {
                decimalValue = -((int) Math.Pow(2, Amplitude.Length) - (Convert.ToInt32(Amplitude, 2) + 1));
            }
            else {
                decimalValue = Convert.ToInt32(Amplitude, 2);
            }

            return decimalValue;
        }
    }
}
