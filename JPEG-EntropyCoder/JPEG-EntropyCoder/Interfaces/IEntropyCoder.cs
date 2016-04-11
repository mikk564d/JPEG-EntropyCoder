using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;

namespace JPEG_EntropyCoder.Interfaces {
    interface IEntropyCoder {
        List<EntropyComponent> 
        void Encoder(string compressedImage);
        Decoder();
    }
}
