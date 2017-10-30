using System.Windows.Shapes;
using System.Xml.Serialization;

namespace GrafikaKomputerowaProjekt
{
    public class Verticle
    {
        public int X;

        public int Y;

        public int Id;

        [XmlIgnore]
        public Rectangle Rectangle;

        public Verticle()
        {
            
        }

        public Verticle(int id, int x, int y, Rectangle rectangle = null)
        {            
            X = x;
            Y = y;
            Id = id;
            Rectangle = rectangle;
        }

        public void SetNewRectangle(int x, int y, Rectangle rectangle)
        {
            X = x;
            Y = y;
            Rectangle = rectangle;
        }

        public void SetNewCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
