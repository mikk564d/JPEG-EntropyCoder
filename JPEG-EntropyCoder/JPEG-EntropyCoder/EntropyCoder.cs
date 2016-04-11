using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPEG_EntropyCoder.Components;
using JPEG_EntropyCoder.Interfaces;

namespace JPEG_EntropyCoder {
    public class EntropyCoder : IEntropyCoder {
        enum HuffmanTable {
            LumDC = 0, LumAC = 1, ChromDC = 2, ChromAC = 3
        }

        public List<EntropyComponent> EntropyComponents { get; set; }
        public void Encode() {
            throw new NotImplementedException();
        }

        public void Decode() {
            throw new NotImplementedException();
        }
    }
}
