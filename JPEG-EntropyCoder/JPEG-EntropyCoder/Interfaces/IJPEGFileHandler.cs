
namespace JPEG_EntropyCoder.Interfaces {
   public interface IJPEGFileHandler {
        byte[] DQT { get; }
        byte[] DHT { get; }
        byte[] SOF { get; }
        byte[] SOS { get; }
        byte[] CompressedImage { get; set; }
        byte[] All { get; }
        void SaveFile( string path );
    }
}
