using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPEG_EntropyCoder;
using JPEG_EntropyCoder.Components;
using JPEG_EntropyCoder.Interfaces;
using NUnit.Framework;
using Utilities;

namespace JPEG_EntropyCoderTests {
    [TestFixture]
    class EntropyCoderTestSmallImage {

        protected const string ROOT_NAME = "JPEG-EntropyCoderTests";
        /* Root path is found and set to the last folder of JPEG-EntropyCoderTests. */
        protected static string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory; // Has to be static in order to initialize rootPath.
        protected static string rootPath = baseDirectoryPath.Remove(baseDirectoryPath.LastIndexOf(ROOT_NAME) + ROOT_NAME.Length);
        private IEntropyCoder Coder { get; set; }    

        [SetUp]
        public void Init() {
            List<IHuffmanTree> huffmanTrees = new List<IHuffmanTree>();
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x09 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            BitArray compressedImageData = new BitArray(new byte[] { 0x2A, 0x02, 0xAA, 0x03 });
            compressedImageData = BitArrayUtilities.ReverseBitArray(BitArrayUtilities.ChangeEndianOnBitArray(compressedImageData));
            Coder = new EntropyCoder(huffmanTrees, compressedImageData, 1);
        }

        [Test]
        public void SplitDHT() {
            List<byte[]> expectedList = new List<byte[]>();
            expectedList.Add(new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x09 });
            expectedList.Add(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            expectedList.Add(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            expectedList.Add(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

            List<byte[]> actualList =
                SplitDHT(new byte[] {
                    0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08,
                    0x09, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x11, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00
                });

            Assert.AreEqual(expectedList[0], actualList[0]);
            Assert.AreEqual(expectedList[1], actualList[1]);
            Assert.AreEqual(expectedList[2], actualList[2]);
            Assert.AreEqual(expectedList[3], actualList[3]);
        }

        private List<byte[]> SplitDHT(byte[] DHTFromFile) {
            List<byte[]> DHTs = new List<byte[]>() { new byte[0], new byte[0], new byte[0], new byte[0] };
            int index = 0;
            for (int i = 0; i < 4; i++) {
                List<byte> dht = new List<byte>();
                int count = 0;
                HuffmanTreeType huffmanTree;
                if (DHTFromFile[index] < 16) {
                    huffmanTree = DHTFromFile[index] % 16 == 0 ? HuffmanTreeType.LumDC : HuffmanTreeType.ChromDC;
                } else {
                    huffmanTree = DHTFromFile[index] % 16 == 0 ? HuffmanTreeType.LumAC : HuffmanTreeType.ChromAC;
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

        [Test]
        public void EntropyCoderConstructor_SimpleValuesSmallPicture_BuildCorrect() {
            List<EntropyComponent> expectedComponents = new List<EntropyComponent>();
            expectedComponents.Add(new DCComponent(new BitArray(new bool[] { false }), 0x08, new BitArray(new bool[] { false, true, false, true, false, true, false, false })));
            expectedComponents.Add(new EOBComponent(new BitArray(new []{false}), 0x00));
            expectedComponents.Add(new DCComponent(new BitArray(new[] { false }), 0x00, new BitArray(new []{false})));
            expectedComponents.Add(new EOBComponent(new BitArray(new[] { false }), 0x00));
            expectedComponents.Add(new DCComponent(new BitArray(new[] { false }), 0x00, new BitArray(new[] { false })));
            expectedComponents.Add(new EOBComponent(new BitArray(new[] { false }), 0x00));

            expectedComponents.Add(new DCComponent(new BitArray(new bool[] { true, false }), 0x09, new BitArray(new bool[] { true, false, true, false, true, false, true, false, false })));
            expectedComponents.Add(new EOBComponent(new BitArray(new[] { false }), 0x00));
            expectedComponents.Add(new DCComponent(new BitArray(new[] { false }), 0x00, new BitArray(new[] { false })));
            expectedComponents.Add(new EOBComponent(new BitArray(new[] { false }), 0x00));
            expectedComponents.Add(new DCComponent(new BitArray(new[] { false }), 0x00, new BitArray(new[] { false })));
            expectedComponents.Add(new EOBComponent(new BitArray(new[] { false }), 0x00));

            Assert.IsTrue(Coder.EntropyComponents.SequenceEqual(expectedComponents));
        }

        [Test]
        public void EncodeToByteArray_SimpleValues_Calculated() {
            byte[] expectedBytes = new byte[] { 0x2A, 0x02, 0xAA, 0x03 };
            byte[] actualBytes = Coder.EncodeToByteArray();

            Assert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void EncodeToBitArray_SimpleValues_Calculated() {
            BitArray expectedBitArray = new BitArray(new[] { false, false, true, false, true, false, true, false,
                                                             false, false, false, false, false, false, true, false,
                                                             true, false, true, false, true, false, true, false,
                                                             false, false, false, false, false, false, true, true });

            BitArray actualBitArray = Coder.EncodeToBitArray();

            Assert.IsTrue(BitArrayUtilities.CompareBitArray(expectedBitArray, actualBitArray));
        }
    }
}
