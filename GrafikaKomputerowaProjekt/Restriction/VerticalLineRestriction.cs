using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GrafikaKomputerowaProjekt.Restriction
{
    public class VerticalLineRestriction : IRestriction
    {
        public bool CheckRestrictionAvailability(Line v1Line, Line v2Line)
        {
            return v1Line.Restriction.GetType() != typeof(VerticalLineRestriction) && v2Line.Restriction.GetType() != typeof(VerticalLineRestriction);
        }

        public Image GetRestrictionPic()
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Images", "VerticalLinePic.png"));
            bmp.EndInit();

            return new Image { Source = bmp };
        }
    }
}
