using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GrafikaKomputerowaProjekt.Restriction
{
    public class LengthStillRestriction : IRestriction
    {
        public bool CheckRestrictionAvailability(Line v1Line, Line v2Line)
        {
            throw new System.NotImplementedException();
        }

        public Image GetRestrictionPic()
        {
            BitmapImage bmp = new BitmapImage();
            try
            {
                bmp.BeginInit();
                bmp.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "LengthStillPic.png"));
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
