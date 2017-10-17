using System.Collections.Generic;
using System.Windows.Shapes;

namespace GrafikaKomputerowaProjekt
{
    public class Line
    {
        public readonly int VerticleOneId;

        public readonly int VerticleTwoId;

        public readonly List<Rectangle> rectangles = new List<Rectangle>();

        public Line(int verticleOneId, int verticleTwoId, List<Rectangle> listOfRectangles)
        {
            VerticleOneId = verticleOneId;
            VerticleTwoId = verticleTwoId;
            rectangles = listOfRectangles;
        }

    }
}
