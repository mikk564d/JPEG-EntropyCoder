using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegaProject {
    class DCComponent : ValueComponent {
        public DCComponent(string huffmanTreePath, string huffmanLeafHexValue, string amplitude) 
            : base(huffmanTreePath, huffmanLeafHexValue, amplitude) { }
    }
}
