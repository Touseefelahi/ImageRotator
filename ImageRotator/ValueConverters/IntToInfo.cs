using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace ImageRotator
{
    public class IntToInfo : BaseValueConverter<IntToInfo>
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
          if(value is int counterReferencePoint)
            {
                switch (counterReferencePoint)
                {
                    case 0:
                        return "Select Left Reference Point";
                    case 1:
                        return "Select Right Reference Point";
                    case 2:
                        return "Select Moving Reticle";
                    case 3:
                        return "Select First Point for rotation";
                    case 4:
                        return "Select Second Point for rotation";
                }
            }
            return "Select Left Reference Point";
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
