using System.IO;
using JPEG_EntropyCoder;
using NUnit.Framework;

namespace JPEG_EntropyCoderTests {
    [TestFixture]
    public class JPEGImageMultipleMarkersNoThumbnail : JPEGFileHandlerTests {
        [SetUp]
        public void Init() {
            string expectedBytesRootPath = $@"{rootPath}\Pictures\MULTIPLE_MARKERS_NO_THUMBNAIL";
            fileHandler = new JPEGFileHandler( $@"{rootPath}\Pictures\IMG_MULTIPLE_MARKERS_NO_THUMBNAIL.jpg" );
            expectedBytes[ DQT_STRING ] = File.ReadAllBytes( $@"{expectedBytesRootPath}\DQT" );
            expectedBytes[ DHT_STRING ] = File.ReadAllBytes( $@"{expectedBytesRootPath}\DHT" );
            expectedBytes[ SOF_STRING ] = File.ReadAllBytes( $@"{expectedBytesRootPath}\SOF" );
            expectedBytes[ SOS_STRING ] = File.ReadAllBytes( $@"{expectedBytesRootPath}\SOS" );
            expectedBytes[ COMPRESSED_IMAGE_STRING ] = File.ReadAllBytes( $@"{expectedBytesRootPath}\CompressedImage" );
            expectedBytes[ ALL_STRING ] = File.ReadAllBytes( $@"{expectedBytesRootPath}\All" );
            expectedBytes[ ALL_CHANGED_STRING ] = File.ReadAllBytes( $@"{expectedBytesRootPath}\AllChanged" );
        }
    }
}
