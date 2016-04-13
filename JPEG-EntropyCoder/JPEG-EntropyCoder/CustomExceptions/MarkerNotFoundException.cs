using System;
using System.Runtime.Serialization;

namespace JPEG_EntropyCoder.CustomExceptions {
    class MarkerNotFoundException : Exception {
        public string Marker { get; }

        public MarkerNotFoundException( string marker ) {
            Marker = marker;
        }

        public MarkerNotFoundException( string message, string marker ) : base( message ) {
            Marker = marker;
        }

        public MarkerNotFoundException( string message, Exception innerException, string marker ) : base( message, innerException ) {
            Marker = marker;
        }

        protected MarkerNotFoundException( SerializationInfo info, StreamingContext context, string marker ) : base( info, context ) {
            Marker = marker;
        }
    }
}
