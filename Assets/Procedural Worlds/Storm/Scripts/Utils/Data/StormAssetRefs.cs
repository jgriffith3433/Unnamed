using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormAssetRefs : ScriptableObject
    {
        [Serializable]
        public struct StormPrefabObjectRef
        {
            [SerializeField]
            public StormFeatureSettings Settings;
            [SerializeField]
            public GameObject Source;
        }
        [Serializable]
        public struct StormMaterialRef
        {
            [SerializeField]
            public Material Original;
            [SerializeField]
            public Shader Shader;
            [SerializeField]
            public Material Updated;
            [NonSerialized]
            public Material Used;
        }
        [Serializable]
        public struct StormMeshRef
        {
            [SerializeField]
            public byte[] ID;
            [SerializeField]
            public Mesh Mesh;
            [SerializeField]
            public Mesh InvMesh;
            [SerializeField]
            public Texture2D HeightMap;
            [SerializeField]
            public long LastMeshFileTime;

            [NonSerialized]
            public bool hasPendingJob;
            [NonSerialized]
            public HeightCreationJobData HeightCreation;
        }
        public class HeightCreationJobData
        {
            public JobHandle Handle;
            public GetMeshHeightJob Job;
            public Action<Texture2D> OnComplete;
        }
        [Serializable]
        public struct StormShaderRef
        {
            [SerializeField]
            public Shader Original;
            [SerializeField]
            public Shader Updated;
            [SerializeField]
            public uint Version;
            [NonSerialized]
            public bool HasAttemptedUpdate;
        }
        [Serializable]
        public struct StormTerrainDataRef
        {
            [SerializeField]
            public UnityEngine.Object Terrain;
            public long LastUpdateTime;

            public Texture2D[] Details;
            public Texture2D[] Textures;
            public Texture2D Height;
            public Texture2D Holes;
            public StormTRSList[] Trees;

            public void Delete()
            {
                if (Height != null)
                    StormUtils.DeleteAsset(Height);
                if (Holes != null)
                    StormUtils.DeleteAsset(Holes);
                for (int d = 0; d < Details.Length; ++d)
                    if (Details[d] != null)
                        StormUtils.DeleteAsset(Details[d]);
                for (int d = 0; d < Textures.Length; ++d)
                    if (Textures[d] != null)
                        StormUtils.DeleteAsset(Textures[d]);
                for (int d = 0; d < Trees.Length; ++d)
                    if (Trees[d] != null)
                        StormUtils.DeleteAsset(Trees[d]);
            }
        }

        [Obsolete("PrefabObjects list is Obsolete. Use StormAssetUtils.GetObjectSettings() instead.")]
        public StormPrefabObjectRef[] PrefabObjects = new StormPrefabObjectRef[0];
        public StormMaterialRef[] Materials = new StormMaterialRef[0];
        public StormMeshRef[] Meshes = new StormMeshRef[0];
        [Obsolete("DetailObjects list is Obsolete. Use StormAssetUtils.GetImpostor() instead.")]
        public Impostor[] Impostors = new Impostor[0];
        public StormShaderRef[] Shaders = new StormShaderRef[0];
        public StormTerrainDataRef[] TerrainDatas = new StormTerrainDataRef[0];

        public static void UpdateMaterial(Material Original, Material Updated, StormSystemSettings systemSettings)
        {
            if (Original != null)
                Updated.CopyPropertiesFromMaterial(Original);
            Updated.enableInstancing = true;
            Updated.EnableKeyword("UNITY_PROCEDURAL_INSTANCING_ENABLED");
            Updated.SetInt("UNITY_PROCEDURAL_INSTANCING_ENABLED", 1);
            Updated.SetFloat("UNITY_PROCEDURAL_INSTANCING_ENABLED", 1);
            Updated.EnableKeyword("PROCEDURAL_INSTANCING_ON");
            Updated.SetInt("PROCEDURAL_INSTANCING_ON", 1);
            Updated.SetFloat("PROCEDURAL_INSTANCING_ON", 1);
            Updated.EnableKeyword("STORM_ENABLED");
            Updated.SetInt("STORM_ENABLED", 1);
            Updated.SetFloat("STORM_ENABLED", 1);
            Updated.EnableKeyword("_STORM_ENABLED");
            Updated.SetInt("_STORM_ENABLED", 1);
            Updated.SetFloat("_STORM_ENABLED", 1);

            StormRP pipeline = StormUtils.GetRenderPipline();
            if (pipeline == StormRP.HDRP)
            {
                UnityEngine.Object diffProfile = systemSettings.DefaultDiffusionProfile;
                if (diffProfile != null && Updated.HasColor("_DiffusionProfileAsset") && Updated.HasFloat("_DiffusionProfileHash"))
                {
                    Color diffProfileAsset = Updated.GetColor("_DiffusionProfileAsset");
                    float diffProfileHash = Updated.GetFloat("_DiffusionProfileHash");
                    if (diffProfileHash == 0 && diffProfileAsset.r == 0 && diffProfileAsset.g == 0 && diffProfileAsset.b == 0 && diffProfileAsset.a == 0)
                    {
                        Vector4 assetID = StormUtils.GetDiffusionProfileAssetID(diffProfile);
                        float assetHash = StormUtils.GetDiffusionProfileHash(diffProfile);
                        Updated.SetColor("_DiffusionProfileAsset", assetID);
                        Updated.SetFloat("_DiffusionProfileHash", assetHash);
                    }
                }
                else if (diffProfile != null && Updated.HasColor("_DiffusionProfile_Asset") && Updated.HasFloat("_DiffusionProfile"))
                {
                    Color diffProfileAsset = Updated.GetColor("_DiffusionProfile_Asset");
                    float diffProfileHash = Updated.GetFloat("_DiffusionProfile");
                    if (diffProfileHash == 0 && diffProfileAsset.r == 0 && diffProfileAsset.g == 0 && diffProfileAsset.b == 0 && diffProfileAsset.a == 0)
                    {
                        Vector4 assetID = StormUtils.GetDiffusionProfileAssetID(diffProfile);
                        float assetHash = StormUtils.GetDiffusionProfileHash(diffProfile);
                        Updated.SetColor("_DiffusionProfile_Asset", assetID);
                        Updated.SetFloat("_DiffusionProfile", assetHash);
                    }
                }
            }
        }


        /// <summary>
        /// Generates a heightmap based on passed mesh generated immediately (use the variant with OnComplete to have it run async)
        /// </summary>
        /// <param name="mesh">Mesh to get heightmap from</param>
        /// <returns></returns>
        public Texture2D GenerateMeshHeight(Mesh mesh, int2 resolution)
        {
            if (mesh == null)
                return null;

            GetMeshHeightJob heightJob = new GetMeshHeightJob() {
                meshVertici = new NativeArray<Vector3>(mesh.vertices, Allocator.TempJob).Reinterpret<float3>(),
                meshTriangles = new NativeArray<int>(mesh.triangles, Allocator.TempJob),
                meshBounds = new float3x2(mesh.bounds.min, mesh.bounds.max),
                OutputHeights = new NativeArray<float>(resolution.x * resolution.y, Allocator.TempJob),
                OutputResolution = resolution,
            };
            heightJob.Schedule().Complete();
            Texture2D ret = StormUtils.GenerateTexture(heightJob.OutputHeights, 0, 1, Color.clear, Color.white, heightJob.OutputResolution, TextureFormat.RFloat);
            heightJob.meshTriangles.Dispose();
            heightJob.meshVertici.Dispose();
            heightJob.OutputHeights.Dispose();
            return ret;
        }

        /// <summary>
        /// Generates a heightmap based on passed mesh generated immediately (use the variant with OnComplete to have it run async)
        /// </summary>
        /// <param name="mesh">Mesh to get heightmap from</param>
        /// <returns></returns>
        public HeightCreationJobData GenerateMeshHeightJob(Mesh mesh, int2 resolution)
        {
            if (mesh == null)
                return default;
            HeightCreationJobData ret = new HeightCreationJobData();
            ret.Job = new GetMeshHeightJob() {
                meshVertici = new NativeArray<Vector3>(mesh.vertices, Allocator.TempJob).Reinterpret<float3>(),
                meshTriangles = new NativeArray<int>(mesh.triangles, Allocator.TempJob),
                meshBounds = new float3x2(mesh.bounds.min, mesh.bounds.max),
                OutputHeights = new NativeArray<float>(),
                OutputResolution = resolution,
            };
            ret.Handle = ret.Job.Schedule();
            if (MeshHeightCoro == null)
            {
                MonoBehaviour coroObject = StormAPI.StormWorldObject;
                if (coroObject == null)
                {
                    createdCoroObject = new GameObject("StormAssetUpdates");
                    createdCoroObject.hideFlags = HideFlags.HideAndDontSave;
                    coroObject = createdCoroObject.AddComponent<StormTransformList>(); //we just need a monobehaviour to attach to ... use one that doesn't cause a bunch of Updates
                }
                MeshHeightCoro = coroObject.StartCoroutine(CoroCheckMeshHeights());
            }
            return ret;
        }

        Coroutine MeshHeightCoro = null;
        GameObject createdCoroObject = null;
        IEnumerator CoroCheckMeshHeights()
        {
            bool isWaiting;
            do
            {
                yield return null;
                isWaiting = false;
                for (int m = 0; m < Meshes.Length; ++m)
                {
                    StormMeshRef mRef = Meshes[m];
                    if (mRef.hasPendingJob)
                    {
                        if (mRef.HeightCreation != null)
                        {
                            if (mRef.HeightCreation.Handle.IsCompleted)
                            {
                                mRef.HeightCreation.Handle.Complete();

                                Texture2D ret = StormUtils.GenerateTexture(mRef.HeightCreation.Job.OutputHeights, 0, 1, Color.clear, Color.white, mRef.HeightCreation.Job.OutputResolution, TextureFormat.RFloat);
                                mRef.HeightCreation.Job.meshTriangles.Dispose();
                                mRef.HeightCreation.Job.meshVertici.Dispose();
                                mRef.HeightCreation.Job.OutputHeights.Dispose();
                                Action<Texture2D> onComplete = mRef.HeightCreation.OnComplete;
                                mRef.hasPendingJob = false;
                                mRef.HeightCreation = null;
                                if (StormUtils.IsEditor())
                                {
                                    string savepath = StormUtils.DefaultSavePath + "Mesh Height/" + StormUtils.EnsureUniqueName(StormUtils.DefaultSavePath + "Mesh Height/", mRef.Mesh.name, "exr");
                                    mRef.HeightMap = StormUtils.SaveTexture(ret, savepath, StormTextureExportFormat.EXR, true);
                                }
                                else
                                    mRef.HeightMap = ret;
                                mRef.LastMeshFileTime = StormUtils.GetAssetWriteTime(mRef.Mesh);
                                try
                                {
                                    onComplete?.Invoke(ret);
                                }
                                catch (Exception ex)
                                {
                                    Debug.LogException(ex);
                                }
                            }
                            else
                                isWaiting = true;
                        }
                        else
                        {
                            mRef.hasPendingJob = false;
                            Meshes[m] = mRef;
                        }
                    }
                }
            } while (isWaiting);
        }
    }
}