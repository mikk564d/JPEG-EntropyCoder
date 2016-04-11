using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Components {
    class EOBComponent : EntropyComponent {
        public EOBComponent(string huffmanTreePath, string huffmanLeafHexValue)
            : base(huffmanTreePath, huffmanLeafHexValue, "EOB") { }
    }
}
