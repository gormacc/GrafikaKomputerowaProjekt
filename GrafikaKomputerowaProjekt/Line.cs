using System.Collections.Generic;
using System.Windows.Shapes;

namespace GrafikaKomputerowaProjekt
{
    public class Line
    {
        public readonly int VerticleOneId;

        public readonly int VerticleTwoId;

        public List<Rectangle> Rectangles = new List<Rectangle>();

        public Line(int verticleOneId, int verticleTwoId, List<Rectangle> listOfRectangles)
        {
            VerticleOneId = verticleOneId;
            VerticleTwoId = verticleTwoId;
            Rectangles = listOfRectangles;
        }

    }
}
