using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2024 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Reads values from Input and copys any data within area to Output array applying a mask
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct CopyTreesJob : IJob
    {
        [ReadOnly]
        public NativeArray<UnityEngine.TreeInstance> Trees;
        public int TargetPrefabIndex;
        public NativeList<StormItemData> Output;
        public float4 Area;
        [ReadOnly]
        public float TerrainSizeY;
        [ReadOnly] 
        public float3 TileOffset;
        [ReadOnly]
        public float3 TileSize;

        public void Execute()
        {
            int numTrees = Trees.Length;
            float3 up = new float3(0, 1, 0);
            for (int t = 0; t < numTrees; ++t)
            {
                UnityEngine.TreeInstance tree = Trees[t];
                float3 pos = tree.position;
                if (tree.prototypeIndex != TargetPrefabIndex || pos.x < Area.x || pos.x > (Area.x + Area.z) || pos.z < Area.y || pos.z > (Area.y + Area.w))
                    continue;
                pos.x = (pos.x - Area.x) / Area.z;
                pos.z = (pos.z - Area.y) / Area.w;
                pos.y = ((pos.y * TerrainSizeY) - TileOffset.y) / TileSize.y;
                Output.Add(new StormItemData(pos, quaternion.AxisAngle(up, /*math.TODEGREES*/ 57.29578F * tree.rotation), new float3(tree.widthScale, tree.heightScale, tree.widthScale), new float2(0, (t % 32) / 32f)));
            }
            Output.TrimExcess();
        }
    }
} 