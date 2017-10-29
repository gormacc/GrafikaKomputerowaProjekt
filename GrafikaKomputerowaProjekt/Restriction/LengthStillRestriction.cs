using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GrafikaKomputerowaProjekt.Restriction
{
    public class LengthStillRestriction : IRestriction
    {
        public int LengthSet { get; set; } = 0;

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

        public void ReorganizeLine(Verticle movedVerticle, Verticle secondVerticle)
        {

            int x1 = movedVerticle.X;
            int x2 = secondVerticle.X;
            int y1 = movedVerticle.Y;
            int y2 = secondVerticle.Y;

            int counter = 0;

            int d, dx, dy, ai, bi, xi, yi;
            int x = x1, y = y1;
            // ustalenie kierunku rysowania
            if (x1 < x2)
            {
                xi = 1;
                dx = x2 - x1;
            }
            else
            {
                xi = -1;
                dx = x1 - x2;
            }
            // ustalenie kierunku rysowania
            if (y1 < y2)
            {
                yi = 1;
                dy = y2 - y1;
            }
            else
            {
                yi = -1;
                dy = y1 - y2;
            }

            // pierwszy piksel
            counter++;

            // oś wiodąca OX
            if (dx > dy)
            {
                ai = (dy - dx) * 2;
                bi = dy * 2;
                d = bi - dx;
                // pętla po kolejnych x
                while (counter++ != LengthSet)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        x += xi;
                    }
                }
            }
            // oś wiodąca OY
            else
            {
                ai = (dx - dy) * 2;
                bi = dx * 2;
                d = bi - dy;
                // pętla po kolejnych y
                while (counter++ != LengthSet)
                {
                    // test współczynnika
                    if (d >= 0)
                    {
                        x += xi;
                        y += yi;
                        d += ai;
                    }
                    else
                    {
                        d += bi;
                        y += yi;
                    }
                }
            }

            //ustawienie x i y
            secondVerticle.X = x;
            secondVerticle.Y = y;
        }
    }
}
