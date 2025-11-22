using UnityEngine;

/// Copyright 2023 Procedural Worlds Ltd
/// Written by Jonathan Williams as part of Storm

namespace ProceduralWorlds.Storm.Utils
{
    public class StormShaderData : ScriptableObject
    {
        public string ShaderVersion = "";
        public bool IsCustom = false;
        public uint ShaderConverterVersion;
    }
}