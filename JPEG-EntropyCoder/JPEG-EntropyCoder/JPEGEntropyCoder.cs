using System;
using System.Collections;
using System.Collections.Generic;
using JPEG_EntropyCoder.Components;
using JPEG_EntropyCoder.Interfaces;
using Utilities;

namespace JPEG_EntropyCoder {
    /// <summary>
    /// Works as the primary class for this library.
    /// </summary>
    public class JPEGEntropyCoder : IJPEGEntropyCoder {
        /// <summary>
        /// List with EntropyComponent
        /// </summary>
        public List<EntropyComponent> EntropyComponents {
            get { return Coder.EntropyComponents; }
            set { Coder.EntropyComponents = value; }
        }

        private IEntropyCoder Coder { get; }
        private IJPEGFileHandler FileHandler { get; }

        /// <summary>
        /// Loads JPEG image and decodes it.
        /// </summary>
        /// <param name="path">Path to JPEG image</param>
        public JPEGEntropyCoder(string path) {
            FileHandler = new JPEGFileHandler(path);
            List<IHuffmanTree> huffmanTrees = BuildHuffmanTrees(FileHandler.DHT);
            BitArray compressedImage = ConvertBytesToBitArray(FileHandler.CompressedImage);
            int luminensBlocks = GetNumberOfLuminanceBlocksPerMCU(FileHandler.SOF);
            Coder = new EntropyCoder(huffmanTrees, compressedImage, luminensBlocks);
        }

        /// <summary>
        /// Builds HuffmanTrees with <paramref name="DHTFromFile"/>
        /// </summary>
        /// <param name="DHTFromFile">DHT from IJPEGFileHandler</param>
        /// <returns>Returns a list with HuffmanTree</returns>
        private List<IHuffmanTree> BuildHuffmanTrees(byte[] DHTFromFile) {
            List<byte[]> DHTs = new List<byte[]>() { new byte[0], new byte[0], new byte[0], new byte[0] };
            int index = 0;
            for (int i = 0; i < 4; i++) {
                List<byte> dht = new List<byte>();
                int count = 0;
                HuffmanTable huffmanTree;
                if (DHTFromFile[index] < 16) {
                    huffmanTree = DHTFromFile[index] % 16 == 0 ? HuffmanTable.LumDC : HuffmanTable.ChromDC;
                } else {
                    huffmanTree = DHTFromFile[index] % 16 == 0 ? HuffmanTable.LumAC : HuffmanTable.ChromAC;
                }
                index++;
                for (int j = 0; j < 16; index++, j++) {
                    dht.Add(DHTFromFile[index]);
                    count += Convert.ToInt32(dht[j]);
                }

                for (int k = 0; k < count; index++, k++) {
                    dht.Add(DHTFromFile[index]);
                }
                DHTs[(int)huffmanTree] = dht.ToArray();
            }

            List<IHuffmanTree> huffmanTrees = new List<IHuffmanTree>();
            foreach (byte[] table in DHTs) {
                huffmanTrees.Add(new HuffmanTree(table));
            }

            return huffmanTrees;
        }

        /// <summary>
        /// Convert byte[] to BitArray and reverse the BitArray.
        /// </summary>
        /// <param name="bytesFromFile"></param>
        /// <returns>Returns BitArray</returns>
        private BitArray ConvertBytesToBitArray(byte[] bytesFromFile) {
            List<byte> bytes = new List<byte>();

            bytes.Add(bytesFromFile[0]);
            for (int i = 1; i < bytesFromFile.Length; i++) {
                if (!(bytesFromFile[i] == 0x00 && bytesFromFile[i - 1] == 0xFF)) {
                    bytes.Add(bytesFromFile[i]);
                }
            }

            BitArray binData = new BitArray(bytes.ToArray());
            binData = BitArrayUtilities.ChangeEndianOnBitArray(binData);
            binData = BitArrayUtilities.ReverseBitArray(binData);

            return binData;
        }

        ///<summary>
        /// Gets the number of luminance blocks per MCU based on SOF bytes.
        /// </summary>
        /// <param name="SOF">Bytes with SOF data.</param>
        /// <returns>Returns either 1 or 4 which corresponds to either 4:4:4 or 4:2:0 subsampling.</returns>
        private int GetNumberOfLuminanceBlocksPerMCU(byte[] SOF) {
            byte luminanceSubsampling = SOF[7]; // Luminance subsampling info in SOF field data.

            /* Either Luminance subsampling is 0x11 or 0x22 */
            if (luminanceSubsampling == 0x11) {
                return 1;
            } else {
                return 4;
            }
        }

        /// <summary>
        /// Save the JPEG image to <paramref name="path"/>
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path) {
            FileHandler.CompressedImage = Coder.EncodeToByteArray();
            FileHandler.SaveFile(path);
        }
    }
}
