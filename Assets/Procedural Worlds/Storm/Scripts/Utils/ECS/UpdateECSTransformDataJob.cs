#if PW_STORM_ECS

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Rendering;

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// Job that set LocalToWorld matrices from a shared StormItemData array
    /// </summary>
    [BurstCompile]
    public partial struct UpdateECSTransformDataJob : IJob
    {
        /// <summary>
        /// Array of Entities to update
        /// </summary>
        [ReadOnly]
        public NativeArray<Entity> Entities;

        /// <summary>
        /// Shared StormItemData array containing the instance data from the cell
        /// </summary>
        [ReadOnly]
        public NativeList<StormItemData> Instances;

        /// <summary>
        /// Offset of all instance data from world origin
        /// </summary>
        [ReadOnly]
        public float3 DataOffset;
        [ReadOnly]
        public float DataScale;

        [ReadOnly]
        public RenderBounds Bounds;

        /// <summary>
        /// LocalToWorld component to update
        /// </summary>
        public ComponentLookup<LocalToWorld> LocalToWorldLookup;
        /// <summary>
        /// RenderBounds component to update
        /// </summary>
        public ComponentLookup<RenderBounds> RenderBoundsLookup;
        /// <summary>
        /// RenderBounds component to update
        /// </summary>
        public ComponentLookup<WorldRenderBounds> WorldRenderBoundsLookup;
        /// <summary>
        /// RenderBounds component to update
        /// </summary>
        public ComponentLookup<StormECSEntityDataComponent> EntityDataLookup;
        public void Execute()
        {
            for (int i = 0; i < Instances.Length; ++i)
                Execute(i);
        }
        /// <summary>
        /// Execute the job for a single entity
        /// </summary>
        public void Execute(int index)
        {
            // Update the LocalToWorld component
            Entity entity = Entities[index];
            StormItemData itemData = Instances[index];
            float3 Translation = itemData.Translation;
            Translation.xz = itemData.Translation.xz * DataScale;
            Translation += DataOffset;
            quaternion Rotation = itemData.Rotation;
            float3 Scale = itemData.Scale;

            if (LocalToWorldLookup.HasComponent(entity))
            {
                Translation += LocalToWorldLookup[entity].Value.Translation();
                LocalToWorldLookup[entity] = new LocalToWorld {
                    Value =float4x4.TRS(Translation, Rotation, Scale)
                };
            }
            if (RenderBoundsLookup.HasComponent(entity))
            {
                RenderBoundsLookup[entity] = new RenderBounds {
                    Value = new AABB() { Center = Bounds.Value.Center, Extents = Bounds.Value.Extents }
                };
            }
            if (WorldRenderBoundsLookup.HasComponent(entity))
            {
                WorldRenderBoundsLookup[entity] = new WorldRenderBounds {
                    Value = new AABB() { Center = Translation + Bounds.Value.Center, Extents = Bounds.Value.Extents }
                };
            }
            if (EntityDataLookup.HasComponent(entity))
            {
                EntityDataLookup[entity] = new StormECSEntityDataComponent {
                    Translation = Translation,
                    Rotation = Rotation,
                    Scale = Scale
                };
            }
        }
    }
}

#endif