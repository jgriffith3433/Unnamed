using System.Collections.Generic;
namespace ProceduralWorlds.Storm.BuildingGen
{
    public class RoofMatrixConstants
    {
        public List<RoofIDMatirx> PeakJoin = new List<RoofIDMatirx>()
        {
            //all larges
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None),
            //all blends
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None),
            //the rest of this is very clunky because i dont have the ability to say hey its fine if its this or that, only direct matches, will likly want to add that functionanity in the future
            //single blends
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None),
            //double blends (this doesnt cover senarios where there is one blend on either side
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None),
            //triple blends
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None),





        };

        public List<RoofIDMatirx> Corner = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.None, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.None,
                CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.Inconsequential),

            new RoofIDMatirx(CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.None, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.None,
                CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.Inconsequential),
        };
        public List<RoofIDMatirx> InnerCorner = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None,
                CellRoofType.Inconsequential, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None,
                CellRoofType.Inconsequential, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),

            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None,
                CellRoofType.Inconsequential, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),

            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None,
                CellRoofType.Inconsequential, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
        };
        public List<RoofIDMatirx> Straight = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.None,
                CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),

            new RoofIDMatirx(CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),

           new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.None,
                CellRoofType.Inconsequential, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),

            new RoofIDMatirx(CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.Inconsequential, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
        };


        public List<RoofIDMatirx> SmallPeak = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential,
                CellRoofType.None, CellRoofType.None,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
        };
        public List<RoofIDMatirx> SmallPeakCap = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.None, CellRoofType.None,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
        };
        public List<RoofIDMatirx> SmallPeakCorner = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.None,
                CellRoofType.None, CellRoofType.Small,
                CellRoofType.None, CellRoofType.None, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.None,
                CellRoofType.None, CellRoofType.Blend,
                CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.None,
                CellRoofType.None, CellRoofType.Inconsequential,
                CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential),
        };


        public List<RoofIDMatirx> StrightBlend = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Small, CellRoofType.None,
                CellRoofType.Large, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Small, CellRoofType.None,
                CellRoofType.Blend, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Small, CellRoofType.None,
                CellRoofType.Large, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Small, CellRoofType.None,
                CellRoofType.Blend, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
        };
        public List<RoofIDMatirx> CornerRightBlend = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Small, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Small, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Small, CellRoofType.Blend,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Small, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.None),
            new RoofIDMatirx(CellRoofType.None, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Large,
                CellRoofType.Inconsequential, CellRoofType.None, CellRoofType.None),
        };

        public List<RoofIDMatirx> CornerLeftBlend = new List<RoofIDMatirx>()
        {
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Large, CellRoofType.Small,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Small,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Blend, CellRoofType.Inconsequential,
                CellRoofType.Blend, CellRoofType.Small,
                CellRoofType.Inconsequential, CellRoofType.Inconsequential, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None,
                CellRoofType.Large, CellRoofType.Small,
                CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None,
                CellRoofType.Large, CellRoofType.Large,
                CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential),
            new RoofIDMatirx(CellRoofType.Inconsequential, CellRoofType.Large, CellRoofType.None,
                CellRoofType.Large, CellRoofType.Blend,
                CellRoofType.None, CellRoofType.None, CellRoofType.Inconsequential),

        };



    }
}
