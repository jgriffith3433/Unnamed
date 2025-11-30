using UnityEngine;
using System.Collections;

/// <summary>
/// Aggressively finds and destroys all terrain GameObjects.
/// </summary>
public class DisableTerrainLoading : MonoBehaviour
{
    private void OnEnable()
    {
        StartCoroutine(DestroyAllTerrainsCoroutine());
    }
    
    private IEnumerator DestroyAllTerrainsCoroutine()
    {
        Debug.Log("DisableTerrainLoading: Starting aggressive terrain destruction...");
        
        // Wait a frame to ensure everything is loaded
        yield return null;
        
        // Method 1: Find by component type
        Terrain[] terrains1 = FindObjectsOfType<Terrain>(true);
        Debug.Log($"DisableTerrainLoading: FindObjectsOfType found {terrains1.Length} terrains");
        foreach (Terrain t in terrains1)
        {
            if (t != null && t.gameObject != null)
            {
                Debug.Log($"DisableTerrainLoading: Destroying terrain '{t.gameObject.name}' via FindObjectsOfType");
                DestroyImmediate(t.gameObject);
            }
        }
        
        yield return null;
        
        // Method 2: Find by name pattern
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        Debug.Log($"DisableTerrainLoading: Searching through {allObjects.Length} GameObjects for terrain names...");
        int nameMatchCount = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("terrain"))
            {
                Debug.Log($"DisableTerrainLoading: Destroying GameObject '{obj.name}' (name contains 'terrain')");
                DestroyImmediate(obj);
                nameMatchCount++;
            }
        }
        Debug.Log($"DisableTerrainLoading: Destroyed {nameMatchCount} objects by name match");
        
        Debug.Log("DisableTerrainLoading: Complete!");
    }
}
