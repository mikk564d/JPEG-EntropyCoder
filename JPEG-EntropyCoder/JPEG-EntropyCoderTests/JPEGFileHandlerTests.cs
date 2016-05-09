using System;
using System.Collections.Generic;
using System.IO;
using JPEG_EntropyCoder;
using JPEG_EntropyCoder.Interfaces;
using NUnit.Framework;

namespace JPEG_EntropyCoderTests {
    [TestFixture]
    public abstract class JPEGFileHandlerTests {
        protected const string ROOT_NAME = "JPEG-EntropyCoderTests";

        protected const string DQT_STRING = "DQT";
        protected const string DHT_STRING = "DHT";
        protected const string SOF_STRING = "SOF";
        protected const string SOS_STRING = "SOS";
        protected const string COMPRESSED_IMAGE_STRING = "CompressedImage";
        protected const string ALL_STRING = "All";
        protected const string ALL_CHANGED_STRING = "AllChanged";

        /* Root path is found and set to the last folder of JPEG-EntropyCoderTests. */
        protected static string baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory; // Has to be static in order to initialize rootPath.
        protected static string rootPath = baseDirectoryPath.Remove( baseDirectoryPath.LastIndexOf( ROOT_NAME ) + ROOT_NAME.Length );

        protected IJPEGFileHandler fileHandler;
        protected Dictionary<string, byte[]> expectedBytes = new Dictionary<string, byte[]>();
        protected byte[] setBytes = { 0x1, 0x2, 0x3, 0x4, 0x5 };

        [Test]
        public void DQT_Get() {
            Assert.AreEqual( expectedBytes[ DQT_STRING ], fileHandler.DQT );
        }

        [Test]
        public void DHT_Get() {
            Assert.AreEqual( expectedBytes[ DHT_STRING ], fileHandler.DHT );
        }

        [Test]
        public void SOF_Get() {
            Assert.AreEqual( expectedBytes[ SOF_STRING ], fileHandler.SOF );
        }

        [Test]
        public void SOS_Get() {
            Assert.AreEqual( expectedBytes[ SOS_STRING ], fileHandler.SOS );
        }

        [Test]
        public void CompressedImage_Get() {
            Assert.AreEqual( expectedBytes[ COMPRESSED_IMAGE_STRING ], fileHandler.CompressedImage );
        }

        [Test]
        /* This test also tests the private FindMarkerIndexes method. */
        public void CompressedImage_Set_All_Changed_Accordingly() {
            fileHandler.CompressedImage = setBytes;
            byte[] allActualBytes = fileHandler.All;
            Assert.AreEqual( expectedBytes[ ALL_CHANGED_STRING ], allActualBytes );
        }

        [Test]
        public void All_Get() {
            Assert.AreEqual( expectedBytes[ ALL_STRING ], fileHandler.All );
        }

        [Test]
        public void Save() {
            string testFilePath = $@"{rootPath}\Pictures\TestFile.jpg";
            fileHandler.Save( testFilePath );
            byte[] actualBytes = File.ReadAllBytes( testFilePath );
            Assert.AreEqual( expectedBytes[ ALL_STRING ], actualBytes );
        }
    }
}
