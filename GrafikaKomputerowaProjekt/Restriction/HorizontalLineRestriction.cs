namespace GrafikaKomputerowaProjekt.Restriction
{
    public class HorizontalLineRestriction : IRestriction
    {
        public bool CheckRestrictionAvailability(Line v1Line, Line v2Line)
        {
            return v1Line.Restriction.GetType() != typeof(HorizontalLineRestriction) && v2Line.Restriction.GetType() != typeof(HorizontalLineRestriction);
        }

        public void DrawInformationPic(int v1x, int v2x)
        {
            throw new System.NotImplementedException();
        }
    }
}
