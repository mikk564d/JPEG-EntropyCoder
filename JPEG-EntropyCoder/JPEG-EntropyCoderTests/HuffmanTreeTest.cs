using System;
using JPEG_EntropyCoder;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace JPEG_EntropyCoderTests {
    [TestFixture]
    public class HuffmanTreeTest {

        private HuffmanTree tree;

        [SetUp]
        public void init() {
            byte[] testDHT = new byte[] { 0x00, 0x00, 0x06, 0x02, 0x03, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x07, 0x08, 0x06, 0x05, 0x04, 0x09, 0x03, 0x0A, 0x02, 0x01, 0x00, 0x0B};
            this.tree = new HuffmanTree(testDHT);
        }

        public static BitVector16 charsToBitArray(char[] input) {
            bool[] result = new bool[input.Length];

            for (int i = 0; i < input.Length; i++) {
                if (input[i] == '1') {
                    result[i] = true;
                } else if (input[i] == '0') {
                    result[i] = false;
                }
            }

            return new BitVector16(result);
        }

        public static IEnumerable<Tuple<byte, BitVector16>> FindGetTestCases() {
            string[] addrs = new string[] {
                    "07_000",
                    "08_001",
                    "06_010",
                    "05_011",
                    "04_100",
                    "09_101",
                    "03_1100",
                    "0A_1101",
                    "02_11100",
                    "01_11101",
                    "00_11110",
                    "0B_111110"
            };

            foreach (string addr in addrs) {
                string[] addrc = addr.Split('_');
                char[] chars = addrc[1].ToCharArray();
                for (int i = 0; i < chars.Length - 1; i++) {
                  yield return new Tuple<byte, BitVector16>(0xFF, charsToBitArray(chars.Take(i).ToArray()));
                }

                yield return new Tuple<byte, BitVector16>(byte.Parse(addrc[0],System.Globalization.NumberStyles.HexNumber), charsToBitArray(chars));
            }
        }

        [Test, TestCaseSource("FindGetTestCases")]
        public void FindTest(Tuple<byte, BitVector16> casetup) {
            Assert.AreEqual(casetup.Item1, this.tree.Find(casetup.Item2), casetup.Item2.ToString());
        }

        [Test]
        public void testest() {
            Assert.AreEqual(1, 1);

        }
    }
}
