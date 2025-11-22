using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Job to organize StormItemData into a grid, reorder items by grid cell, and calculate cell bounds
    /// </summary>
    [BurstCompile]
#if UNITY_2022_2_OR_NEWER
    [GenerateTestsForBurstCompatibility(CompileTarget = GenerateTestsForBurstCompatibilityAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#else
    [BurstCompatible(CompileTarget = BurstCompatibleAttribute.BurstCompatibleCompileTarget.PlayerAndEditor)]
#endif
    public struct StormGridOrganizeJob : IJob
    {
        [ReadOnly]
        public Bounds ItemBounds;
        [ReadOnly]
        public float2 CellSize;
        [ReadOnly]
        public int SubGridCount;
        [ReadOnly]
        public int SubGridRequirement;
        
        [NativeDisableParallelForRestriction]
        public NativeList<StormItemData> Items;
        
        public NativeArray<StormSubgridData> GridCells;

        public void Execute()
        {
            // Initialize grid cell data
            int totalCells = SubGridCount * SubGridCount;
            for (int i = 0; i < totalCells; i++)
            {
                GridCells[i] = new StormSubgridData
                {
                    ItemCount = 0,
                    Bounds = new float3x2(float.MaxValue, float.MinValue)
                };
            }

            int subGridCount = 1;
            if (Items.Length >= SubGridRequirement)
                subGridCount = SubGridCount;
            else
                totalCells = 1;
            float subgridSize = 1f / subGridCount;

            // First pass: count items per cell and calculate cell bounds
            for (int i = 0; i < Items.Length; i++)
            {
                StormItemData item = Items[i];
                int2 cellCoord = new int2(
                    math.clamp((int)(item.Translation.x / subgridSize), 0, subGridCount - 1),
                    math.clamp((int)(item.Translation.z / subgridSize), 0, subGridCount - 1)
                );
                int cellIndex = cellCoord.y * subGridCount + cellCoord.x;
                
                if (cellIndex >= 0 && cellIndex < totalCells)
                {
                    StormSubgridData cell = GridCells[cellIndex];
                    cell.ItemCount++;
                    
                    // Calculate transformed bounds for this item
                    float3x2 itemTransformedBounds = CalculateTransformedBounds(item);
                    cell.Bounds.c0 = math.min(GridCells[cellIndex].Bounds.c0, itemTransformedBounds.c0);
                    cell.Bounds.c1 = math.max(GridCells[cellIndex].Bounds.c1, itemTransformedBounds.c1);
                    GridCells[cellIndex] = cell;
                }
            }

            // Second pass: reorder items by grid cell
            // Copy original items to temp array
            NativeArray<StormItemData> TempItems = new NativeArray<StormItemData>(Items.AsArray(), Allocator.Temp);

            // Calculate start indices for each cell
            int currentIndex = 0;
            NativeArray<int> CellStartIndices = new NativeArray<int>(totalCells, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            NativeArray<int> CellCounters = new NativeArray<int>(totalCells, Allocator.Temp, NativeArrayOptions.ClearMemory);
            for (int cellIndex = 0; cellIndex < totalCells; cellIndex++)
            {
                CellStartIndices[cellIndex] = currentIndex;
                currentIndex += GridCells[cellIndex].ItemCount;
            }

            // Reorder items by placing them in their correct grid cells
            for (int i = 0; i < Items.Length; i++)
            {
                StormItemData item = TempItems[i];
                int2 cellCoord = new int2(
                    math.clamp((int)(item.Translation.x / subgridSize), 0, subGridCount - 1),
                    math.clamp((int)(item.Translation.z / subgridSize), 0, subGridCount - 1)
                );
                int cellIndex = cellCoord.y * subGridCount + cellCoord.x;
                
                if (cellIndex >= 0 && cellIndex < totalCells)
                {
                    int targetIndex = CellStartIndices[cellIndex] + CellCounters[cellIndex];
                    if (targetIndex < Items.Length)
                    {
                        Items[targetIndex] = item;
                        CellCounters[cellIndex]++;
                    }
                }
            }
            CellStartIndices.Dispose();
            CellCounters.Dispose();
            TempItems.Dispose();
        }

        private float3x2 CalculateTransformedBounds(StormItemData item)
        {
            // Get the 8 corners of the original bounds
            float3 transformedMin = float.MaxValue;
            float3 transformedMax = float.MinValue;

            float3 min = ItemBounds.min;
            float3 max = ItemBounds.max;

            float3 translation = item.Translation;
            translation.xz *= CellSize;
            // Corner 0: (min.x, min.y, min.z)
            float3 corner = new float3(min.x, min.y, min.z);
            float3 transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            // Corner 1: (max.x, min.y, min.z)
            corner = new float3(max.x, min.y, min.z);
            transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            // Corner 2: (min.x, max.y, min.z)
            corner = new float3(min.x, max.y, min.z);
            transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            // Corner 3: (max.x, max.y, min.z)
            corner = new float3(max.x, max.y, min.z);
            transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            // Corner 4: (min.x, min.y, max.z)
            corner = new float3(min.x, min.y, max.z);
            transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            // Corner 5: (max.x, min.y, max.z)
            corner = new float3(max.x, min.y, max.z);
            transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            // Corner 6: (min.x, max.y, max.z)
            corner = new float3(min.x, max.y, max.z);
            transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            // Corner 7: (max.x, max.y, max.z)
            corner = new float3(max.x, max.y, max.z);
            transformed = math.rotate(item.Rotation, corner * item.Scale) + translation;
            transformedMin = math.min(transformedMin, transformed);
            transformedMax = math.max(transformedMax, transformed);

            return new float3x2(transformedMin, transformedMax);
        }
    }
} 