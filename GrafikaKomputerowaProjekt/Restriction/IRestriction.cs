using System.Windows.Controls;

namespace GrafikaKomputerowaProjekt.Restriction
{
    public interface IRestriction
    {
        bool CheckRestrictionAvailability(Line v1Line, Line v2Line);

        Image GetRestrictionPic();

        void ReorganizeLine(Verticle verticleMoved, Verticle secondVerticle);
    }
}
