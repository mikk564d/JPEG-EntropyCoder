﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    public interface IHuffmanTree {
        string DHT { get; }
        string Find(string treePath);
        List<string> PrintTree();
    }
}
