using System.Collections.Generic;

namespace GrafikaKomputerowaProjekt
{
    public class Polygon
    {
        public List<Verticle> Verticles { get; set; } = new List<Verticle>();

        public List<Line> Lines { get; set; } = new List<Line>();

        public Polygon()
        {
            
        }

        public Polygon(List<Verticle> verticles, List<Line> lines)
        {
            Lines = lines;
            Verticles = verticles;
        }
    }
}
