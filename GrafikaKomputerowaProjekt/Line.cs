using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GrafikaKomputerowaProjekt.Restriction;

namespace GrafikaKomputerowaProjekt
{
    public class Line
    {
        public readonly int VerticleOneId;

        public readonly int VerticleTwoId;

        public List<Rectangle> Rectangles = new List<Rectangle>();

        public IRestriction Restriction = new NoneRestriction();

        public BitmapImage RestrictionPic = new BitmapImage();

        public Line(int verticleOneId, int verticleTwoId, List<Rectangle> listOfRectangles)
        {
            VerticleOneId = verticleOneId;
            VerticleTwoId = verticleTwoId;
            Rectangles = listOfRectangles;
        }

        public void EnableClicking()
        {
            int lineMarginOfError = Properties.Settings.Default.LineMarginOfError;
            for (int i = lineMarginOfError; i < Rectangles.Count - lineMarginOfError; i++)
            {
                Rectangles[i].IsHitTestVisible = true;
            }
        }
    }

    
}
