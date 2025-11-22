using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralWorlds.Storm.AreaPopulator
{
    public class DecorationObjectInfo : MonoBehaviour
    {
        #region Object Data
        public List<BranchObject> branchObjects = new List<BranchObject>();

        public List<Vector2> objectPolygon = new List<Vector2>();
        public Bounds objectSize = new Bounds(Vector3.zero, Vector3.zero);
        #endregion


        #region Placement Settings
        public PlacementFilter placementFilter;
        public bool occupySpace = true;

        public bool snapToWall = false;
        public bool snapToFloor = false;
        public bool snapToCeiling = false;

        public bool randomizeObjectPos;
        public Vector3 posRandomization;
        public bool randomizeObjectRotation;
        public Vector3 rotRandomization;

        public bool useObjectNoSpawnZone = false;
        public Bounds objectNoSpawnZone = new Bounds(Vector3.zero, Vector3.zero);

        public PlacementType placementType = PlacementType.GameObject;
        #endregion

        #region Editor Panel States
        [HideInInspector]
        public bool objectDataPanelExpanded = false;
        [HideInInspector]
        public bool placementSettingsPanelExpanded = false;
        #endregion

        public void OnDrawGizmosSelected()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.color = new Color(1, 0, 0, 0.1f);
            Gizmos.DrawCube(Vector3.zero, objectSize.size);

            Gizmos.color = Color.blue;
            for (int i = 0; i < objectPolygon.Count; i++)
            {
                Vector2 a1 = objectPolygon[i];
                Vector2 a2 = objectPolygon[(i + 1) % objectPolygon.Count];
                Gizmos.DrawLine(new Vector3(a1.x, 0, a1.y), new Vector3(a2.x, 0, a2.y));
            }

            if(useObjectNoSpawnZone)
            {
                Gizmos.color = new Color(1, 0.92f, 0.16f, 0.1f);
                Gizmos.DrawWireCube(objectNoSpawnZone.center, objectNoSpawnZone.size);
            }
        }

        public void BakeBranchObjects()
        {
#if UNITY_EDITOR
            // Only run in edit mode
            if (Application.isPlaying)
                return;
            
            // Find all direct children with DecorationObjectInfo components
            // Iterate backwards to avoid issues when destroying children
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                DecorationObjectInfo childInfo = child.GetComponent<DecorationObjectInfo>();
                
                if (childInfo != null)
                {
                    // Try to get the prefab asset that this instance was created from
                    GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromOriginalSource(child.gameObject);
                    
                    // Only proceed if there's a valid prefab
                    if (prefabAsset != null)
                    {
                        DecorationObjectInfo prefabInfo = prefabAsset.GetComponent<DecorationObjectInfo>();
                        
                        // Only proceed if the prefab has a DecorationObjectInfo component
                        if (prefabInfo != null)
                        {
                            // Check if this prefab already exists at the same position and rotation in the branchObjects list
                            bool objectExists = false;
                            foreach (BranchObject existingBranch in branchObjects)
                            {
                                if (existingBranch.objectInfo != null && 
                                    existingBranch.objectInfo == prefabInfo &&
                                    Vector3.Distance(existingBranch.relativePosition, child.localPosition) < 0.001f &&
                                    Mathf.Abs(Mathf.DeltaAngle(existingBranch.reletiveRotation, child.localEulerAngles.y)) < 0.001f)
                                {
                                    objectExists = true;
                                    break;
                                }
                            }
                            
                            // If the object doesn't exist at this position and rotation in the list, add it
                            if (!objectExists)
                            {
                                // Create a new branch object for this child's prefab
                                BranchObject branchObject = new BranchObject
                                {
                                    objectInfo = prefabInfo,
                                    ignoreCellsOccupiedByParent = true,
                                    allowNullCellsInSpawns = true,
                                    relativePosition = child.localPosition,
                                    reletiveRotation = child.localEulerAngles.y,
                                    spawnChance = 1f
                                };
                                
                                // Add to the branch objects list
                                branchObjects.Add(branchObject);
                            }
                            
                        }
                        DestroyImmediate(child.gameObject);
                    }
                    
                }
            }
#endif
        }
    
        public void SpawnBranchObjects(bool destroyChildren = true)
        {
#if UNITY_EDITOR
            if(branchObjects.Count == 0)
            {
                return;
            }
            if (destroyChildren)
            {
                // Clear any existing child objects first
                List<GameObject> childrenToDestroy = new List<GameObject>();
                for (int i = 0; i < transform.childCount; i++)
                {
                    DecorationObjectInfo childInfo = transform.GetChild(i).GetComponent<DecorationObjectInfo>();

                    if (childInfo != null)
                    {
                        childrenToDestroy.Add(transform.GetChild(i).gameObject);
                    }
                }

                foreach (GameObject child in childrenToDestroy)
                {
                    DestroyImmediate(child);
                }
            }

            
            // Spawn all objects in the branchObjects list
            foreach (BranchObject branchObject in branchObjects)
            {
                if (branchObject.objectInfo != null)
                {
                    DecorationObjectInfo prefabInfo = branchObject.objectInfo;
                    
                    if (prefabInfo != null)
                    {
                        // Check if we should spawn this object based on spawn chance
                        if (Random.value <= branchObject.spawnChance)
                        {
                            // Get the prefab GameObject
                            GameObject prefab = prefabInfo.gameObject;
                            
                            // Instantiate the prefab as a child of this object
                            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                            if (instance != null)
                            {
                                // Set the parent and position
                                instance.transform.SetParent(transform, false);
                                instance.transform.localPosition = branchObject.relativePosition;
                                instance.transform.localRotation = Quaternion.Euler(0, branchObject.reletiveRotation, 0);
                                
                                // Ensure the name is set properly
                                instance.name = prefab.name;
                            }
                        }
                    }
                }
            }
#endif
        }

    }
    
    public enum PlacementType
    {
        GameObject,
        TerrainTree,
    }
}
