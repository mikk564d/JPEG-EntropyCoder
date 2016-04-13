using System.Collections.Generic;
using JPEG_EntropyCoder.Components;

namespace JPEG_EntropyCoder.Interfaces {
    public interface IEntropyCoder {
        List<EntropyComponent> EntropyComponents { get; }
        void Encode();
        void Decode();
        void Save(string path);
    }
}
