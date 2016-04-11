
namespace JPEG_EntropyCoder {
   public interface IJPEGFileHandler {
        string DQT { get; set; }
        string DHT { get; set; }
        string SOF { get; set; }
        string SOS { get; set; }
        string CompressedImage { get; set; }
        string All { get; set; }
        void LoadFile(string path);
        void SaveFile(string path);
    }
}
