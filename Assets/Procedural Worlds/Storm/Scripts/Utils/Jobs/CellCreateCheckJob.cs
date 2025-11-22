using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Checks if Tiles around CameraPos need to be added
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
    public struct CellCreateCheckJob : IJobFilter
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
    public struct CellCreateCheckJob : IJobParallelForFilter
#endif
    {
        /// <summary>
        /// Index of tile Camera is on
        /// </summary>
        [ReadOnly] public int2 CameraCellIndex;
        /// <summary>
        /// Number of tiles on each axis to check
        /// </summary>
        [ReadOnly] public int CellCount;
        /// <summary>
        /// Array of JobTileData for each tile
        /// </summary>
        [ReadOnly] public NativeList<JobRenderCellData> Cells;

        public bool Execute(int index)
        {
            int2 pos = new int2(index % CellCount, index / CellCount) + (CameraCellIndex - CellCount / 2);
            for (int t = 0; t < Cells.Length; ++t)
            {
                if (math.all(Cells[t].Index == pos))
                    return false;
            }
            return true;
        }
    }
}