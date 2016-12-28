using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace Trax.Helpers.Bitmaps {

    /// <summary>
    /// Simple bitmap effects.
    /// </summary>
    public class FX {

        /// <summary>
        /// Converts a bitmap to grayscale.
        /// </summary>
        /// <param name="bitmap">GDI+ bitmap.</param>
        /// <param name="opacity">Target opacity.</param>
        /// <returns>Processed bitmap.</returns>
        public static Bitmap GrayScale(Bitmap bitmap, double opacity = 1) {
            if (opacity < 0) opacity = 0;
            if (opacity > 1) opacity = 1;
            var bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
            DirectBitmap<Pixel24> d24;
            DirectBitmap<Pixel32> d32;
            switch (bpp) {
                case 24:
                    d24 = new DirectBitmap<Pixel24>(bitmap);
                    if (opacity == 1) {
                        for (int i = 0, n = d24.Pixels.Length; i < n; i++) {
                            var pixel = d24.Pixels[i];
                            var y = (byte)(0.21 * pixel.R + 0.72 * pixel.G + 0.07 * pixel.B);
                            pixel.R = y;
                            pixel.G = y;
                            pixel.B = y;
                            d24.Pixels[i] = pixel;
                        }
                        return d24;
                    } else {
                        d32 = new DirectBitmap<Pixel32>(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
                        for (int i = 0, n = d24.Pixels.Length; i < n; i++) {
                            var pixel = d24.Pixels[i];
                            var y = (byte)(0.21 * pixel.R + 0.72 * pixel.G + 0.07 * pixel.B);
                            d32.Pixels[i] = new Pixel32 { R = y, G = y, B = y, A = (byte)(opacity * 255d) };
                        }
                        d24.Dispose();
                        return d32;
                    }
                case 32:
                    d32 = new DirectBitmap<Pixel32>(bitmap);
                    for (int i = 0, n = d32.Pixels.Length; i < n; i++) {
                        var pixel = d32.Pixels[i];
                        var y = (byte)(0.21 * pixel.R + 0.72 * pixel.G + 0.07 * pixel.B);
                        pixel.R = y;
                        pixel.G = y;
                        pixel.B = y;
                        pixel.A = (byte)(d32.Pixels[i].A / 255d * opacity * 255d);
                        d32.Pixels[i] = pixel;
                    }
                    return d32;
                default: throw new NotImplementedException("24-bit and 32-bit images supported only.");
            }
        }

        /// <summary>
        /// Changes image opacity only.
        /// </summary>
        /// <param name="bitmap">GDI+ bitmap.</param>
        /// <param name="opacity">Target opacity.</param>
        /// <returns>Processed bitmap.</returns>
        public static Bitmap SetOpacity(Bitmap bitmap, double opacity = 1) {
            var bpp = Image.GetPixelFormatSize(bitmap.PixelFormat);
            DirectBitmap<Pixel24> d24;
            DirectBitmap<Pixel32> d32;
            switch (bpp) {
                case 24:
                    d24 = new DirectBitmap<Pixel24>(bitmap);
                    d32 = new DirectBitmap<Pixel32>(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
                    for (int i = 0, n = d24.Pixels.Length; i < n; i++) {
                        var pixel = d24.Pixels[i];
                        d32.Pixels[i] = new Pixel32 { R = pixel.R, G = pixel.G, B = pixel.B, A = (byte)(opacity * 255d) };
                    }
                    d24.Dispose();
                    return d32;
                case 32:
                    d32 = new DirectBitmap<Pixel32>(bitmap);
                    for (int i = 0, n = d32.Pixels.Length; i < n; i++) {
                        var pixel = d32.Pixels[i];
                        pixel.A = (byte)(d32.Pixels[i].A / 255d * opacity * 255d);
                        d32.Pixels[i] = pixel;
                    }
                    return d32;
                default: throw new NotImplementedException("24-bit and 32-bit images supported only.");
            }
        }

    }

}