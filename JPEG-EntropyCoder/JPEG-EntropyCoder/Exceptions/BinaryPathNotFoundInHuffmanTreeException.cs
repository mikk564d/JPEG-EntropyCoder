using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Exceptions {
    class BinaryPathNotFoundInHuffmanTreeException : Exception {
        public BinaryPathNotFoundInHuffmanTreeException(string message) 
            : base(message) {
        }
    }
}
