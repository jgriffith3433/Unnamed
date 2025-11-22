using System;
using System.Collections.Generic;
using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    [CreateAssetMenu(menuName = "Procedural Worlds/Storm/Color Targets")]
    public class StormColorTargetData : ScriptableObject
    {
        [System.Serializable]
        public struct ColorTarget
        {
            public Color Color;
            [StormLayer(null, StormLayerRemapMode.FEATURESET)]
            public string Layer;
            [Range(0, 2f)]
            public float Range;
            [Range(0, 2f)]
            public float Falloff;
        }
        [System.Serializable]
        public struct ColorSet
        {
            public string Name;
            public bool AllowRetargeting;
            public ColorTarget[] Colors;
        }

        public ColorSet[] ColorSets;

        //Reference to image used to create / update color list in editor
        public Texture2D ReferenceImage;
        public float ReferenceColorRange = 0.2f;
        public float ReferenceColorFalloff = 0.05f;

        public Action<StormColorTargetData, StormTileData> OnColorTargetsChanged;

        public List<StormTileData> UsedByTiles = new List<StormTileData>();

        public long LastUpdateTS;
    }
}