using System;
using System.Collections;

namespace JPEG_EntropyCoder.Components {
    public abstract class EntropyValueComponent : EntropyComponent{

        public SimpleBitVector16 Amplitude { get; set; }

        public bool LSB {
            get { return Amplitude[(byte)(Amplitude.Length - 1)]; }
            set { Amplitude[(byte)(Amplitude.Length - 1)] = value ; }
        }

        protected EntropyValueComponent(SimpleBitVector16 huffmanTreePath, byte huffmanLeafHexValue, SimpleBitVector16 amplitude)
            : base(huffmanTreePath, huffmanLeafHexValue) {
            Amplitude = amplitude;
        }
    }
}
