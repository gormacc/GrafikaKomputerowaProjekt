using System.CodeDom;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml.Serialization;
using GrafikaKomputerowaProjekt.Properties;
using GrafikaKomputerowaProjekt.Restriction;

namespace GrafikaKomputerowaProjekt
{
    public class Line
    {
        public int VerticleOneId;

        public int VerticleTwoId;

        [XmlIgnore]
        public List<Rectangle> Rectangles = new List<Rectangle>();

        [XmlIgnore]
        public IRestriction Restriction = new NoneRestriction();

        public RestrictionEnumToXml RestrictionToSerialize => ConvertRestricionToEnum();

        private RestrictionEnumToXml ConvertRestricionToEnum()
        {
            if(Restriction.GetType() == typeof(HorizontalLineRestriction))
                return RestrictionEnumToXml.HorizontalLine;

            if (Restriction.GetType() == typeof(VerticalLineRestriction))
                return RestrictionEnumToXml.VerticalLine;

            if (Restriction.GetType() == typeof(LengthStillRestriction))
                return RestrictionEnumToXml.StillLength;

            return RestrictionEnumToXml.None;
        }

        [XmlIgnore]
        public Image RestrictionPic = new Image();

        public Line()
        {
            
        }

        public Line(int verticleOneId, int verticleTwoId, List<Rectangle> listOfRectangles)
        {
            VerticleOneId = verticleOneId;
            VerticleTwoId = verticleTwoId;
            Rectangles = listOfRectangles;
        }

        public void EnableClicking()
        {
            int lineMarginOfError = Properties.Settings.Default.LineMarginOfError;
            for (int i = lineMarginOfError; i < Rectangles.Count - lineMarginOfError; i++)
            {
                Rectangles[i].IsHitTestVisible = true;
            }
        }

        public void DisableClicking()
        {
            foreach (var rec in Rectangles)
            {
                rec.IsHitTestVisible = false;
            }
        }
    }

    
}
