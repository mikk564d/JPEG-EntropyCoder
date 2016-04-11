using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StegaProject {
     public abstract class EntropyComponent {
        protected EntropyComponent(string huffmanTreePath, string huffmanLeafHexValue, string amplitude) {
            HuffmanTreePath = huffmanTreePath;
            HuffmanLeafHexValue = huffmanLeafHexValue;
            Amplitude = amplitude;
        }

        public string HuffmanTreePath { get; }

        public string Amplitude { get; set; }

        public string HuffmanLeafHexValue { get; }      
    }
}
