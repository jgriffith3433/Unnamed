using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// ForEach Job that checks visibility of tiles
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct CellCullingJob : IJobParallelFor
    {
        /// <summary>
        /// Maximum Distance to calculate shadows for culling
        /// </summary>
        [ReadOnly] public float MaxShadowDist;
        /// <summary>
        /// Distances to draw out objects (Grass, Detail, Plant, and Tree)
        /// </summary>
        [ReadOnly] public float MaxRange;
        /// <summary>
        /// Load ranges for layers
        /// </summary>
        [ReadOnly] public float4x2 LoadRanges;
        /// <summary>
        /// Number of tiles from center of Camera on each axis to keep for Grass, Detail, Plant, Tree layers
        /// </summary>
        [ReadOnly] public float4x2 UnloadRanges;

        /// <summary>
        /// NativeArray containing shared Camera Data for jobs
        /// </summary>
        [ReadOnly] public NativeArray<JobCameraData> CamData;
        /// <summary>
        /// Array of JobTileData for each tile
        /// </summary>
        [ReadOnly] public NativeList<JobRenderCellData> Tiles;

        /// <summary>
        /// Output Array of visibility data for tiles
        /// </summary>
        [WriteOnly] public NativeArray<JobCellLODData> TileVisibility;

        public void Execute(int index)
        {
            if (CamData.Length < 1 || index >= Tiles.Length || index >= TileVisibility.Length)
                return;
            JobRenderCellData tile = Tiles[index];
            JobCellLODData visibility = new JobCellLODData();
            UpdateCulling(index, ref tile, ref visibility);
            visibility.TypesToLoad = GetLoad(ref tile, ref visibility);
            visibility.TypesToUnload = GetUnload(ref tile, ref visibility);
            TileVisibility[index] = visibility;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateCulling(int index, ref JobRenderCellData tile, ref JobCellLODData visibility)
        {
            float3 center = tile.Position;
            float3 extents = tile.Extents;
            float boundsSize = math.length(extents);
            visibility.Index = index;
            visibility.CellIndex = tile.Index;

            GetDistance(center, boundsSize, out visibility.DistanceMin, out visibility.DistanceMax);
            if (visibility.DistanceMin <= 0)
            {
                visibility.Visible = true;
                visibility.FullyVisible = false;
                visibility.ShadowOnly = false;
            }
            else if (visibility.DistanceMin >= MaxRange)
            {
                visibility.Visible = false;
                visibility.FullyVisible = false;
                visibility.ShadowOnly = false;
            }
            else
            {
                visibility.ShadowOnly = false;
                visibility.Visible = CamData[0].IsAABBVisible(center, extents, visibility.DistanceMin, out visibility.FullyVisible);
                //visibility.Visible = visibility.DistanceMin <= boundsSize || (visibility.DistanceMin < MaxRange && CamData[0].IsSphereVisible(center, boundsSize, out visibility.FullyVisible));
                if (!visibility.Visible && math.any(CamData[0].LightDir) && (MaxShadowDist < 0 || visibility.DistanceMin < MaxShadowDist))
                {
                    GetShadowBounds(extents, boundsSize, out float3 shadowBoundsMin, out float3 shadowBoundsMax);
                    extents = (shadowBoundsMax - shadowBoundsMin) * 0.5f;
                    center += (shadowBoundsMin + extents);
                    visibility.ShadowOnly = (visibility.DistanceMin <= 0) || (visibility.DistanceMin < MaxRange && CamData[0].IsAABBVisible(center, extents, visibility.DistanceMin, out bool _));
                }
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void GetDistance(float3 pos, float radius, out float min, out float max)
        {
            if (pos.y == 0) // do distance calc in 2D (by making Y values the same) for unloaded tiles
                pos.y = CamData[0].CamPos.y;
            float dist = math.distance(pos, CamData[0].CamPos);
            min = math.max(dist - radius, 0);
            max = math.max(dist + radius, 0);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void GetShadowBounds(float3 extents, float boundsSize, out float3 objectBoundsMin, out float3 objectBoundsMax)
        {
            objectBoundsMin = -extents;
            objectBoundsMax = extents;

            float denominator = math.dot(CamData[0].LightDir, math.down());
            if (denominator < 0.00001f)
                return;
            float height = objectBoundsMin.y;

            float3 hitPoint = IntersectPlane(math.float3(0, boundsSize, 0), height, denominator);
            objectBoundsMin = math.min(objectBoundsMin, hitPoint);
            objectBoundsMax = math.max(objectBoundsMax, hitPoint);

            hitPoint = IntersectPlane(math.float3(boundsSize, 0, 0), height, denominator);
            objectBoundsMin = math.min(objectBoundsMin, hitPoint);
            objectBoundsMax = math.max(objectBoundsMax, hitPoint);

            hitPoint = IntersectPlane(math.float3(0, 0, boundsSize), height, denominator);
            objectBoundsMin = math.min(objectBoundsMin, hitPoint);
            objectBoundsMax = math.max(objectBoundsMax, hitPoint);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float3 IntersectPlane(float3 position, float height, float denominator)
        {
            float t = math.dot(math.float3(0, height, 0) - position, math.down()) / denominator;
            return position + (CamData[0].LightDir * t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StormFeatureType GetLoad(ref JobRenderCellData tile, ref JobCellLODData vis)
        {
            return
                ((vis.DistanceMin <= LoadRanges.c0.x && (tile.LoadedTypes & StormFeatureType.GRASS) == 0) ? StormFeatureType.GRASS : 0) |
                ((vis.DistanceMin <= LoadRanges.c1.x && (tile.LoadedTypes & StormFeatureType.DETAIL) == 0) ? StormFeatureType.DETAIL : 0) |
                ((vis.DistanceMin <= LoadRanges.c0.y && (tile.LoadedTypes & StormFeatureType.PLANT) == 0) ? StormFeatureType.PLANT : 0) |
                ((vis.DistanceMin <= LoadRanges.c1.y && (tile.LoadedTypes & StormFeatureType.TREE) == 0) ? StormFeatureType.TREE : 0) |
                ((vis.DistanceMin <= LoadRanges.c0.z && (tile.LoadedTypes & StormFeatureType.SMALL_OBJECT) == 0) ? StormFeatureType.SMALL_OBJECT : 0) |
                ((vis.DistanceMin <= LoadRanges.c1.z && (tile.LoadedTypes & StormFeatureType.MEDIUM_OBJECT) == 0) ? StormFeatureType.MEDIUM_OBJECT : 0) |
                ((vis.DistanceMin <= LoadRanges.c0.w && (tile.LoadedTypes & StormFeatureType.LARGE_OBJECT) == 0) ? StormFeatureType.LARGE_OBJECT : 0) |
                ((vis.DistanceMin <= LoadRanges.c1.w && (tile.LoadedTypes & StormFeatureType.COLLIDERS) == 0) ? StormFeatureType.COLLIDERS : 0);

        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StormFeatureType GetUnload(ref JobRenderCellData tile, ref JobCellLODData vis)
        {
            return
                ((vis.DistanceMin > UnloadRanges.c0.x && (tile.LoadedTypes & StormFeatureType.GRASS) != 0) ? StormFeatureType.GRASS : 0) |
                ((vis.DistanceMin > UnloadRanges.c1.x && (tile.LoadedTypes & StormFeatureType.DETAIL) != 0) ? StormFeatureType.DETAIL : 0) |
                ((vis.DistanceMin > UnloadRanges.c0.y && (tile.LoadedTypes & StormFeatureType.PLANT) != 0) ? StormFeatureType.PLANT : 0) |
                ((vis.DistanceMin > UnloadRanges.c1.y && (tile.LoadedTypes & StormFeatureType.TREE) != 0) ? StormFeatureType.TREE : 0) |
                ((vis.DistanceMin > UnloadRanges.c0.z && (tile.LoadedTypes & StormFeatureType.SMALL_OBJECT) != 0) ? StormFeatureType.SMALL_OBJECT : 0) |
                ((vis.DistanceMin > UnloadRanges.c1.z && (tile.LoadedTypes & StormFeatureType.MEDIUM_OBJECT) != 0) ? StormFeatureType.MEDIUM_OBJECT : 0) |
                ((vis.DistanceMin > UnloadRanges.c0.w && (tile.LoadedTypes & StormFeatureType.LARGE_OBJECT) != 0) ? StormFeatureType.LARGE_OBJECT : 0) |
                ((vis.DistanceMin > UnloadRanges.c1.w && (tile.LoadedTypes & StormFeatureType.COLLIDERS) != 0) ? StormFeatureType.COLLIDERS : 0);

        }
    }
}