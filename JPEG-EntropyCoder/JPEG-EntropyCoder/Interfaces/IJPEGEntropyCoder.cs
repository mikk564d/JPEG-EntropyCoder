using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPEG_EntropyCoder.Components;

namespace JPEG_EntropyCoder.Interfaces {
    /// <summary>
    /// This is the libraries main interface.
    /// </summary>
    public interface IJPEGEntropyCoder {
        /// <summary>
        /// List with EntropyComponent
        /// </summary>
        List<EntropyComponent> EntropyComponents { get; set; }
        /// <summary>
        /// Saves the file to <paramref name="path"/>
        /// </summary>
        /// <param name="path"></param>
        void Save(string path);
    }
}
