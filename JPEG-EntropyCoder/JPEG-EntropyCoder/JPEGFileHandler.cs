using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using JPEG_EntropyCoder.Interfaces;

namespace JPEG_EntropyCoder {
    /// <summary>
    /// This class holds properties with data from and methods to process a JPEG file.
    /// The class is able to save a JPEG file aswell as gettting markers' data and all data.
    /// Also the class is able to get and set compressed image data in the JPEG file.
    /// </summary>
    public class JPEGFileHandler : IJPEGFileHandler {
        /* Symbolic constants */
        private const byte MARKER_LENGTH = 2;
        private const byte LENGTH_OF_FIELD_LENGTH = 2;
        private const byte MARKER_PREFIX = 0xFF;
        private const byte SOI_MARKER = 0xD8;
        private const byte DQT_MARKER = 0xDB;
        private const byte DHT_MARKER = 0xC4;
        private const byte SOF0_MARKER = 0xC0;
        private const byte SOF2_MARKER = 0xC2;
        private const byte SOS_MARKER = 0xDA;
        private const byte EOI_MARKER = 0xD9;

        /* Array with all markers which makes it easy to iterate through all markers. */
        private readonly byte[] markers = { SOI_MARKER, DQT_MARKER, DHT_MARKER, SOF0_MARKER, SOF2_MARKER, SOS_MARKER, EOI_MARKER };
        /* Dictionary with all marker indexes. Markers can occur multiple times hence the value is a list. */
        private Dictionary<byte, List<uint>> markerIndexes = new Dictionary<byte, List<uint>> {
            { SOI_MARKER, new List<uint>() },
            { DQT_MARKER, new List<uint>() },
            { DHT_MARKER, new List<uint>() },
            { SOF0_MARKER, new List<uint>() },
            { SOF2_MARKER, new List<uint>() },
            { SOS_MARKER, new List<uint>() },
            { EOI_MARKER, new List<uint>() }
        };

        /* Private "property-variables" */
        private byte[] _DQT;
        private byte[] _DHT;
        private byte[] _SOF;
        private byte[] _SOS;
        private byte[] _compressedImage;
        private byte[] _all;

        /// <summary>
        /// Contains DQT bytes from the JPEG file.
        /// </summary>
        public byte[] DQT {
            get { return Array.AsReadOnly(_DQT).ToArray(); }
            private set { _DQT = value; }
        }

        /// <summary>
        /// Contains DHT bytes from the JPEG file.
        /// </summary>
        public byte[] DHT {
            get { return Array.AsReadOnly(_DHT).ToArray(); }
            private set { _DHT = value; }
        }

        /// <summary>
        /// Contains SOF bytes from the JPEG file.
        /// </summary>
        public byte[] SOF {
            get { return Array.AsReadOnly(_SOF).ToArray(); }
            private set { _SOF = value; }
        }

        /// <summary>
        /// Contains SOS bytes from the JPEG file.
        /// </summary>
        public byte[] SOS {
            get { return Array.AsReadOnly(_SOS).ToArray(); }
            private set { _SOS = value; }
        }

        /// <summary>
        /// Contains compressed image bytes from the JPEG file.
        /// </summary>
        public byte[] CompressedImage {
            get { return Array.AsReadOnly(_compressedImage).ToArray(); }
            set {
                Contract.Requires<ArgumentNullException>(value != null);
                Contract.Requires<ArgumentException>(value.Length > 0);

                UpdateAllWithCompressedImage(value);

                /* Elvis operator is used because _compressedImage will be null first time property is set. */
                if (_compressedImage?.Length != value.Length) {
                    FillMarkerIndexes(markerIndexes, All);
                }

                _compressedImage = value;
            }
        }

        /// <summary>
        /// Updates All property with the compressedImage bytes.
        /// </summary>
        /// <param name="compressedImage">Compressed image bytes from JPEG file.</param>
        private void UpdateAllWithCompressedImage(byte[] compressedImage) {
            byte[] all = All;

            List<uint> SOSIndexesList = markerIndexes[SOS_MARKER];
            uint SOSIndex = SOSIndexesList[SOSIndexesList.Count - 1];

            /* Garbage bytes can contain markers. Find the first EOI marker after SOS marker. */
            List<uint> EOIIndexesList = markerIndexes[EOI_MARKER];
            uint EOIIndex = EOIIndexesList.First(markerIndex => markerIndex > SOSIndex);

            uint lengthOfField = GetFieldLength(markerIndexes, SOS_MARKER, SOSIndexesList.Count - 1, all);

            byte[] bytes = all.Take(Convert.ToInt32(SOSIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH + lengthOfField)).ToArray();

            bytes = bytes.Concat(compressedImage).ToArray();

            bytes = bytes.Concat(all.Skip(Convert.ToInt32(EOIIndex))).ToArray();

            All = bytes;
        }

        /// <summary>
        /// Contains all bytes from the JPEG file.
        /// </summary>
        public byte[] All {
            get { return Array.AsReadOnly(_all).ToArray(); }
            private set { _all = value; }
        }

        /// <summary>
        /// Loads bytes from a JPEG file into the instance.
        /// </summary>
        /// <param name="bytes">Bytes from the JPEG file you wish to process.</param>
        public JPEGFileHandler(byte[] bytes) {
            Contract.Requires<ArgumentNullException>(bytes != null);
            Contract.Requires<ArgumentException>(bytes.Length > 0);

            All = bytes;

            FillMarkerIndexes(markerIndexes, All);

            uint[] thumbnailIndexes = GetThumbnailIndexes(markerIndexes, All);

            DQT = GetFieldBytes(markerIndexes, DQT_MARKER, thumbnailIndexes[0], thumbnailIndexes[1], All);
            DHT = GetFieldBytes(markerIndexes, DHT_MARKER, thumbnailIndexes[0], thumbnailIndexes[1], All);

            /* Image contains either SOF0 or SOF2 - never both. */
            if (markerIndexes[SOF0_MARKER].Count > 0)
                SOF = GetFieldBytes(markerIndexes, SOF0_MARKER, thumbnailIndexes[0], thumbnailIndexes[1], All);
            else
                SOF = GetFieldBytes(markerIndexes, SOF2_MARKER, thumbnailIndexes[0], thumbnailIndexes[1], All);

            SOS = GetFieldBytes(markerIndexes, SOS_MARKER, thumbnailIndexes[0], thumbnailIndexes[1], All);

            CompressedImage = GetCompressedImageBytes(markerIndexes, All);
        }

        /// <summary>
        /// Loads a JPEG file into the instance.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to process.</param>
        public JPEGFileHandler(string path) : this(File.ReadAllBytes(path)) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);
        }

        /// <summary>
        /// Fills the markerIndexes dictionary with all indexes for all found markers.
        /// </summary>
        /// <param name="dictionary">Dictionary to be filled with marker indexes.</param>
        /// <param name="bytes">Array of bytes that will be searched for markers.</param>
        private void FillMarkerIndexes(Dictionary<byte, List<uint>> dictionary, byte[] bytes) {
            /* Empty the dictionary before finding all indexes.
             * This is done because indexes might have changed after setting compressedImage. */
            foreach (var item in dictionary) {
                dictionary[item.Key].Clear();
            }

            /* Iterate through bytes. If a byte is the marker prefix, check if next byte is either of the markers. */
            for (uint i = 0; i < bytes.Length - 1; i++) {
                if (bytes[i] == MARKER_PREFIX) {
                    foreach (byte marker in markers) {
                        if (bytes[i + 1] == marker)
                            dictionary[marker].Add(i);
                    }
                }
            }
        }

        /// <summary>
        /// Returns indexes of the thumbnail's SOI and EOI markers if the array of bytes contains a thumbnail.
        /// Otherwise unreachable values are returned.
        /// </summary>
        /// <param name="dictionary">Dictionary with marker indexes.</param>
        /// <param name="bytes">Array of bytes the markers were found in.</param>
        /// <returns>Array with start and end index of the thumbnail. First value is the start index.
        ///  Second value is the end index.</returns>
        private uint[] GetThumbnailIndexes(Dictionary<byte, List<uint>> dictionary, byte[] bytes) {
            /* Initialize to unreachable values so evaluation in GetFieldBytes always evaluates to true when there is no thumbnail. */
            uint[] thumbnailIndexes = { Convert.ToUInt32(bytes.Length), Convert.ToUInt32(bytes.Length) };

            if (dictionary[SOI_MARKER].Count > 1) {
                thumbnailIndexes[0] = dictionary[SOI_MARKER][1]; // Second SOI marker
                thumbnailIndexes[1] = dictionary[EOI_MARKER][0]; // First EOI marker
            }

            return thumbnailIndexes;
        }

        /// <summary>
        /// Calculates and returns the length of the field in relation to the given marker 
        /// and index of the specific marker in the given dictionary.
        /// </summary>
        /// <param name="dictionary">Dictionary with marker indexes.</param>
        /// <param name="marker">Marker to calculate length of field for.</param>
        /// <param name="dictionaryMarkerIndex">Index of the specific marker in the dictionary.</param>
        /// <param name="bytes">Array of bytes the markers were found in.</param>
        /// <returns>Length of the field of the given specific marker.</returns>
        private uint GetFieldLength(Dictionary<byte, List<uint>> dictionary, byte marker, int dictionaryMarkerIndex, byte[] bytes) {
            uint markerIndex = dictionary[marker][dictionaryMarkerIndex];
            uint lengthOfFieldIndex = markerIndex + MARKER_LENGTH;

            /* Array of bytes is neccesary in order to convert to uint32. */
            byte[] lengthOfField = { bytes[lengthOfFieldIndex + 1], bytes[lengthOfFieldIndex], 0, 0 }; // Little endian

            return BitConverter.ToUInt32(lengthOfField, 0) - LENGTH_OF_FIELD_LENGTH;
        }

        /// <summary>
        /// Gets the bytes from the given bytes for the given marker with index from the given dictionary.
        /// If the marker occurs multiple times the field bytes are concatenated.
        /// </summary>
        /// <param name="dictionary">Dictionary with marker indexes.</param>
        /// <param name="marker">Marker to get field bytes for.</param>
        /// <param name="thumbnailStartIndex">Start index of the thumbnail.</param>
        /// <param name="thumbnailEndIndex">End index of the thumbnail.</param>
        /// <param name="bytes">Array of bytes the markers were found in.</param>
        /// <returns>Array of bytes containing the bytes for the given marker.</returns>
        private byte[] GetFieldBytes(Dictionary<byte, List<uint>> dictionary, byte marker,
                           uint thumbnailStartIndex, uint thumbnailEndIndex, byte[] bytes) {

            List<uint> markerIndexesList = dictionary[marker];
            byte[] fieldBytes = { };

            for (int i = 0; i < markerIndexesList.Count; i++) {
                if (markerIndexesList[i] > dictionary[SOI_MARKER][0]
                     && markerIndexesList[i] < dictionary[EOI_MARKER][dictionary[EOI_MARKER].Count - 1]
                     && (markerIndexesList[i] < thumbnailStartIndex || markerIndexesList[i] > thumbnailEndIndex)) {
                    uint lengthOfField = GetFieldLength(dictionary, marker, i, bytes);
                    fieldBytes = fieldBytes.Concat(
                        bytes.Skip(Convert.ToInt32(markerIndexesList[i] + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH))
                            .Take(Convert.ToInt32(lengthOfField))).ToArray();
                }
            }

            return fieldBytes;
        }

        /// <summary>
        /// Gets compressed image bytes from the given bytes.
        /// </summary>
        /// <param name="dictionary">Dictionary with marker indexes.</param>
        /// <param name="bytes">Array of bytes the markers were found in.</param>
        /// <returns>Array of bytes containing the compressed image bytes.</returns>
        private byte[] GetCompressedImageBytes(Dictionary<byte, List<uint>> dictionary, byte[] bytes) {
            List<uint> SOSIndexesList = dictionary[SOS_MARKER];

            /* SOS marker can occur multiple times. Always take the last. */
            uint SOSIndex = SOSIndexesList[SOSIndexesList.Count - 1];
            uint lengthOfField = GetFieldLength(dictionary, SOS_MARKER, dictionary[SOS_MARKER].Count - 1, bytes);
            uint compressedImageIndex = SOSIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH + lengthOfField;
            List<uint> EOIIndexesList = dictionary[EOI_MARKER];

            /* Garbage bytes can contain markers. Find the first EOI marker after SOS marker. */
            uint EOIIndex = EOIIndexesList.First(markerIndex => markerIndex > SOSIndex);

            return bytes.Skip(Convert.ToInt32(compressedImageIndex)).Take(Convert.ToInt32(EOIIndex - compressedImageIndex)).ToArray();
        }

        /// <summary>
        /// Saves a JPEG file at the given path based on all contained bytes.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to save.</param>
        public void SaveFile(string path) {
            Contract.Requires<ArgumentNullException>(path != null);
            Contract.Requires<ArgumentException>(path.Length > 0);

            File.WriteAllBytes(path, All);
        }
    }
}
