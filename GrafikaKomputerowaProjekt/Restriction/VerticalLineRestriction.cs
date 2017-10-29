﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GrafikaKomputerowaProjekt.Properties;

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
            try
            {
                bmp.BeginInit();
                bmp.UriSource = new Uri(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "VerticalLinePic.png" ));
                bmp.EndInit();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }

            return new Image() {Source = bmp};
        }

        public void ReorganizeLine(Verticle verticeMoved, Verticle secondVerticle)
        {
            secondVerticle.X = verticeMoved.X;
        }
    }
}