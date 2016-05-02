using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JPEG_EntropyCoder;
using JPEG_EntropyCoder.Interfaces;
using NUnit.Framework;
using Utilities;

namespace JPEG_EntropyCoderTests {
    [TestFixture]
    public class EntropyCoderTestMediumImage {

        public EntropyCoder Coder { get; set; } 

        [SetUp]
        public void Init() {
            List<byte[]> DHTs = SplitDHT(File.ReadAllBytes(@"C:\Users\Nyggi\Documents\GitHub\JPEG-EntropyCoder\JPEG-EntropyCoder\JPEG-EntropyCoderTests\Pictures\SINGLE_MARKER_NO_THUMBNAIL\DHT"));
            List<IHuffmanTree> huffmanTrees = new List<IHuffmanTree>();
            huffmanTrees.Add(new HuffmanTree(DHTs[0]));
            huffmanTrees.Add(new HuffmanTree(DHTs[1]));
            huffmanTrees.Add(new HuffmanTree(DHTs[2]));
            huffmanTrees.Add(new HuffmanTree(DHTs[3]));
            BitArray compressedImageData = new BitArray(File.ReadAllBytes(@"C:\Users\Nyggi\Documents\GitHub\JPEG-EntropyCoder\JPEG-EntropyCoder\JPEG-EntropyCoderTests\Pictures\SINGLE_MARKER_NO_THUMBNAIL\CompressedImage"));
            compressedImageData = BitArrayUtilities.ReverseBitArray(BitArrayUtilities.ChangeEndianOnBitArray(compressedImageData));
            Coder = new EntropyCoder(huffmanTrees, compressedImageData, 4);
        }

        [Test]
        public void EncodeToByteArray_SimpleValues_Calculated() {
            byte[] actualBytes = Coder.EncodeToByteArray();

            byte[] expectedBytes = File.ReadAllBytes( @"C:\Users\Nyggi\Documents\GitHub\JPEG-EntropyCoder\JPEG-EntropyCoder\JPEG-EntropyCoderTests\Pictures\SINGLE_MARKER_NO_THUMBNAIL\CompressedImage");

            Assert.AreEqual(expectedBytes, actualBytes);
        }

        private List<byte[]> SplitDHT(byte[] DHTFromFile) {
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

            return DHTs;
        }
    }
}