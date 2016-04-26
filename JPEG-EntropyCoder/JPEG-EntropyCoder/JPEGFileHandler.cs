using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using JPEG_EntropyCoder.Exceptions;
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
        /* Array with all markers which makes it easy to iterate through all markers. */
        /* Dictionary with all marker indexes. Markers can occur multiple times hence the value is a list. */

        private readonly byte[] SOIMarker = { MARKER_PREFIX, 0xD8 };
        private readonly byte[] DQTMarker = { MARKER_PREFIX, 0xDB };
        private readonly byte[] DHTMarker = { MARKER_PREFIX, 0xC4 };
        private readonly byte[] SOF0Marker = { MARKER_PREFIX, 0xC0 };
        private readonly byte[] SOF2Marker = { MARKER_PREFIX, 0xC2 };
        private readonly byte[] SOSMarker = { MARKER_PREFIX, 0xDA };
        private readonly byte[] EOIMarker = { MARKER_PREFIX, 0xD9 };
        
        private bool compressedImageIsSet;
        private bool allIsSet;
        private bool fileContainsThumbnail;
        private uint thumbnailStartIndex;
        private uint thumbnailEndIndex;
        private uint compressedImageIndex;

        private bool FindMarkerIndex(byte[] marker, out uint markerIndex, uint startIndex = 0, bool firstMarkerInARow = true) {
            uint index = startIndex;
            byte[] nextTwoBytes = { _all[ index ], _all[ index + 1 ] };

            while ( !nextTwoBytes.SequenceEqual( marker ) && !nextTwoBytes.SequenceEqual( EOIMarker ) ) {
                index++;
                nextTwoBytes[ 0 ] = _all[ index ];
                nextTwoBytes[ 1 ] = _all[ index + 1 ];
        /// <summary>
        /// Fills the markerIndexes dictionary with all indexes for all found markers.
        /// </summary>
        /// <param name="dictionary">Dictionary to be filled with marker indexes.</param>
        /// <param name="bytes">Array of bytes that will be searched for markers.</param>
            Contract.Requires<ArgumentNullException>( dictionary != null );
            Contract.Requires<ArgumentNullException>( bytes != null );
            Contract.Requires<ArgumentException>( bytes.Length > 1 );
            }

            markerIndex = index;
                
            if ( firstMarkerInARow && nextTwoBytes.SequenceEqual( EOIMarker ) && !marker.SequenceEqual( EOIMarker ) )
                throw new MarkerNotFoundException("Marker was not found in JPEG file.", marker);

            if ( !firstMarkerInARow && nextTwoBytes.SequenceEqual( EOIMarker ) && !marker.SequenceEqual( EOIMarker ) )
                return false;

            return true;
        }

        private uint CalculateLengthOfField( byte[] marker, uint startIndex = 0 ) {
            uint lengthOfFieldIndex;
            FindMarkerIndex( marker, out lengthOfFieldIndex, startIndex );
            lengthOfFieldIndex += MARKER_LENGTH;
            byte[] lengthOfField = { _all[ lengthOfFieldIndex + 1 ], _all[ lengthOfFieldIndex ], 0, 0 }; // Little endian
            /* Empty the dictionary before finding all indexes.
             * This is done because indexes might have changed after setting compressedImage. */

            return BitConverter.ToUInt32( lengthOfField, 0 ) - LENGTH_OF_FIELD_LENGTH;
            /* Iterate through bytes. If a byte is the marker prefix, check if next byte is either of the markers. */
        }

        private byte[] GetFieldBytes( byte[] marker ) {
            uint markerIndex;
            FindMarkerIndex( marker, out markerIndex );

            if ( fileContainsThumbnail && markerIndex >= thumbnailStartIndex && markerIndex <= thumbnailEndIndex )
                FindMarkerIndex( marker, out markerIndex, thumbnailEndIndex + MARKER_LENGTH );

            uint lengthOfField = CalculateLengthOfField( marker, markerIndex );
            uint fieldBytesIndex = markerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH;
            byte[] fields = _all.Skip( Convert.ToInt32( fieldBytesIndex ) ).Take( Convert.ToInt32( lengthOfField ) ).ToArray();

            while ( FindMarkerIndex( marker, out markerIndex, markerIndex + MARKER_LENGTH, false ) ) {
                lengthOfField = CalculateLengthOfField( marker, markerIndex );
                fieldBytesIndex = markerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH;
                fields = fields.Concat( _all.Skip( Convert.ToInt32( fieldBytesIndex ) ).Take( Convert.ToInt32( lengthOfField ) ) ).ToArray();
        /// <summary>
        /// Returns indexes of the thumbnail's SOI and EOI markers if the array of bytes contains a thumbnail.
        /// Otherwise unreachable values are returned.
        /// </summary>
        /// <param name="dictionary">Dictionary with marker indexes.</param>
        /// <param name="bytes">Array of bytes the markers were found in.</param>
        /// <returns>Array with start and end index of the thumbnail. First value is the start index. Second value is the end index.</returns>
            Contract.Requires<ArgumentNullException>( dictionary != null );
            Contract.Requires<ArgumentException>( dictionary[ SOI_MARKER ].Count > 0 );
            }

            return fields;
        }
            /* Initialize to unreachable values so evaluation in GetFieldBytes always evaluates to true when there is no thumbnail. */

        private uint FindCompressedImageIndex( uint startIndex = 0 ) {
            /* Caching of compressedImageIndex */
            if ( compressedImageIndex != 0 )
                return compressedImageIndex;

            uint SOSMarkerIndex;
            FindMarkerIndex( SOSMarker, out SOSMarkerIndex, startIndex );
            uint lengthOfField = CalculateLengthOfField( SOSMarker, SOSMarkerIndex );

            compressedImageIndex = SOSMarkerIndex + MARKER_LENGTH + LENGTH_OF_FIELD_LENGTH + lengthOfField;
        /// <summary>
        /// Calculates and returns the length of the field in relation to the given marker and index of the specific marker in the given dictionary.
        /// </summary>
        /// <param name="dictionary">Dictionary with marker indexes.</param>
        /// <param name="marker">Marker to calculate length of field for.</param>
        /// <param name="dictionaryMarkerIndex">Index of the specific marker in the dictionary.</param>
        /// <param name="bytes">Array of bytes the markers were found in.</param>
        /// <returns>Length of the field of the given specific marker.</returns>
            Contract.Requires<ArgumentNullException>( dictionary != null );
            Contract.Requires<ArgumentException>( dictionary[ marker ].Count > 0 );
            Contract.Requires<ArgumentNullException>( bytes != null );
            Contract.Requires<ArgumentException>( bytes.Length > 1 );
            Contract.Requires<ArgumentException>( marker == DQT_MARKER || marker == DHT_MARKER || marker == SOF0_MARKER || marker == SOF2_MARKER || marker == SOS_MARKER );
            Contract.Requires<ArgumentException>( dictionaryMarkerIndex > -1 && dictionaryMarkerIndex < dictionary[ marker ].Count );

            return compressedImageIndex;
        }

        private byte[] GetCompressedImageBytes() {
            uint compressedImageBytesIndex = FindCompressedImageIndex();
            /* Array of bytes is neccesary in order to convert to uint32. */

            if ( fileContainsThumbnail && compressedImageBytesIndex >= thumbnailStartIndex &&
                 compressedImageBytesIndex <= thumbnailEndIndex )
                compressedImageBytesIndex = FindCompressedImageIndex( thumbnailEndIndex + MARKER_LENGTH );
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
            Contract.Requires<ArgumentNullException>( dictionary != null );
            Contract.Requires<ArgumentException>( dictionary[ marker ].Count > 0 );
            Contract.Requires<ArgumentNullException>( bytes != null );
            Contract.Requires<ArgumentException>( bytes.Length > 0 );
            Contract.Requires<ArgumentException>( marker == DQT_MARKER || marker == DHT_MARKER || marker == SOF0_MARKER || marker == SOF2_MARKER || marker == SOS_MARKER );

            uint EOIMarkerIndex;
            FindMarkerIndex( EOIMarker, out EOIMarkerIndex, compressedImageBytesIndex );
            
            return _all.Skip( Convert.ToInt32( compressedImageBytesIndex ) ).Take( Convert.ToInt32( EOIMarkerIndex - compressedImageBytesIndex ) ).ToArray();
        }
        
        private void FindThumbnail(out uint startIndex, out uint endIndex) {
            uint firstSOIIndex;
            FindMarkerIndex( SOIMarker, out firstSOIIndex );

            fileContainsThumbnail = FindMarkerIndex( SOIMarker, out startIndex, firstSOIIndex + MARKER_LENGTH, false );
            FindMarkerIndex( EOIMarker, out endIndex );
        /// <summary>
        /// Gets compressed image bytes from the given bytes.
        /// </summary>
        /// <param name="dictionary">Dictionary with marker indexes.</param>
        /// <param name="bytes">Array of bytes the markers were found in.</param>
        /// <returns>Array of bytes containing the compressed image bytes.</returns>
            Contract.Requires<ArgumentException>( dictionary[ SOS_MARKER ].Count > 0 );
            Contract.Requires<ArgumentException>( dictionary[ EOI_MARKER ].Count > 0 );
            Contract.Requires<ArgumentNullException>( bytes != null );
            Contract.Requires<ArgumentException>( bytes.Length > 0 );
        }

        private byte[] _DQT;
        /// <summary>
        /// Contains DQT bytes from the JPEG file.
        /// </summary>
        public byte[] DQT {
            get { return Array.AsReadOnly( _DQT ).ToArray(); }
            private set {
                Contract.Requires<ArgumentNullException>( value != null );
                Contract.Requires<ArgumentException>( value.Length > 0 );

                _DQT = value;
            }
        }

        private byte[] _DHT;
        /// <summary>
        /// Contains DHT bytes from the JPEG file.
        /// </summary>
        public byte[] DHT {
            get { return Array.AsReadOnly( _DHT ).ToArray(); }
            private set {
                Contract.Requires<ArgumentNullException>( value != null );
                Contract.Requires<ArgumentException>( value.Length > 0 );

                _DHT = value;
            }
        }

        private byte[] _SOF;
        /// <summary>
        /// Contains SOF bytes from the JPEG file.
        /// </summary>
        public byte[] SOF {
            get { return Array.AsReadOnly( _SOF ).ToArray(); }
            private set {
                Contract.Requires<ArgumentNullException>( value != null );
                Contract.Requires<ArgumentException>( value.Length > 0 );

                _SOF = value;
            }
        }

        private byte[] _SOS;
        /// <summary>
        /// Contains SOS bytes from the JPEG file.
        /// </summary>
        public byte[] SOS {
            get { return Array.AsReadOnly( _SOS ).ToArray(); }
            private set {
                Contract.Requires<ArgumentNullException>( value != null );
                Contract.Requires<ArgumentException>( value.Length > 0 );

                _SOS = value;
            }
        }

        private byte[] _compressedImage;
        /// <summary>
        /// Contains compressed image bytes from the JPEG file.
        /// </summary>
        public byte[] CompressedImage {
            get { return Array.AsReadOnly( _compressedImage ).ToArray(); }
            set {
                Contract.Requires<ArgumentNullException>( value != null );
                Contract.Requires<ArgumentException>( value.Length > 0 );

                if ( compressedImageIsSet && value.Length != _compressedImage.Length )
                    throw new ArgumentException( "Argument length is not equal to property length and thus cannot be set." );

                if ( compressedImageIsSet ) {
                    uint compressedImageIndex = FindCompressedImageIndex();

                    for ( uint i = 0, j = compressedImageIndex; i < value.Length; i++, j++ ) {
                        All[ j ] = value[ i ];
                    }
                }

                compressedImageIsSet = true;

                _compressedImage = value;
            }
        }

        private byte[] _all;
        /// <summary>
        /// Contains all bytes from the JPEG file.
        /// </summary>
        public byte[] All {
            get { return Array.AsReadOnly( _all ).ToArray(); }
            private set {
                Contract.Requires<ArgumentNullException>( value != null );
                Contract.Requires<ArgumentException>( value.Length > 1 );

                _all = value;
            }
        }

        /// <summary>
        /// Loads bytes from a JPEG file into the instance.
        /// </summary>
        public JPEGFileHandler( string path ) {
            LoadFile( path );
        /// <param name="bytes">Bytes from the JPEG file you wish to process.</param>
            Contract.Requires<ArgumentException>( bytes.Length > 0 );
            /* Image contains either SOF0 or SOF2 - never both. */
        }

        /// <summary>
        /// Loads a JPEG file into the instance.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to process.</param>
        public void LoadFile( string path ) {
            All = File.ReadAllBytes( path );
            DQT = GetFieldBytes( DQTMarker );
            DHT = GetFieldBytes( DHTMarker );
            SOF = GetFieldBytes( SOF0Marker );
            SOS = GetFieldBytes( SOSMarker );
            CompressedImage = GetCompressedImageBytes();
            FindThumbnail( out thumbnailStartIndex, out thumbnailEndIndex );
            Contract.Requires<ArgumentNullException>( path != null );
            Contract.Requires<ArgumentException>( path.Length > 0 );
        }

        /// <summary>
        /// Saves a JPEG file at the given path based on all contained bytes.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to save.</param>
        public void SaveFile( string path ) {
            Contract.Requires<ArgumentNullException>( path != null );
            Contract.Requires<ArgumentException>( path.Length > 0 );

            File.WriteAllBytes( path, All );
        }
    }
}
