using System.Windows.Shapes;

namespace GrafikaKomputerowaProjekt
{
    public class Verticle
    {
        public int X;

        public int Y;

        public readonly int Id;

        public Rectangle Rectangle;

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
    }
}
