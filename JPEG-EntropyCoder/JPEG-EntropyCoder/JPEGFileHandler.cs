using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using JPEG_EntropyCoder;

namespace StegaProject {
    /// <summary>
    /// This class holds methods to process a JPEG file. The class is able to load and save a JPEG file aswell as gettting markers' data, compressed image data and all data from it.
    /// </summary>
    public class JPEGFileHandler {
        private const byte MARKERLENGTH = 2;
        private const byte FIELDLENGTH = 2;
        private const string DQTMARKER = "FFDB";
        private const string DHTMARKER = "FFC4";
        private const string DRIMARKER = "FFDD";
        private const string SOFMARKER = "FFC0";
        private const string SOSMARKER = "FFDA";
        private const string EOIMARKER = "FFD9";

        private byte[] fileBytes;

        private string RemoveDashes(string s) {
            return s.Replace("-", "");
        }

        private string ReplaceDashesWithSpaces(string s) {
            return s.Replace("-", " ");
        }

        private int FindMarker(string marker, int startIndex = 0) {
            int index = startIndex;
            
            while ( index != -1 && index < fileBytes.Length - 1 &&
                    RemoveDashes( BitConverter.ToString( fileBytes, index, MARKERLENGTH ) ) != marker ) {
                index++;
            }

            if ( index < fileBytes.Length - 1 ) {
                return index;
            }

            return -1;
        }

        private int FindDataStart(string marker, int startIndex = 0) {
            int index = startIndex;

            index = FindMarker(marker, startIndex);
            
            if ( index != -1 ) {
                return index + ( MARKERLENGTH + FIELDLENGTH );
            }

            return -1;
        }

        private List<string> GetFieldData(string marker) {
            List<string> list = new List<string>();

            int index = FindMarker(marker);
            int fieldLength = GetFieldLength(marker);
            int dataStart = FindDataStart(marker);
            
            while ( index != -1 ) {
                list.Add( ReplaceDashesWithSpaces( BitConverter.ToString( fileBytes, dataStart, fieldLength ) ) );

                index++;

                index = FindMarker(marker, index);
                fieldLength = GetFieldLength(marker, index);
                dataStart = FindDataStart(marker, index);
            }

            return list;
        }

        private int GetFieldLength(string marker, int startIndex = 0) {
            int index = FindMarker(marker, startIndex);
            
            return int.Parse( RemoveDashes( BitConverter.ToString( fileBytes, index + MARKERLENGTH, MARKERLENGTH ) ),
                NumberStyles.HexNumber ) - FIELDLENGTH;
        }

        private bool DataContainsThumbnail() {
            int firstIndex = FindMarker(SOSMARKER);

            if (FindMarker(SOSMARKER, firstIndex + MARKERLENGTH) != -1) {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calls the LoadImage method.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to process.</param>
        public JPEGFileHandler( string path ) {
            LoadImage( path );
        }

        /// <summary>
        /// Loads a JPEG file into the class.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to process.</param>
        public void LoadImage( string path ) {
            try {
                fileBytes = File.ReadAllBytes( path );
            } catch ( ArgumentException e ) {
                Console.WriteLine( "Path is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars. Message: " + e.Message );
            } catch ( PathTooLongException e ) {
                Console.WriteLine( "The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. Message: " + e.Message );
            } catch ( DirectoryNotFoundException e ) {
                Console.WriteLine( "The specified path is invalid (for example, it is on an unmapped drive). Message: " + e.Message );
            } catch ( IOException e ) {
                Console.WriteLine( "An I/O error occurred while opening the file. Message: " + e.Message );
            } catch ( UnauthorizedAccessException e ) {
                Console.WriteLine( "This operation is not supported on the current platform. - or - path specified a directory. - or - The caller does not have the required permission. Message: " + e.Message );
            } catch ( NotSupportedException e ) {
                Console.WriteLine( "path is in an invalid format. Message: " + e.Message );
            } catch ( SecurityException e ) {
                Console.WriteLine( "The caller does not have the required permission. Message: " + e.Message );
            }
        }


        /// <summary>
        /// Saves a JPEG file at the given path based on the contained compressed image data.
        /// </summary>
        /// <param name="path">Path to the JPEG file you wish to save.</param>
        public void SaveImage( string path ) {
            try {
                File.WriteAllBytes( path, fileBytes );
            } catch ( ArgumentException e ) {
                Console.WriteLine( "Path is a zero-length string, contains only white space, or contains one or more invalid characters as defined by InvalidPathChars. Message: " + e.Message );
            } catch ( PathTooLongException e ) {
                Console.WriteLine( "The specified path, file name, or both exceed the system-defined maximum length. For example, on Windows-based platforms, paths must be less than 248 characters, and file names must be less than 260 characters. Message: " + e.Message );
            } catch ( DirectoryNotFoundException e ) {
                Console.WriteLine( "The specified path is invalid (for example, it is on an unmapped drive). Message: " + e.Message );
            } catch ( IOException e ) {
                Console.WriteLine( "An I/O error occurred while opening the file. Message: " + e.Message );
            } catch ( UnauthorizedAccessException e ) {
                Console.WriteLine( "This operation is not supported on the current platform. - or - path specified a directory. - or - The caller does not have the required permission. Message: " + e.Message );
            } catch ( NotSupportedException e ) {
                Console.WriteLine( "path is in an invalid format. Message: " + e.Message );
            } catch ( SecurityException e ) {
                Console.WriteLine( "The caller does not have the required permission. Message: " + e.Message );
            }
        }

        /// <summary>
        /// Gets DefineQuantizationTable marker data as a List.
        /// </summary>
        /// <returns>A List containing DefineQuantizationTable marker data.</returns>
        public List<string> GetDQT() {
            return GetFieldData(DQTMARKER);
        }

        /// <summary>
        /// Gets DefineHuffmanTable marker data as a List.
        /// </summary>
        /// <returns>A List containing DefineHuffmanTable marker data.</returns>
        public List<string> GetDHT() {
            return GetFieldData(DHTMARKER);
        }

        /// <summary>
        /// Gets DefineRestartInteroperability marker data as a List.
        /// </summary>
        /// <returns>A List containing DefineRestartInteroperability marker data.</returns>
        public List<string> GetDRI() {
            return GetFieldData(DRIMARKER);
        }

        /// <summary>
        /// Gets StartOfFrame marker data as a List.
        /// </summary>
        /// <returns>A List containing StartOfFrame marker data.</returns>
        public List<string> GetSOF() {
            return GetFieldData(SOFMARKER);
        }

        /// <summary>
        /// Gets StartOfScan marker data as a List.
        /// </summary>
        /// <returns>A List containing StartOfScan marker data.</returns>
        public List<string> GetSOS() {
            return GetFieldData(SOSMARKER);
        }

        /// <summary>
        /// Gets compressed image data as a string.
        /// </summary>
        /// <returns>A string containing compressed image data.</returns>
        public string GetCompressedImageData() {
            int index = FindMarker(SOSMARKER);

            if (DataContainsThumbnail()) {
                index = FindMarker(SOSMARKER, index + MARKERLENGTH);
            }

            int SOSFieldLength = GetFieldLength(SOSMARKER, index);

            int startIndex = index + ( MARKERLENGTH + FIELDLENGTH ) + SOSFieldLength;
            
            while ( RemoveDashes( BitConverter.ToString( fileBytes, index, MARKERLENGTH ) ) != EOIMARKER ) {
                index++;
            }

            int endIndex = index;
            
            return RemoveDashes( BitConverter.ToString( fileBytes, startIndex, endIndex - startIndex ) );
        }

        /// <summary>
        /// Gets all data from MemoryStream as a string.
        /// </summary>
        /// <returns>A string containing all data from MemoryStream.</returns>
        public string GetAllData() {
            return ReplaceDashesWithSpaces( BitConverter.ToString( fileBytes, 0, fileBytes.Length ) );
        }
    }
}
