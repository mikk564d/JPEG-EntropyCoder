using JPEG_EntropyCoder.Components;

namespace JPEG_EntropyCoder {
    public interface IRunLengthCoder {
        void Encode(EntropyComponent entropyComponent);
        EntropyComponent Decode();
    }
}
