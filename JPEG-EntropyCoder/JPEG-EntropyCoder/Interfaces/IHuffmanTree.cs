using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    interface IHuffmanTree {
        string Find(string treePath);
        string PrintTree();
        string DHT { get; }
    }
}
