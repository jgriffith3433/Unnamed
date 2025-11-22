using Unity.Mathematics;

#if PW_STORM_ECS

using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using System;
using UnityEngine.LowLevel;

#endif

/// Copyright 2025 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public static class StormECSUtility
    {
        public static void CreateEntities(object bindingObject, NativeListRef<StormItemData> instanceArray, float3 dataOffset, float dataScale, StormFeatureSettings feature, StormSystemSettings systemSettings, StormQualitySettings qualitySettings)
        {
#if PW_STORM_ECS
            StormECSRenderSystem.Singleton.CreateEntities(bindingObject, instanceArray, dataOffset, dataScale, feature, systemSettings, qualitySettings);
#endif
        }
        public static void RemoveEntities(object bindingObject)
        {
#if PW_STORM_ECS
            StormECSRenderSystem.Singleton.RemoveEntities(bindingObject);
#endif
        }
    }
#if PW_STORM_ECS


    [UpdateInGroup(typeof(TransformSystemGroup))]
    public partial class StormECSRenderSystem : SystemBase
    {
        public class SharedDataGroup : IDisposable
        {
            public NativeArray<Entity>[] Entities;
            //I store this extra reference to the container ref so I can dispose properly when destroying as SharedData also contains a native reference to the data
            public NativeListRef<StormItemData> Instances;
            StormECSRenderSystem ecsSystem;

            public SharedDataGroup(StormECSRenderSystem system, NativeListRef<StormItemData> instanceArray, float3 dataOffset, float dataScale, StormFeatureSettings feature, StormSystemSettings systemSettings, StormQualitySettings qualitySettings)
            {
                ecsSystem = system;
                Instances = instanceArray;
                CreateEntities(instanceArray, dataOffset, dataScale, feature, systemSettings, qualitySettings);
            }

            string LimitLength(string str, int len)
            {
                if (str.Length < len)
                    return str;
                return str.Substring(0, len);
            }

            void CreateEntities(NativeListRef<StormItemData> instanceArray, float3 dataOffset, float dataScale, StormFeatureSettings feature, StormSystemSettings systemSettings, StormQualitySettings qualitySettings)
            {
                if (instanceArray == null || feature.LODs == null || feature.LODs.Length == 0)
                    return;
                if (Instances != instanceArray)
                {
                    Instances = instanceArray;
                    Instances.Use();
                }
                
                if (!ecsSystem.Dependency.IsCompleted)
                    ecsSystem.CompleteDependency();
                RenderBounds bounds = GetBounds(feature);
                List<Entity> parts = new List<Entity>();
                float renderDist = feature.ObjectType switch {
                    StormFeatureType.GRASS => qualitySettings.m_grassDistance,
                    StormFeatureType.DETAIL => qualitySettings.m_detailDistance,
                    StormFeatureType.PLANT => qualitySettings.m_plantDistance,
                    StormFeatureType.TREE => qualitySettings.m_treeDistance,
                    StormFeatureType.SMALL_OBJECT => qualitySettings.m_smallObjDistance,
                    StormFeatureType.MEDIUM_OBJECT => qualitySettings.m_mediumObjDistance,
                    StormFeatureType.LARGE_OBJECT => qualitySettings.m_largeObjDistance,
                    _ => 1,
                };

                for (int lod = 0; lod < feature.LODs?.Length; ++lod)
                {
                    float startDist = 0;
                    if (lod > 0)
                        startDist = renderDist * feature.LODSplits[lod - 1];
                    StormECSLodDataSharedComponent lodSharedData = new StormECSLodDataSharedComponent() {
                        BeginScaleoutDist = renderDist * (1f - feature.ScaleoutRange),
                        EndScaleoutDist = renderDist,
                        EndDist = renderDist,
                        ScaleoutMin = feature.ScaleoutMinScale,
                        StartDist = startDist,
                    };
                    for (int r = 0; r < feature.LODs[lod].renderData.Length; ++r)
                    {
                        StormObjectRenderData rData = feature.LODs[lod].renderData[r];
                        RenderMeshDescription rmd = new RenderMeshDescription(rData.shadowMode, rData.receiveShadows, MotionVectorGenerationMode.Camera);
                        int numMats = rData.Materials.Length;
                        int numS = math.max(numMats, rData.Mesh.subMeshCount);
                        for (int s = 0; s < numS; ++s)
                        {
                            ushort submeshID = (ushort)s;
                            int matID = s;
                            if (s >= numMats)
                            {
                                if (systemSettings.ExtraMaterialMode == StormExtraMaterialMode.NONE)
                                    break;
                                matID = systemSettings.ExtraMaterialMode switch {
                                    StormExtraMaterialMode.LAST => numMats - 1,
                                    StormExtraMaterialMode.LOOP => s % numMats,
                                    _ => numMats - 1,
                                };
                            }
                            if (submeshID > numS)
                            {
                                if (systemSettings.ExtraMaterialMode == StormExtraMaterialMode.NONE)
                                    break;
                                submeshID = (ushort)(systemSettings.ExtraMaterialMode switch {
                                    StormExtraMaterialMode.LAST => numS - 1,
                                    StormExtraMaterialMode.LOOP => s % numS,
                                    _ => numS - 1,
                                });
                            }

                            Entity entity = ecsSystem.EntityManager.CreateEntity(typeof(StormECSEntityDataComponent), typeof(StormECSEntityInstanceComponent));
                            ecsSystem.EntityManager.SetName(entity, new FixedString64Bytes(LimitLength(feature.name, 50) + " L" + lod + " R" + r));
                            RenderMeshArray rma = new RenderMeshArray(rData.Materials, new Mesh[] { rData.Mesh });
                            RenderMeshUtility.AddComponents(entity, ecsSystem.EntityManager, rmd, rma, MaterialMeshInfo.FromRenderMeshArrayIndices(matID, 0, submeshID));
                            ecsSystem.EntityManager.RemoveComponent(entity, typeof(PerInstanceCullingTag));
                            ecsSystem.EntityManager.SetComponentData(entity, new LocalToWorld() {
                                Value = float4x4.TRS(rData.Offset, rData.Rotation, rData.Scale),
                            });
                            ecsSystem.EntityManager.SetComponentData(entity, new StormECSEntityInstanceComponent() {
                                ScaleMult = 1,
                            });
                            ecsSystem.EntityManager.AddSharedComponent(entity, lodSharedData);
                            parts.Add(entity);
                        }
                    }
                }
                //first create all our new entity sets
                Entities = new NativeArray<Entity>[parts.Count];
                for (int e = 0; e < Entities.Length; ++e)
                {
                    Entities[e] = ecsSystem.EntityManager.Instantiate(parts[e], Instances.Length, Allocator.Persistent);
                }
                //then schedule the update job
                for (int e = 0; e < Entities.Length; ++e)
                {
                    UpdateECSTransformDataJob eJob = new UpdateECSTransformDataJob() {
                        DataOffset = dataOffset,
                        DataScale = dataScale,
                        Entities = Entities[e],
                        LocalToWorldLookup = ecsSystem.GetComponentLookup<LocalToWorld>(),
                        RenderBoundsLookup = ecsSystem.GetComponentLookup<RenderBounds>(),
                        WorldRenderBoundsLookup = ecsSystem.GetComponentLookup<WorldRenderBounds>(),
                        EntityDataLookup = ecsSystem.GetComponentLookup<StormECSEntityDataComponent>(),
                        Instances = Instances,
                        Bounds = bounds,
                    };
                    eJob.Run();
                }
                parts.ForEach(ecsSystem.EntityManager.DestroyEntity);
            }

            public void DestroyEntities()
            {
                for (int e = 0; e < Entities?.Length; ++e)
                {
                    if (Entities[e].IsCreated)
                    {
                        ecsSystem.EntityManager.DestroyEntity(Entities[e]);
                        Entities[e].Dispose();
                    }
                }
                Entities = default;
                if (Instances != null)
                {
                    Instances.Dispose();
                    Instances = null;
                }
            }

            public void Update(NativeListRef<StormItemData> instanceArray, float3 dataOffset, float dataScale, StormFeatureSettings feature, StormSystemSettings systemSettings, StormQualitySettings qualitySettings)
            {
                DestroyEntities();
                CreateEntities(instanceArray, dataOffset, dataScale, feature, systemSettings, qualitySettings);
            }

            RenderBounds GetBounds(StormFeatureSettings feature)
            {
                Bounds meshBounds = feature.LODs[0].renderData[0].Mesh.bounds;
                meshBounds.center += (Vector3)feature.LODs[0].renderData[0].Offset;
                meshBounds.size *= feature.LODs[0].renderData[0].Scale;

                for (int s = 1; s < feature.LODs[0]?.renderData.Length; ++s)
                {
                    StormObjectRenderData rData = feature.LODs[0].renderData[s];
                    Mesh mesh = rData?.Mesh;
                    if (mesh == null)
                        continue;
                    Bounds b = mesh.bounds;
                    b.center = Vector3.Scale(b.center + (Vector3)math.rotate(rData.Rotation, rData.Offset), rData.Scale);
                    b.extents = Vector3.Scale(b.extents, rData.Scale);
                    if (meshBounds.extents == Vector3.zero)
                        meshBounds = b;
                    else
                        meshBounds.Encapsulate(b);
                }

                return new RenderBounds() {
                    Value = new AABB() {
                        Center = meshBounds.center,
                        Extents = meshBounds.extents
                    }
                };
            }
            public void Dispose()
            {
                Instances.Dispose();
                Instances = null;
                DestroyEntities();
            }
        }
        Dictionary<object, SharedDataGroup> DataGroups = new Dictionary<object, SharedDataGroup>();

        static StormECSRenderSystem _singleton = null;
        public static StormECSRenderSystem Singleton
        {
            get {
                if (_singleton == null)
                {
                    if (World.DefaultGameObjectInjectionWorld == null)
                    {
                        World.DefaultGameObjectInjectionWorld = DefaultWorldInitialization.Initialize("StormECSWorld", false);
                    }
                   _singleton = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<StormECSRenderSystem>();
                }
                return _singleton;
            }
        }

        public void CreateEntities(object bindingObject, NativeListRef<StormItemData> instanceArray, float3 dataOffset, float dataScale, StormFeatureSettings feature, StormSystemSettings systemSettings, StormQualitySettings qualitySettings)
        {
            if (DataGroups.TryGetValue(bindingObject, out SharedDataGroup group))
                group.Update(instanceArray, dataOffset, dataScale, feature, systemSettings, qualitySettings);
            else
                DataGroups.Add(bindingObject, new SharedDataGroup(this, instanceArray, dataOffset, dataScale, feature, systemSettings, qualitySettings));
            Enabled = true;
            World.Update();
        }

        internal JobHandle JobDependency => Dependency;
        internal EntityQuery CreateEntityQuery(params ComponentType[] types) => GetEntityQuery(types);

        public void RemoveEntities(object bindingObject)
        {
            CompleteDependency();
            if (DataGroups.Remove(bindingObject, out SharedDataGroup group))
            {
                group.Dispose();
            }
            if (DataGroups.Count == 0)
            {
                //last item removed, probably shutting down
                Enabled = false;
            }
        }

        NativeArray<float3> m_cameraPositions;

        protected override void OnCreate()
        {
        }
        
        protected override void OnUpdate()
        {
            ReadOnlySpan<float3> camPos = StormAPI.GetCameraPositions();
            if (!m_cameraPositions.IsCreated || m_cameraPositions.Length != camPos.Length)
            {
                if (m_cameraPositions.IsCreated)
                    m_cameraPositions.Dispose();
                m_cameraPositions = new NativeArray<float3>(camPos.Length, Allocator.Persistent);
            }
            for (int c = 0; c < camPos.Length; ++c)
                m_cameraPositions[c] = camPos[c];
            new StormECSLodUpdateJob() { CameraPositions = m_cameraPositions }.ScheduleParallel();
        }
        
        protected override void OnDestroy()
        {
            foreach (SharedDataGroup group in DataGroups.Values)
                group.Dispose();
            DataGroups.Clear();
            _singleton = null;
        }
    }
#endif
}
