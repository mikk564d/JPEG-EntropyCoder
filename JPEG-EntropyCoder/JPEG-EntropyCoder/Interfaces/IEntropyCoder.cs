using System.Collections;
using System.Collections.Generic;
using JPEG_EntropyCoder.Components;

namespace JPEG_EntropyCoder.Interfaces {
    public interface IEntropyCoder {
        List<EntropyComponent> EntropyComponents { get; }
        BitArray GetDataAsBitArray();
        byte[] GetDataAsByteArray();
        void Encode();
        void Decode();
        void Save(string path);
    }
}
