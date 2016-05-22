using System;
using System.Collections;

namespace JPEG_EntropyCoder.Components {
    public abstract class EntropyValueComponent : EntropyComponent{

        public BitVector16 Amplitude { get; set; }

        public bool LSB {
            get { return Amplitude[(byte)(Amplitude.Length - 1)]; }
            set { Amplitude[(byte)(Amplitude.Length - 1)] = value ; }
        }

        protected EntropyValueComponent(BitVector16 huffmanTreePath, byte huffmanLeafHexValue, BitVector16 amplitude)
            : base(huffmanTreePath, huffmanLeafHexValue) {
            Amplitude = amplitude;
        }
    }
}
