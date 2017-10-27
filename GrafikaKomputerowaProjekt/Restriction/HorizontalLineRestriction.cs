using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GrafikaKomputerowaProjekt.Restriction
{
    public class HorizontalLineRestriction : IRestriction
    {
        public bool CheckRestrictionAvailability(Line v1Line, Line v2Line)
        {
            return v1Line.Restriction.GetType() != typeof(HorizontalLineRestriction) && v2Line.Restriction.GetType() != typeof(HorizontalLineRestriction);
        }

        public Image GetRestrictionPic()
        {
            BitmapImage bmp = new BitmapImage
            {
                UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Images/HorizontalLinePic.png"))
            };
            return new Image { Source = bmp };
        }
    }
}
