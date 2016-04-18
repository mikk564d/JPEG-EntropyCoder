
namespace JPEG_EntropyCoder.Interfaces {
   public interface IJPEGFileHandler {
        byte[] DQT { get; }
        byte[] DHT { get; }
        byte[] SOF { get; }
        byte[] SOS { get; }
        byte[] CompressedImage { get; set; }
        byte[] All { get; set; }
        void LoadFile(string path);
        void SaveFile(string path);
    }
}
