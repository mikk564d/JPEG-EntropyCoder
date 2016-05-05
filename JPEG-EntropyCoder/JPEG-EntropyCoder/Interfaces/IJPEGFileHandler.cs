namespace JPEG_EntropyCoder.Interfaces {
    /// <summary>
    /// This interface holds properties with data from and methods to process a JPEG file.
    /// The class is able to save a JPEG file aswell as gettting markers' data and all data.
    /// Also the class is able to get and set compressed image data in the JPEG file.
    /// </summary>
    public interface IJPEGFileHandler {
        /// <summary>
        /// Contains DQT bytes from the JPEG file.
        /// </summary>
        byte[] DQT { get; }

        /// <summary>
        /// Contains DHT bytes from the JPEG file.
        /// </summary>
        byte[] DHT { get; }

        /// <summary>
        /// Contains SOF bytes from the JPEG file.
        /// </summary>
        byte[] SOF { get; }

        /// <summary>
        /// Contains SOS bytes from the JPEG file.
        /// </summary>
        byte[] SOS { get; }

        /// <summary>
        /// Contains compressed image bytes from the JPEG file.
        /// </summary>
        byte[] CompressedImage { get; set; }

        /// <summary>
        /// Contains all bytes from the JPEG file.
        /// </summary>
        byte[] All { get; }

        /// <summary>
        /// Saves a JPEG file at the given path based on all contained bytes.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to save.</param>
        void Save(string path);
    }
}
