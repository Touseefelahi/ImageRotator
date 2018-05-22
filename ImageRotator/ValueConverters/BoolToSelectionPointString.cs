using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace ImageRotator
{
    public class BoolToSelectionPointString : BaseValueConverter<BoolToSelectionPointString>
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          if(value is bool IsPointA)
            {
                if (IsPointA) return "Select First Point";
                else return "Select Second Point";
            }
            return "Select First Point";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
