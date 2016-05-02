using System;
using System.Collections;

namespace JPEG_EntropyCoder.Components {
    public abstract class EntropyValueComponent : EntropyComponent{

        public BitArray Amplitude { get; set; }

        public int LSB {
            get { return Amplitude[Amplitude.Length] ? 1 : 0; }
            set { Amplitude[Amplitude.Length] = value == 1; }
        }

        protected EntropyValueComponent(BitArray huffmanTreePath, byte huffmanLeafHexValue, BitArray amplitude)
            : base(huffmanTreePath, huffmanLeafHexValue) {
            Amplitude = amplitude;
        }
    }
}
