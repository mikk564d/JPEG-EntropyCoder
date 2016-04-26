using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder.Interfaces {
    public interface IJPEGEntropyCoder {
        void Encode();
        void Save(string path);
    }
}
