using System;
using System.Collections.Generic;
using System.Text;
using JPEG_EntropyCoder;
using JPEG_EntropyCoder.Components;

namespace Components {
    public class Decoder {

        enum HuffmanTable {
            LumDC = 0, LumAC = 1, ChromDC = 2, ChromAC = 3
        }

        private List<HuffmanTree> HuffmanTrees { get; }
        public List<EntropyComponent> EntropyComponents { get; }

        private string BinaryData { get; set; }
        private int CurrentIndex { get; set; }
        public int ComponentsThatCanBeChanged { get; set; }

        /// <summary>
        /// Decodes compressed image data.
        /// </summary>
        /// <para name="extractor"></para>
        public Decoder(JPEGFileHandler extractor) {
            CurrentIndex = 0;
            HuffmanTrees = new List<HuffmanTree>();
            EntropyComponents = new List<EntropyComponent>();

            buildHuffmanTrees(extractor);
            getBinaryData(extractor);
            decodeBinaryData();
            ComponentsThatCanBeChanged = CountValuesThatCanBeChanged();
            Console.WriteLine();
        }

        private void buildHuffmanTrees(JPEGFileHandler extractor) {
            List<string> DHT = extractor.GetDHT();

            foreach (string table in DHT) {
                HuffmanTrees.Add(new HuffmanTree(table));
            }
        }

        private void getBinaryData(JPEGFileHandler extractor) {
            string data = extractor.GetCompressedImageData();
            StringBuilder sBuilder = new StringBuilder();

            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(Convert.ToString(Convert.ToInt32(data[i].ToString(), 16), 2).PadLeft(4, '0'));
            }

            BinaryData = sBuilder.ToString();
        }

        private void ecodeBinaryData() {
            bool hitEOB;
            int count = 0;

            while (CurrentIndex < BinaryData.Length) {
                if (count % 1000 == 0) {
                    Console.WriteLine($"MCU {count}");
                }

                //Lum manuel supsampling for now change i
                for (int i = 0; i < 1; i++) {
                    decodeHuffmanHexValue(HuffmanTable.LumDC, true);
                    for (int j = 0; j < 63; j++) {
                       hitEOB = decodeHuffmanHexValue(HuffmanTable.LumAC, false);
                       if (hitEOB) {
                           break;
                        }
                    }
                }

                //Crom manuel supsampling for now change i
                for (int i = 0; i < 2; i++) {
                    decodeHuffmanHexValue(HuffmanTable.ChromDC, true);
                    for (int j = 0; j < 63; j++) {
                        hitEOB = decodeHuffmanHexValue(HuffmanTable.ChromAC, false);
                        if (hitEOB) {
                            break;
                        }
                    }
                }
                if (BinaryData.Length - CurrentIndex < 8) {
                    Console.WriteLine("Only trash left");
                    break;
                }
                count++;
            }
        }

        private bool decodeHuffmanHexValue(HuffmanTable table, bool isDC) {
            string huffmanLeafHexValue;
            string amplitude;
            string huffmanTreePath;

            getHuffmanLeafHexValue(out huffmanTreePath, out huffmanLeafHexValue, table);

            if (huffmanLeafHexValue != "00" || (table == HuffmanTable.ChromDC || table == HuffmanTable.LumDC)) {
                amplitude = getAmplitude(huffmanLeafHexValue);

                if (amplitude == "") {
                    amplitude = "0";
                }

                if (isDC) {
                    EntropyComponents.Add(new DCComponent(huffmanTreePath, huffmanLeafHexValue, amplitude));
                } else {
                    EntropyComponents.Add(new ACComponent(huffmanTreePath, huffmanLeafHexValue, amplitude));
                }
            } else {
                EntropyComponents.Add(new EOBComponent(huffmanTreePath, huffmanLeafHexValue));
                return true;
            }
            
            return false;
        }

        private string getHuffmanLeafHexValue(out string currentHuffmanTreePath, out string huffmanLeafHexValue, HuffmanTable table) {
            currentHuffmanTreePath = "";
            huffmanLeafHexValue = "";

            //LocalCount skal måske laves om
            int localCount = 0;

            while (huffmanLeafHexValue == "" && localCount < 16) {
                currentHuffmanTreePath += BinaryData[CurrentIndex];
                huffmanLeafHexValue = HuffmanTrees[(int)table].SearchFor(currentHuffmanTreePath, 0);
                CurrentIndex++;
                localCount++;
            }

            return huffmanLeafHexValue;
        }

        private string getAmplitude(string huffmanLeafHexValue) {
            string value = "";
            int lenght = Convert.ToInt32(huffmanLeafHexValue[1].ToString(), 16);

            for (int i = 0; i < lenght; i++, CurrentIndex++) {
                value += BinaryData[CurrentIndex];
            }
            return value;
        }

        /// <summary>
        /// Gets encoded compressed image data, which can be injected into a new JPEG.
        /// </summary>
        /// <returns>encoded compressed image data.</returns>
        public string getReEncodedRawHexData() {
            BinaryData = "";
            string HexData = "";

            StringBuilder sBuilder = new StringBuilder();

            foreach (EntropyComponent entropyComponent in EntropyComponents) {
                if ((entropyComponent is DCComponent && entropyComponent.HuffmanLeafHexValue == "00") || entropyComponent is EOBComponent) {
                    sBuilder.Append(entropyComponent.HuffmanTreePath);
                } else {
                    sBuilder.Append(entropyComponent.HuffmanTreePath + entropyComponent.Amplitude);
                }
            }

            while (sBuilder.Length % 8 != 0) {
                sBuilder.Append("1");
            }

            BinaryData = sBuilder.ToString();

            sBuilder.Clear();

            for (int i = 0; i < BinaryData.Length; i += 4) {
                sBuilder.Append(Convert.ToString(Convert.ToInt32(BinaryData.Substring(i, 4), 2), 16));
            }

            HexData = sBuilder.ToString();

            return HexData;
        }

        private int CountValuesThatCanBeChanged() {
            int count = 0;

            foreach (EntropyComponent entropyComponent in EntropyComponents) {
                if (entropyComponent is ACComponent) {
                    count++;
                }
            }

            return count;
        }
    }
}
