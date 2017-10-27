namespace GrafikaKomputerowaProjekt.Restriction
{
    public class NoneRestriction : IRestriction
    {
        public bool CheckRestrictionAvailability(Line v1Line, Line v2Line)
        {
            return true;
        }

        public void DrawInformationPic(int v1x, int v2x)
        {
        }
    }
}
