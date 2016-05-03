using System.Collections.Generic;
using JPEG_EntropyCoder.Components;

namespace JPEG_EntropyCoder.Interfaces {
    /// <summary>
    /// Wrapper interface to directly execute entropy coding on JPEG file.
    /// Alternatively, combine IJPEGFileHandler, IHuffmanTree and IEntropyCoder to do it yourself.
    /// </summary>
    public interface IJPEGEntropyCoder {
        /// <summary>
        /// List with EntropyComponents
        /// </summary>
        List<EntropyComponent> EntropyComponents { get; set; }
        
        /// <summary>
        /// Saves the JPEG file to <paramref name="path"/>
        /// </summary>
        /// <param name="path">Path to JPEG file.</param>
        void Save(string path);
    }
}
