using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Interfaces {
    public interface IHuffmanTree {
        byte[] DHT { get; }
        byte Find(SimpleBitVector16 treePath);
        List<string> PrintTree();
    }
}
