using System;
using System.IO;
using System.Windows;
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
            BitmapImage bmp = new BitmapImage();
            try
            {
                bmp.BeginInit();
                bmp.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "HorizontalLinePic.png"));
                bmp.EndInit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }


            return new Image { Source = bmp };
        }
    }
}
