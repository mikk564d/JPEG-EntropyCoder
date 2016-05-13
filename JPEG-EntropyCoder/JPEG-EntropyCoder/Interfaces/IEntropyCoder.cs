using System;
using System.Collections;
using System.Collections.Generic;
using JPEG_EntropyCoder.Components;

namespace JPEG_EntropyCoder.Interfaces {
    /// <summary>
    /// This interface EntropyCodes given data.
    /// </summary>
    public interface IEntropyCoder {
        /// <summary>
        /// List with EntropyComponent.
        /// </summary>
        List<EntropyComponent> EntropyComponents { get; set; }
        /// <summary>
        /// Encodes EntropyComponents to a BitArray.
        /// </summary>
        /// <returns>BitArray</returns>
        BitArray EncodeToBitArray();
        /// <summary>
        /// Encodes EntropyComponents to a byte[].
        /// </summary>
        /// <returns>byte[]</returns>
        byte[] EncodeToByteArray();     
    }
}
