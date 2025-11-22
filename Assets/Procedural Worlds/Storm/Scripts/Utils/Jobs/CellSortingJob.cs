/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public struct CellComparer : System.Collections.Generic.IComparer<JobCellLODData>
    {
        public int Compare(JobCellLODData x, JobCellLODData y)
        {
            if ((!x.ShouldDelete) && y.ShouldDelete) //move cells that need deleted to the end of the list so our indexes don't get out of order
                return -1;
            if (x.ShouldDelete && (!y.ShouldDelete))
                return 1;
            if (x.Visible && (!y.Visible)) //we are visible but they arn't. skip
                return -1;

            if ((!x.Visible) && y.Visible || //visible tiles before hidden
                x.DistanceMin > y.DistanceMin) //closer tiles before farther
                return 1;
            if (x.DistanceMin < y.DistanceMin)
                return -1;
            if (x.DistanceMax > y.DistanceMax) //closer tiles before farther
                return 1;
            if (x.DistanceMax < y.DistanceMax)
                return -1;
            return 0;
        }
    }
}