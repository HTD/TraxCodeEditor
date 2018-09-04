using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Trax.Helpers.Bitmaps {

    /// <summary>
    /// Bitmap with direct pixel manipulation support.
    /// </summary>
    /// <typeparam name="TPixel"></typeparam>
    public class DirectBitmap<TPixel> : IDisposable where TPixel : struct {

        /// <summary>
        /// Pixels accessible directly.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = 4)]
        public TPixel[] Pixels;

        /// <summary>
        /// Gets regular GDI+ bitmap.
        /// </summary>
        public Bitmap Bitmap { get; private set; }

        /// <summary>
        /// Gets the flag indicating if the object is disposed.
        /// </summary>
        public bool Disposed { get; private set; }



        /// <summary>
        /// Gets the bitpam width.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the bitmap height.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the bitmap size.
        /// </summary>
        public Size Size {
            get => new Size(Width, Height);
            set { Width = value.Width; Height = value.Height; }
        }

        /// <summary>
        /// Gets or sets pixel format for the bitmap.
        /// </summary>
        public PixelFormat PixelFormat { get; set; }

        /// <summary>
        /// The number of bits per pixel dependent on <see cref="PixelFormat"/>.
        /// </summary>
        public int BitsPerPixel { get; }

        /// <summary>
        /// The number of bytes per pixel dependent on <see cref="PixelFormat"/>.
        /// </summary>
        public int BytesPerPixel { get; }

        /// <summary>
        /// Provides a safe way to access pinned memory.
        /// </summary>
        protected GCHandle PixelsHandle { get; private set; }

        /// <summary>
        /// Creates an empty <see cref="DirectBitmap"/>.
        /// </summary>
        /// <param name="width">Bitmap width.</param>
        /// <param name="height">Bitmap height.</param>
        /// <param name="pixelFormat">Bitmap pixel format. It must match <see cref="TPixel"/> type.</param>
        public DirectBitmap(int width, int height, PixelFormat pixelFormat = PixelFormat.Format32bppArgb) {
            PixelFormat = pixelFormat;
            BitsPerPixel = Image.GetPixelFormatSize(PixelFormat);
            BytesPerPixel = BitsPerPixel / 8;
            Width = width;
            Height = height;
            Pixels = new TPixel[width * height];
            PixelsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * BytesPerPixel, PixelFormat, PixelsHandle.AddrOfPinnedObject());
        }

        /// <summary>
        /// Creates a <see cref="DirectBitmap"/>  from regular GDI+ bitmap.
        /// </summary>
        /// <param name="source">Source GDI+ bitmap.</param>
        /// <param name="dispose">True to dispose source bitmap after copying its data.</param>
        public DirectBitmap(Bitmap source, bool dispose = false) {
            BitsPerPixel = Image.GetPixelFormatSize(PixelFormat = source.PixelFormat);
            BytesPerPixel = BitsPerPixel / 8;
            if (BytesPerPixel > 0) {
                Width = source.Width;
                Height = source.Height;
                Pixels = new TPixel[Width * Height];
                PixelsHandle = GCHandle.Alloc(Pixels, GCHandleType.Pinned);
                Bitmap = new Bitmap(Width, Height, Width * BytesPerPixel, PixelFormat, PixelsHandle.AddrOfPinnedObject());
                var bitmapData = source.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat);
                NativeMethods.CopyMemory(PixelsHandle.AddrOfPinnedObject(), bitmapData.Scan0, Width * Height * BytesPerPixel);
                source.UnlockBits(bitmapData);
                if (dispose) source.Dispose();
            }
            else throw new ArgumentException("The source PixelFormat is not supported.", "source");
        }

        /// <summary>
        /// Disposes unmanaged bitmap memory.
        /// </summary>
        public void Dispose() {
            if (Disposed) return;
            Disposed = true;
            Bitmap.Dispose();
            PixelsHandle.Free();
        }

        public static implicit operator Bitmap(DirectBitmap<TPixel> d) => d.Bitmap;
        public static implicit operator DirectBitmap<TPixel>(Bitmap b) => new DirectBitmap<TPixel>(b);

    }

    /// <summary>
    /// Native methods for <see cref="DirectBitmap{TPixel}"/>.
    /// </summary>
    static class NativeMethods {

        /// <summary>
        /// Copies memory fragment.
        /// </summary>
        /// <param name="dest">Destination pointer.</param>
        /// <param name="src">Source pointer.</param>
        /// <param name="count">Number of bytes to copy.</param>
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, int count);

    }

}