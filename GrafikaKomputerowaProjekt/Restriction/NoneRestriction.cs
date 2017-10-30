using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GrafikaKomputerowaProjekt.Restriction
{
    public class NoneRestriction : IRestriction
    {
        public bool CheckRestrictionAvailability(Line v1Line, Line v2Line)
        {
            return true;
        }

        public Image GetRestrictionPic()
        {
            return new Image();
        }

        public void ReorganizeLine(Verticle verticleOne, Verticle verticleTwo)
        {
            return;
        }

        public bool CheckLastLine(Verticle verticleMoved, Verticle secondVerticle)
        {
            return true;
        }
    }
}
