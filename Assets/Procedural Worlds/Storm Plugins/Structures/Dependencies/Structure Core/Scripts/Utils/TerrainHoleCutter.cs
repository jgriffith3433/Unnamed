using UnityEngine;

public static class TerrainHoleCutter
{
    public static void CutHole(int holeSize, Vector3 position)
    {

        Terrain terrain = GetTerrainAtPoint(position);
        if (terrain == null)
        {
            return;
        }
        CutHoleInTerrain(terrain, holeSize, position);
    }

    public static void FillHole(int holeSize, Vector3 position)
    {
        Terrain terrain = GetTerrainAtPoint(position);
        if (terrain == null)
        {
            return;
        }
        FillHoleInTerrain(terrain, holeSize, position);
    }

    private static void CutHoleInTerrain(Terrain terrain, int holeSize, Vector3 position)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;
        int holeRes = terrainData.holesResolution;
        float holeMultiple = holeRes/ terrainSize.x;
        int adjustedHoleSize = Mathf.CeilToInt(holeSize * holeMultiple);
        bool[,] holes = new bool[adjustedHoleSize, adjustedHoleSize];

        for (int x = 0; x < holes.GetLength(0); x++)
        {
            for (int y = 0; y < holes.GetLength(1); y++)
            {
                holes[x, y] = false;
            }
        }
        /*
        Vector3 pos = terrain.transform.InverseTransformPoint(transform.position - new Vector3(holeSize / 2, 0, holeSize / 2));
        terrainData.SetHoles((int)pos.x / 2, (int)pos.z / 2, holes);
        */
        // Calculate the position in the terrain holes map
        Vector3 pos = terrain.transform.InverseTransformPoint(position) - new Vector3(holeSize/2, 0, holeSize / 2);
        int holePosX =  Mathf.FloorToInt(pos.x * holeMultiple);
        int holePosZ = Mathf.FloorToInt(pos.z * holeMultiple);

        // Set the holes in the terrain
        terrainData.SetHoles(holePosX, holePosZ, holes);
    }

    private static void FillHoleInTerrain(Terrain terrain, int holeSize, Vector3 position)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainSize = terrainData.size;
        int holeRes = terrainData.holesResolution;
        float holeMultiple = holeRes / terrainSize.x;
        int adjustedHoleSize = Mathf.CeilToInt(holeSize * holeMultiple);
        bool[,] holes = new bool[adjustedHoleSize, adjustedHoleSize];

        for (int x = 0; x < holes.GetLength(0); x++)
        {
            for (int y = 0; y < holes.GetLength(1); y++)
            {
                holes[x, y] = true;
            }
        }
        /*
        Vector3 pos = terrain.transform.InverseTransformPoint(transform.position - new Vector3(holeSize / 2, 0, holeSize / 2));
        terrainData.SetHoles((int)pos.x / 2, (int)pos.z / 2, holes);
        */
        // Calculate the position in the terrain holes map
        Vector3 pos = terrain.transform.InverseTransformPoint(position) - new Vector3(holeSize / 2, 0, holeSize / 2);
        int holePosX = Mathf.FloorToInt(pos.x * holeMultiple);
        int holePosZ = Mathf.FloorToInt(pos.z * holeMultiple);

        // Set the holes in the terrain
        terrainData.SetHoles(holePosX, holePosZ, holes);
    }

    private static Terrain GetTerrainAtPoint(Vector3 point)
    {
        
        // Get all active terrains in the scene
        Terrain[] terrains = Terrain.activeTerrains;

        foreach (Terrain terrain in terrains)
        {
            // Check if the point is within the terrain's bounds
            if (IsPointInsideTerrain(terrain, point))
            {
                return terrain;
            }
        }


        // Return null if no terrain contains the point
        return null;
    }

    private static bool IsPointInsideTerrain(Terrain terrain, Vector3 point)
    {
        if (terrain == null || terrain.terrainData == null)
        {
            return false;
        }

        // Get the terrain's position and size
        Vector3 terrainBottomLeftPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        Vector3 terrainTopRightPos = terrainBottomLeftPos + terrainSize;


        if(point.x < terrainBottomLeftPos.x || point.x > terrainTopRightPos.x)
        {
            return false;
        }
        else if (point.z < terrainBottomLeftPos.z || point.z > terrainTopRightPos.z)
        {
            return false;
        }
        return true;
    }
}
