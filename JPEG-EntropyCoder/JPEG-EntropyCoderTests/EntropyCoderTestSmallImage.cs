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

        private IEntropyCoder Coder { get; set; }    

        [SetUp]
        public void Init() {
            List<IHuffmanTree> huffmanTrees = new List<IHuffmanTree>();
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x09 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            BitArray compressedImageData = new BitArray(new byte[] { 0x2A, 0x02, 0xAA, 0x03 });
            BitArrayUtilities.ChangeEndianOnBitArray(compressedImageData);
            BitArrayUtilities.ReverseBitArray(compressedImageData);
            Coder = new EntropyCoder(huffmanTrees, compressedImageData, 1);
        }

        [Test]
        public void EntropyCoderConstructor_SimpleValuesSmallPicture_BuildCorrect() {
            List<EntropyComponent> expectedComponents = new List<EntropyComponent>();
            expectedComponents.Add(new DCComponent(new SimpleBitVector16(new bool[] { false }), 0x08, new SimpleBitVector16(new bool[] { false, true, false, true, false, true, false, false })));
            expectedComponents.Add(new EOBComponent(new SimpleBitVector16(new []{false}), 0x00));
            expectedComponents.Add(new DCComponent(new SimpleBitVector16(new[] { false }), 0x00, new SimpleBitVector16(new []{false})));
            expectedComponents.Add(new EOBComponent(new SimpleBitVector16(new[] { false }), 0x00));
            expectedComponents.Add(new DCComponent(new SimpleBitVector16(new[] { false }), 0x00, new SimpleBitVector16(new[] { false })));
            expectedComponents.Add(new EOBComponent(new SimpleBitVector16(new[] { false }), 0x00));

            expectedComponents.Add(new DCComponent(new SimpleBitVector16(new bool[] { true, false }), 0x09, new SimpleBitVector16(new bool[] { true, false, true, false, true, false, true, false, false })));
            expectedComponents.Add(new EOBComponent(new SimpleBitVector16(new[] { false }), 0x00));
            expectedComponents.Add(new DCComponent(new SimpleBitVector16(new[] { false }), 0x00, new SimpleBitVector16(new[] { false })));
            expectedComponents.Add(new EOBComponent(new SimpleBitVector16(new[] { false }), 0x00));
            expectedComponents.Add(new DCComponent(new SimpleBitVector16(new[] { false }), 0x00, new SimpleBitVector16(new[] { false })));
            expectedComponents.Add(new EOBComponent(new SimpleBitVector16(new[] { false }), 0x00));

            Assert.IsTrue(Coder.EntropyComponents.SequenceEqual(expectedComponents));
        }

        [Test]
        public void EncodeToByteArray_SimpleValues_Calculated() {
            byte[] expectedBytes = new byte[] { 0x2A, 0x02, 0xAA, 0x00 }; //0x2A, 0x02, 0xAA, 0x03
            byte[] actualBytes = Coder.EncodeToByteArray();

            Assert.AreEqual(expectedBytes, actualBytes);
        }

        [Test]
        public void EncodeToBitArray_SimpleValues_Calculated() {
            BitArray expectedBitArray = new BitArray(new[] { false, false, true, false, true, false, true, false,
                                                             false, false, false, false, false, false, true, false,
                                                             true, false, true, false, true, false, true, false,
                                                             false, false, false, false, false, false, false, false }); // true, true last two.

            BitArray actualBitArray = Coder.EncodeToBitArray();

            Assert.IsTrue(BitArrayUtilities.CompareBitArray(expectedBitArray, actualBitArray));
        }
    }
}
