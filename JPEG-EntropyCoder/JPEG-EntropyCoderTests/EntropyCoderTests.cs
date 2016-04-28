using System;
using System.Collections;
using System.Collections.Generic;
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
    class EntropyCoderTests {

        [SetUp]
        public void Init() {
            
        }

        [Test]
        public void EntropyCoderConstructor_SimpleValues_BuildCorrect() {
            List<IHuffmanTree> huffmanTrees = new List<IHuffmanTree>();
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x09 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            huffmanTrees.Add(new HuffmanTree(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }));
            BitArray compressedImageData = new BitArray(new byte[] {0x2A, 0x02, 0xAA, 0x03});
            compressedImageData = BitArrayUtilities.ReverseBitArray(BitArrayUtilities.ChangeEndianOnBitArray(compressedImageData));
            EntropyCoder coder = new EntropyCoder(huffmanTrees, compressedImageData);

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

            Assert.IsTrue(coder.EntropyComponents.SequenceEqual(expectedComponents));
        }
    }
}
