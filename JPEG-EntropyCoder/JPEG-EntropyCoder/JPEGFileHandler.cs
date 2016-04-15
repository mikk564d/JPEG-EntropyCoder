using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JPEG_EntropyCoder.Exceptions;
using JPEG_EntropyCoder.Interfaces;

namespace JPEG_EntropyCoder {
    /// <summary>
    /// This class holds methods to process a JPEG file. The class is able to load and save a JPEG file aswell as gettting markers' data, compressed image data and all data from it.
    /// </summary>
    public class JPEGFileHandler : IJPEGFileHandler {
        private const byte MARKER_LENGTH = 2;
        private const byte LENGTH_OF_FIELD_LENGTH = 2;
        private const byte MARKER_PREFIX = 0xFF;

        private readonly byte[] SOIMarker = { MARKER_PREFIX, 0xD8 };
        private readonly byte[] DQTMarker = { MARKER_PREFIX, 0xDB };
        private readonly byte[] DHTMarker = { MARKER_PREFIX, 0xC4 };
        private readonly byte[] SOF0Marker = { MARKER_PREFIX, 0xC0 };
        private readonly byte[] SOF2Marker = { MARKER_PREFIX, 0xC2 };
        private readonly byte[] SOSMarker = { MARKER_PREFIX, 0xDA };
        private readonly byte[] EOIMarker = { MARKER_PREFIX, 0xD9 };

        private bool fileContainsThumbnail;
        private uint thumbnailStartIndex;
        private uint thumbnailEndIndex;
        private Dictionary<byte[], uint> markerIndexes = new Dictionary<byte[], uint>();

        private string RemoveDashes(string s) {
            return s.Replace( "-", "" );
        }

        private string ReplaceDashesWithSpaces(string s) {
            return s.Replace("-", " ");
        }

        private bool FindMarkerIndex(byte[] marker, out uint markerIndex, uint startIndex = 0, bool firstMarkerInARow = true) {
            /* Caching */
            if ( markerIndexes.ContainsKey( marker ) ) {
                markerIndex = markerIndexes[ marker ];
                return true;
            }

            uint index = startIndex;
            byte[] nextTwoBytes = { _all[ index ], _all[ index + 1 ] };

            while ( !nextTwoBytes.SequenceEqual( marker ) && !nextTwoBytes.SequenceEqual( EOIMarker ) ) {
                index++;
                nextTwoBytes[ 0 ] = _all[ index ];
                nextTwoBytes[ 1 ] = _all[ index + 1 ];
            }

            markerIndex = index;
                
            if ( firstMarkerInARow && nextTwoBytes.SequenceEqual( EOIMarker ) && !marker.SequenceEqual( EOIMarker ) )
                throw new MarkerNotFoundException("Marker was not found in JPEG file.", marker);

            if ( !firstMarkerInARow && nextTwoBytes.SequenceEqual( EOIMarker ) && !marker.SequenceEqual( EOIMarker ) )
                return false;

            if ( !markerIndexes.ContainsKey( marker ) )
                markerIndexes.Add( marker, markerIndex );

            return true;
        }

        private uint CalculateLengthOfField( byte[] marker, uint startIndex = 0 ) {
            uint lengthOfFieldIndex;
            FindMarkerIndex( marker, out lengthOfFieldIndex, startIndex );
            lengthOfFieldIndex += MARKER_LENGTH;
            byte[] lengthOfField = { _all[ lengthOfFieldIndex + 1 ], _all[ lengthOfFieldIndex ], 0, 0 }; // Little endian

            return BitConverter.ToUInt32( lengthOfField, 0 ) - LENGTH_OF_FIELD_LENGTH;
        }

        private byte[] GetFieldBytes( byte[] marker ) {
            uint markerIndex;
            FindMarkerIndex( marker, out markerIndex );
            uint lengthOfField = CalculateLengthOfField( marker, markerIndex );
            uint fieldBytesIndex = markerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH;
            byte[] fields = _all.Skip( Convert.ToInt32( fieldBytesIndex ) ).Take( Convert.ToInt32( lengthOfField ) ).ToArray();

            while ( FindMarkerIndex( marker, out markerIndex, markerIndex + MARKER_LENGTH, false ) ) {
                lengthOfField = CalculateLengthOfField( marker, markerIndex );
                fieldBytesIndex = markerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH;
                fields = fields.Concat( _all.Skip( Convert.ToInt32( fieldBytesIndex ) ).Take( Convert.ToInt32( lengthOfField ) ) ).ToArray();
            }

            return fields;
        }

        private byte[] GetCompressedImageBytes() {
            uint SOSMarkerIndex;
            FindMarkerIndex( SOSMarker, out SOSMarkerIndex );
            uint lengthOfField = CalculateLengthOfField( SOSMarker, SOSMarkerIndex );
            uint compressedImageBytesIndex = SOSMarkerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH + lengthOfField;
            uint EOIMarkerIndex;
            FindMarkerIndex( EOIMarker, out EOIMarkerIndex, compressedImageBytesIndex );
            
            return _all.Skip( Convert.ToInt32( compressedImageBytesIndex ) ).Take( Convert.ToInt32( EOIMarkerIndex - compressedImageBytesIndex ) ).ToArray();
        }
        /*
        private bool FindThumbnail(out int startIndex, out int endIndex) {
            int firstSOIIndex;

            return true;
        }

        private void SetFieldBytes(byte[] marker, byte[] bytes) {
            int markerIndex = FindMarkerIndex( marker );
            int lengthOfField = CalculateLengthOfField( marker, markerIndex );
            string[] splitBytes = bytes.Split( ' ' );

            

        }*/

        private byte[] _DQT;
        public byte[] DQT {
            get { return _DQT; }
            set {
                if (value == null)
                    throw new ArgumentNullException();
                _DQT = value;
            }
        }

        private byte[] _DHT;
        public byte[] DHT {
            get { return _DHT; }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _DHT = value;
            }
        }

        private byte[] _SOF;
        public byte[] SOF {
            get { return _SOF; }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _SOF = value;
            }
        }

        private byte[] _SOS;
        public byte[] SOS {
            get { return _SOS; }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _SOS = value;
            }
        }

        private byte[] _compressedImage;
        public byte[] CompressedImage {
            get {
                return _compressedImage;
            }
            set {
                if ( value == null )
                    throw new ArgumentNullException();
                _compressedImage = value;
            }
        }

        private byte[] _all;
        public byte[] All {
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
            All = File.ReadAllBytes( path );
            DQT = GetFieldBytes( DQTMarker );
            DHT = GetFieldBytes( DHTMarker );
            SOF = GetFieldBytes( SOF0Marker );
            SOS = GetFieldBytes( SOSMarker );
            CompressedImage = GetCompressedImageBytes();
        }

        /// <summary>
        /// Saves a JPEG file at the given path based on all contained bytes.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to save.</param>
        public void SaveFile( string path ) {
            File.WriteAllBytes( path, All );
        }
    }
}
