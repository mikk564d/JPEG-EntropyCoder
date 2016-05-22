using System;
using NUnit.Framework;
using JPEG_EntropyCoder;

namespace JPEG_EntropyCoderTests
{
    [TestFixture]
    public class SimpleBitVector16Tests
    {
        [Test]
        public void SimpleBitVector16Constructor_SimpleValue_BuildCorrect()
        {
            bool[] arr = {false, true, false, false, false, true, true, true};

            BitVector16 v = new BitVector16(arr);
            bool[] resultArray = new bool[8];

            for (byte i = 0; i < v.Length; i++)
            {
                resultArray[i] = v.Get(i);
            }

            Assert.AreEqual(arr, resultArray);
        }

        [Test]
        public void SimpleBitVector16Get_TrueValue_Fetched()
        {
            bool[] arr = { false, true, false, false, false, true, true, true };

            BitVector16 v = new BitVector16(arr);
            
            Assert.AreEqual(true, v[1]);
        }

        [Test]
        public void SimpleBitVector16Get_FalseValue_Fetched()
        {
            bool[] arr = { false, true, false, false, false, true, true, true };

            BitVector16 v = new BitVector16(arr);

            Assert.AreEqual(false, v[2]);
        }

        [Test]
        public void SimpleBitVector16Get_FalseOnlyValue_Fetched()
        {
            bool[] arr = { true, true, false, true, true, true, true, true };

            BitVector16 v = new BitVector16(arr);

            Assert.AreEqual(false, v[2]);
        }

        [Test]
        public void SimpleBitVector16Set_FalseValue_Set()
        {
            bool[] arr = { false, true, false, false, false, true, true, true };

            BitVector16 v = new BitVector16(arr);
            v[1] = false;

            Assert.AreEqual(false, v[1]);
        }

        [Test]
        public void SimpleBitVector16Set_TrueValue_Set()
        {
            bool[] arr = { false, true, false, false, false, true, true, true };

            BitVector16 v = new BitVector16(arr);
            v[2] = true;

            Assert.AreEqual(true, v[2]);
        }
    }
}
