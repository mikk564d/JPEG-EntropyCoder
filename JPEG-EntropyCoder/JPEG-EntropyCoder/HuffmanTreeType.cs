namespace JPEG_EntropyCoder {
    /// <summary>
    /// Enum to describe the different HuffmanTrees
    /// </summary>
    public enum HuffmanTreeType {
        /// <summary>
        /// A luminance DC type.
        /// </summary>
        LumDC = 0,
        /// <summary>
        /// A chrominance DC type.
        /// </summary>
        ChromDC = 1,
        /// <summary>
        /// A luminance AC type.
        /// </summary>
        LumAC = 2,
        /// <summary>
        /// A chrominance AC type.
        /// </summary>
        ChromAC = 3,
        /// <summary>
        /// A luminance type.
        /// </summary>
        Luminance = 4,
        /// <summary>
        /// A luminance type.
        /// </summary>
        Chrominance = 5
    }
}