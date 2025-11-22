using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StormVTTerrainDataContainer", menuName = "Procedural Worlds/Storm/Terrain/StormVTTerrainDataContainer",
        order = 2)]
[Serializable]
public class StormVTTerrainDataContainer : ScriptableObject
{
    /// <summary>
    /// Contains a list of file paths to the Unity TerrainData assets
    /// </summary>
    [SerializeField] public string[] m_terrainDataList;


}
