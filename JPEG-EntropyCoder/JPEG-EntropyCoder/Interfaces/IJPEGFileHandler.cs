
namespace JPEG_EntropyCoder.Interfaces {
   public interface IJPEGFileHandler {
        byte[] DQT { get; set; }
        byte[] DHT { get; set; }
        byte[] SOF { get; set; }
        byte[] SOS { get; set; }
        byte[] CompressedImage { get; set; }
        byte[] All { get; set; }
        void LoadFile(string path);
        void SaveFile(string path);
    }
}
