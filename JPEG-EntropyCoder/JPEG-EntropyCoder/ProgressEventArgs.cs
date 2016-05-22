using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPEG_EntropyCoder {
    /// <summary>
    /// A small class to contain a progress for different operations.
    /// </summary>
    public class ProgressEventArgs : EventArgs {
        /// <summary>
        /// The actual progress.
        /// </summary>
        public int Progress { get; }
        /// <summary>
        /// Simple initialize.
        /// </summary>
        /// <param name="progress">Start progress.</param>
        public ProgressEventArgs(int progress) {
            Progress = progress;
        }
    }
}
