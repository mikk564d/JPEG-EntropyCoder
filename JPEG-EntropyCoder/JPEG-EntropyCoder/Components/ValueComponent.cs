using System;
using System.Collections;

namespace JPEG_EntropyCoder.Components {
    abstract class ValueComponent : EntropyComponent{
        protected ValueComponent(BitArray huffmanTreePath, byte huffmanLeafHexValue, BitArray amplitude)
            : base(huffmanTreePath, huffmanLeafHexValue) {
            Amplitude = amplitude;
        }

        public BitArray Amplitude { get; set; }

        public int LSB {
            get { return Amplitude[Amplitude.Length] ? 1 : 0; }

            set { Amplitude[Amplitude.Length] = value == 1; }
        }

        /// <summary>
        /// Gets Decimal Value.
        /// </summary>
        //public int getDecimalValue() {
        //    int decimalValue;

        //    if (Amplitude[0] == '0') {
        //        decimalValue = -((int) Math.Pow(2, Amplitude.Length) - (Convert.ToInt32(Amplitude, 2) + 1));
        //    }
        //    else {
        //        decimalValue = Convert.ToInt32(Amplitude, 2);
        //    }

        //    return decimalValue;
        //}
    }
}
