using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Components {
    class DCComponent : ValueComponent {
        public DCComponent(BitArray huffmanTreePath, byte huffmanLeafHexValue, BitArray amplitude) 
            : base(huffmanTreePath, huffmanLeafHexValue, amplitude) { }
    }
}
