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
    [BurstCompile]
    [WithOptions(EntityQueryOptions.IgnoreComponentEnabledState)]
    public partial struct StormECSLodUpdateJob : IJobEntity
    {
        public NativeArray<float3> CameraPositions;

        public void Execute(EnabledRefRW<MaterialMeshInfo> enabled, ref StormECSEntityInstanceComponent instanceData, ref LocalToWorld localToWorld, in Entity entity, in StormECSLodDataSharedComponent lodData, in StormECSEntityDataComponent entityData)
        {
            float dist = GetClosestDist(entityData.Translation);

            bool shouldBeEnabled = dist >= lodData.StartDist && dist < lodData.EndDist;
            if (shouldBeEnabled != enabled.ValueRW)
            {
                enabled.ValueRW = shouldBeEnabled;
            }
            if (!shouldBeEnabled)
                return;
            float scaleMult = 1;
            if (dist > lodData.BeginScaleoutDist)
                scaleMult = math.clamp(1f - ((dist - lodData.BeginScaleoutDist) / (lodData.EndScaleoutDist - lodData.BeginScaleoutDist)), lodData.ScaleoutMin, 1);

            if (scaleMult != instanceData.ScaleMult)
            {
                instanceData.ScaleMult = scaleMult;
                localToWorld = new LocalToWorld() {
                    Value = float4x4.TRS(entityData.Translation, entityData.Rotation, entityData.Scale * scaleMult),
                };
            }
        }

        float GetClosestDist(float3 Translation)
        {
            float closestDist = float.MaxValue;
            foreach (float3 pos in CameraPositions)
            {
                float d = math.distancesq(pos, Translation);
                if (closestDist > d)
                    closestDist = d;
            }
            return math.sqrt(closestDist);
        }
    }
}

#endif