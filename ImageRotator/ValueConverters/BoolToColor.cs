using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace ImageRotator
{
    public class BoolToColor : BaseValueConverter<BoolToColor>
    {

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string Invert)
            {
                if (Invert=="true")
                {
                    if (value is bool IsReferencePointLeft1)
                    {
                        if (IsReferencePointLeft1) return Color.White;
                        else return Color.Green;
                    }
                }
            }
            if (value is bool IsReferencePointLeft)
            {
                if (IsReferencePointLeft) return Color.Green;
                else return Color.White;
            }
            return Color.White;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
