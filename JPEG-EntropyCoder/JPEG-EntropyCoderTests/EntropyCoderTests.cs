using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPEG_EntropyCoder;
using NUnit.Framework;

namespace JPEG_EntropyCoderTests {
    class EntropyCoderTests {

        [SetUp]
        public void Init() {
            EntropyCoder coder = new EntropyCoder(@"C:\Users\Mikke\Desktop\HammingTest\rainbow-1024p-q60-optimized.jpg");
            

        }

        [Test]
        public void BuildHuffmanTrees_SimpleValues_TreeCreated() {
            

        }

        [Test]
        public void GetByteFromHuffmantree_SimpleValues_ByteFound() {
            
        }
    }
}
