using System;
using System.Runtime.Serialization;

namespace JPEG_EntropyCoder.Exceptions {
    class MarkerNotFoundException : Exception {
        public byte[] Marker { get; }

        public MarkerNotFoundException( byte[] marker ) {
            Marker = marker;
        }

        public MarkerNotFoundException( string message, byte[] marker ) : base( message ) {
            Marker = marker;
        }

        public MarkerNotFoundException( string message, Exception innerException, byte[] marker ) : base( message, innerException ) {
            Marker = marker;
        }

        protected MarkerNotFoundException( SerializationInfo info, StreamingContext context, byte[] marker ) : base( info, context ) {
            Marker = marker;
        }
    }
}
