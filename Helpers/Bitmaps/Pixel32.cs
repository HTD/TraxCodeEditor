using System.Runtime.InteropServices;

namespace Trax.Helpers.Bitmaps {

    /// <summary>
    /// RGBA pixel.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Pixel32 {
        /// <summary>
        /// Blue channel value.
        /// </summary>
        public byte B;
        /// <summary>
        /// Green channel value.
        /// </summary>
        public byte G;
        /// <summary>
        /// Red channel value.
        /// </summary>
        public byte R;
        /// <summary>
        /// Alpha channel value.
        /// </summary>
        public byte A;
    }

}