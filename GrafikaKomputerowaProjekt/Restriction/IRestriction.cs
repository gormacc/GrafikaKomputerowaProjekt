namespace GrafikaKomputerowaProjekt.Restriction
{
    public interface IRestriction
    {
        bool CheckRestrictionAvailability(Line v1Line, Line v2Line);

        void DrawInformationPic(int v1x, int v2x);
    }
}
