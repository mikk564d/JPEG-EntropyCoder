using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Components {
    public class DCComponent : EntropyValueComponent {
        public DCComponent(BitVector16 huffmanTreePath, byte huffmanLeafHexValue, BitVector16 amplitude) 
            : base(huffmanTreePath, huffmanLeafHexValue, amplitude) { }
    }
}
