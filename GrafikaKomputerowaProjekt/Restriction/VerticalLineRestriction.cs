namespace GrafikaKomputerowaProjekt.Restriction
{
    public class VerticalLineRestriction : IRestriction
    {
        public bool CheckRestrictionAvailability(Line v1Line, Line v2Line)
        {
            return v1Line.Restriction.GetType() != typeof(VerticalLineRestriction) && v2Line.Restriction.GetType() != typeof(VerticalLineRestriction);
        }

        public void DrawInformationPic(int v1x, int v2x)
        {
            throw new System.NotImplementedException();
        }
    }
}
