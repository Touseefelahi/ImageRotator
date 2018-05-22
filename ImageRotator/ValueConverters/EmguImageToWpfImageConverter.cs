using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace ImageRotator
{
    public class EmguImageToWpfImageConverter : BaseValueConverter<EmguImageToWpfImageConverter>
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IImage GraphicsRaw)
            {

                using (Bitmap source = GraphicsRaw.Bitmap)
                {
                    IntPtr ptr = source.GetHbitmap();

                    BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                        ptr,
                        IntPtr.Zero,
                        System.Windows.Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    DeleteObject(ptr);
                    return bs;
                }
            }
            return null;

        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
