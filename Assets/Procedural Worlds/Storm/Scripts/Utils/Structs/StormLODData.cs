using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [Serializable]
    public class StormObjectRenderData
    {
        /// <summary>
        /// Mesh to render
        /// </summary>
        public Mesh Mesh;
        /// <summary>
        /// List of Materials to render
        /// </summary>
        public Material[] Materials;
        /// <summary>
        /// Offset from center of instance to draw Mesh
        /// </summary>
        public float3 Offset;
        /// <summary>
        /// Rotation to apply to rendered Mesh
        /// </summary>
        public quaternion Rotation;
        /// <summary>
        /// Scale to apply to rendered Mesh
        /// </summary>
        public float3 Scale;
        /// <summary>
        /// Shadow casting mode to use for this LOD level
        /// </summary>
        public ShadowCastingMode shadowMode;
        /// <summary>
        /// Should this LOD level recieve shadows?
        /// </summary>
        public bool receiveShadows;
    }
    /// <summary>
    /// Holds renderer information for a single LOD in a StormObjectSettings object
    /// </summary>
    [Serializable]
    public class StormLODData
    {
        /// <summary>
        /// List of Mesh/Material combos to draw per instance of this LOD
        /// </summary>
        public StormObjectRenderData[] renderData;

        /// <summary>
        /// Should this LOD be disabled? (for debug)
        /// </summary>
        public bool DisableRender;

        public StormLODData(Mesh mesh, Material[] mats, ShadowCastingMode shadowCasting = ShadowCastingMode.On, bool shadows = true)
        {
            renderData = new StormObjectRenderData[1];
            renderData[0] = new StormObjectRenderData();
            renderData[0].Mesh = mesh;
            if (mats != null)
            {
                renderData[0].Materials = new Material[mats.Length];
                Array.Copy(mats, renderData[0].Materials, mats.Length);
            }
            else
                renderData[0].Materials = new Material[0];
            renderData[0].Rotation = quaternion.identity;
            renderData[0].Scale = math.float3(1,1,1);
            renderData[0].shadowMode = shadowCasting;
            renderData[0].receiveShadows = shadows;
            DisableRender = false;
        }
        public StormLODData(StormLODData source)
        {
            renderData = new StormObjectRenderData[source.renderData.Length];
            //copy everything else
            Array.Copy(source.renderData, renderData, renderData.Length);
            //then make materials array a new one instead of copy
            for (int r = 0; r < renderData.Length; ++r)
            {
                renderData[r].Materials = new Material[source.renderData[r].Materials.Length];
                Array.Copy(source.renderData[r].Materials, renderData[r].Materials, renderData[r].Materials.Length);
            }
            DisableRender = false;
        }
        public StormLODData(StormObjectRenderData rData)
        {
            renderData = new StormObjectRenderData[1];
            renderData[0] = new StormObjectRenderData();
            renderData[0].Mesh = rData.Mesh;
            if (rData.Materials != null)
            {
                renderData[0].Materials = new Material[rData.Materials.Length];
                Array.Copy(rData.Materials, renderData[0].Materials, rData.Materials.Length);
            }
            else
                renderData[0].Materials = new Material[0];
            renderData[0].Rotation = quaternion.identity;
            renderData[0].Scale = math.float3(1,1,1);
            renderData[0].shadowMode = rData.shadowMode;
            renderData[0].receiveShadows = rData.receiveShadows;
            DisableRender = false;
        }
        public StormLODData(List<StormObjectRenderData> rData)
        {
            renderData = new StormObjectRenderData[rData.Count];
            for (int r = 0; r < renderData.Length; ++r)
            {
                renderData[r] = new StormObjectRenderData();
                renderData[r].Mesh = rData[r].Mesh;
                if (rData[r].Materials != null)
                {
                    renderData[r].Materials = new Material[rData[r].Materials.Length];
                    Array.Copy(rData[r].Materials, renderData[r].Materials, rData[r].Materials.Length);
                }
                else
                    renderData[r].Materials = new Material[0];
                renderData[r].Offset = rData[r].Offset;
                renderData[r].Rotation = math.normalizesafe(rData[r].Rotation);
                renderData[r].Scale = rData[r].Scale;
                renderData[r].shadowMode = rData[r].shadowMode;
                renderData[r].receiveShadows = rData[r].receiveShadows;
            }
            DisableRender = false;
        }
    }
}