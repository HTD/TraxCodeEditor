using System.Runtime.InteropServices;

namespace Trax.Helpers.Bitmaps {

    /// <summary>
    /// RGB pixel.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel24 {
        /// <summary>
        /// Blue channel value.
        /// </summary>
        public byte B;
        /// <summary>
        /// Red channel value.
        /// </summary>
        public byte G;
        /// <summary>
        /// Red channel value.
        /// </summary>
        public byte R;
    }

}