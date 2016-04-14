using System;
using System.Globalization;
using System.IO;
using JPEG_EntropyCoder.Exceptions;

namespace JPEG_EntropyCoder {
    /// <summary>
    /// This class holds methods to process a JPEG file. The class is able to load and save a JPEG file aswell as gettting markers' data, compressed image data and all data from it.
    /// </summary>
    public class JPEGFileHandler : IJPEGFileHandler {
        private const byte MARKER_LENGTH = 2;
        private const byte LENGTH_OF_FIELD_LENGTH = 2;
        private const string DQT_MARKER = "FF DB";
        private const string DHT_MARKER = "FF C4";
        private const string SOF0_MARKER = "FF C0";
        private const string SOF2_MARKER = "FF C2";
        private const string SOS_MARKER = "FF DA";
        private const string EOI_MARKER = "FF D9";

        private byte[] fileBytes;

        private string RemoveDashes(string s) {
            return s.Replace("-", "");
        }

        private string ReplaceDashesWithSpaces(string s) {
            return s.Replace("-", " ");
        }

        private int FindMarkerIndex(string marker, int startIndex = 0) {
            int index = startIndex;
            string nextTwoBytes = ReplaceDashesWithSpaces( BitConverter.ToString( fileBytes, index, MARKER_LENGTH ) );

            while ( nextTwoBytes != marker && nextTwoBytes != EOI_MARKER ) {
                index++;
                nextTwoBytes = ReplaceDashesWithSpaces( BitConverter.ToString( fileBytes, index, MARKER_LENGTH ) );
            }
                
            if (nextTwoBytes == EOI_MARKER && marker != EOI_MARKER)
                throw new MarkerNotFoundException("Marker was not found in JPEG file.", marker);

            return index;
        }

        private int CalculateLengthOfField( string marker, int startIndex = 0 ) {
            int lengthOfFieldIndex = FindMarkerIndex( marker, startIndex ) + MARKER_LENGTH;

            return int.Parse( RemoveDashes( BitConverter.ToString( fileBytes, lengthOfFieldIndex, LENGTH_OF_FIELD_LENGTH ) ),
                NumberStyles.HexNumber ) - LENGTH_OF_FIELD_LENGTH;
        }

        private string GetFieldBytes(string marker) {
            int markerIndex = FindMarkerIndex(marker);
            int lengthOfField = CalculateLengthOfField(marker, markerIndex);
            int fieldBytesIndex = markerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH;

            return ReplaceDashesWithSpaces( BitConverter.ToString( fileBytes, fieldBytesIndex, lengthOfField ) );
        }

        private string GetCompressedImageBytes() {
            int SOSMarkerIndex = FindMarkerIndex( SOS_MARKER );
            int lengthOfField = CalculateLengthOfField( SOS_MARKER, SOSMarkerIndex );
            int compressedImageBytesIndex = SOSMarkerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH + lengthOfField;
            int EOIMarkerIndex = FindMarkerIndex( EOI_MARKER, SOSMarkerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH + lengthOfField );

            return BitConverter.ToString( fileBytes, compressedImageBytesIndex, EOIMarkerIndex );
        }

        private string GetAllBytes() {
            return BitConverter.ToString( fileBytes );
        }

        /*
        private bool DataContainsThumbnail() {
            int firstIndex = FindMarkerIndex(SOSMARKER);

            if (FindMarkerIndex(SOSMARKER, firstIndex + MARKERLENGTH) != -1) {
                return true;
            }

            return false;
        }*/

        private string _DQT;
        public string DQT {
            get { return _DQT; }
            set {
                if (value == null)
                    throw new ArgumentNullException();
                _DQT = value;
            }
        }

        private string _DHT;
        public string DHT {
            get { return _DHT; }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _DHT = value;
            }
        }

        private string _SOF;
        public string SOF {
            get { return _SOF; }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _SOF = value;
            }
        }

        private string _SOS;
        public string SOS {
            get { return _SOS; }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _SOS = value;
            }
        }

        private string _compressedImage;
        public string CompressedImage {
            get {
                return _compressedImage;
            }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _compressedImage = value;
            }
        }

        private string _all;
        public string All {
            get { return _all; }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _all = value;
            }
        }

        /// <summary>
        /// Loads a JPEG file into the instance.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to process.</param>
        public JPEGFileHandler( string path ) {
            LoadFile( path );
        }

        /// <summary>
        /// Loads a JPEG file into the instance and sets all properties.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to process.</param>
        public void LoadFile( string path ) {
            fileBytes = File.ReadAllBytes( path );
            DQT = GetFieldBytes( DQT_MARKER );
            DHT = GetFieldBytes( DHT_MARKER );
            SOF = GetFieldBytes( SOF0_MARKER );
            SOS = GetFieldBytes( SOS_MARKER );
            CompressedImage = GetCompressedImageBytes();
            All = GetAllBytes();
        }

        /// <summary>
        /// Saves a JPEG file at the given path based on all contained bytes.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to save.</param>
        public void SaveFile( string path ) {
            File.WriteAllBytes( path, fileBytes );
        }
    }
}
