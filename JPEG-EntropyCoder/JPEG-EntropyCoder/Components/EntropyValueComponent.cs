using System;
using System.Collections;

namespace JPEG_EntropyCoder.Components {
    public abstract class EntropyValueComponent : EntropyComponent{

        public BitArray Amplitude { get; set; }

        public bool LSB {
            get { return Amplitude[Amplitude.Length]; }
            set { Amplitude[Amplitude.Length] = value ; }
        }

        protected EntropyValueComponent(BitArray huffmanTreePath, byte huffmanLeafHexValue, BitArray amplitude)
            : base(huffmanTreePath, huffmanLeafHexValue) {
            Amplitude = amplitude;
        }
    }
}
