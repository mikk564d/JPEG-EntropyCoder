
namespace JPEG_EntropyCoder.Components {
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
