using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Storm.StructureGenCore;
using UnityEditor;
using System;
using ProceduralWorlds.Storm.Core;


namespace ProceduralWorlds.Storm.AreaPopulator
{
    [RequireComponent(typeof(ObjectInstantiationManager))]
    [RequireComponent(typeof(StormPolygonMask))]
    [ExecuteInEditMode]
    public class DecorationVolume : MonoBehaviour
    {
        public DecorationData decorationData;
        public bool useStorm = false;
        public bool useStormPolygon = false;

        #region AreaSettings
        public List<Vector2> volumePolygon = new List<Vector2>();
        public float height;

        public Vector3 lowestPointGlobal;
        public Vector3 lowestPointLocal;
        #endregion

        #region Subdivision Settings
        private LayerMask collisionLayers;
        private LayerMask terrainLayer;
        private bool checkForOpenings;

        private bool occupyPathways;
        private float openingCheckDistance;
        private float openingPadding;

        private bool checkForGround;
        
        private int cornerCheckDepth = 1;

        [HideInInspector]
        public Vector3 segmentSize;
        #endregion

        #region Population Settings
        public int seed = 3012987;

        public float populationWaitTime;
        public float placementWaitTime;

        #endregion

        #region Init Data
        private bool initalized = false;

        private List<DecorationObjectInfo> decorationObjects = new List<DecorationObjectInfo>();

        public DecorationSegment[,,] volumeSegments;

        public List<DecorationSegment[,,]> corners = new List<DecorationSegment[,,]>();

        public DecorationSegment[,,] center;

        public DecorationSegment[,,] xAxis;

        public DecorationSegment[,,] zAxis;

        public Dictionary<int, List<DecorationSegment[,]>> groundedLevels = new Dictionary<int, List<DecorationSegment[,]>>();

        //this is the padding to the CENTER of the first grid cell
        [HideInInspector]
        public Vector3 gridPadding;

        private int xSteps = 0;
        private int ySteps = 0;
        private int zSteps = 0;
        #endregion

        #region Population Data
        private List<GameObject> childObjects = new List<GameObject>();
        #endregion

        #region Visualization
        public bool showGizmos = false;

        public bool showCorners = false;
        #endregion

        #region Editor Panel States
        [HideInInspector]
        public bool basicSettingsPanelExpanded = false;
        [HideInInspector]
        public bool areaSettingsPanelExpanded = false;
        [HideInInspector]
        public bool populationSettingsPanelExpanded = false;
        [HideInInspector]
        public bool visualizationSettingsPanelExpanded = false;
        #endregion

        [HideInInspector]
        public bool placementDone = true;

        [HideInInspector]
        public XorshiftPlus randomGen;

        public StormPolygonMask polygonObject;

        private float rotationWhenPopualted = 0;

        public ObjectInstantiationManager objectInstantiationManager;

        private void Awake()
        {
            randomGen = new XorshiftPlus(seed);
            if (useStormPolygon)
            {
                if (polygonObject == null)
                {
                    polygonObject = GetComponent<StormPolygonMask>();
                    if (polygonObject == null)
                    {
                        polygonObject = gameObject.AddComponent<StormPolygonMask>();
                    }

                }
            }
        }
        private void Start()
        {
            if (useStormPolygon)
            {
                var polygonData = polygonObject.polygonData;
                var polygonPoints = polygonData.GetPoints(0);

                if (polygonObject == null)
                {
                    polygonObject = GetComponent<StormPolygonMask>();
                    List<Vector2> points = new List<Vector2>();
                    foreach (StormPolygonPoint point in polygonPoints)
                    {
                        points.Add(new Vector2((float)point.x, (float)point.z));
                    }
                    volumePolygon = points;
                }
                else
                {
                    List<Vector2> points = new List<Vector2>();
                    foreach (StormPolygonPoint point in polygonPoints)
                    {
                        points.Add(new Vector2((float)point.x, (float)point.z));
                    }
                    volumePolygon = points;
                }
            }

            objectInstantiationManager = GetComponent<ObjectInstantiationManager>();
        }

        private void Update()
        {
            if (useStormPolygon)
            {
                if (polygonObject == null)
                {
                    polygonObject = GetComponent<StormPolygonMask>();
                }
                else
                {
                    var polygonData = polygonObject.polygonData;
                    var polygonPoints = polygonData.GetPoints(0);
                    List<Vector2> points = new List<Vector2>();
                    foreach (StormPolygonPoint point in polygonPoints)
                    {
                        points.Add(new Vector2((float)point.x, (float)point.z));
                    }
                    volumePolygon = points;
                }
            }

            
        }

        public void Iterate()
        {
            //Debug.Log("Iterating");
            seed = UnityEngine.Random.Range(0, 10000000);
            randomGen = new XorshiftPlus(seed);
            if(volumePolygon.Count < 3)
            {
                Debug.Log("Volume Polygon is invalid");
                return;
            }

            Clear();
            Decorate();
        }

        public List<Vector2> GetVector2PolygonFromStormPolygon()
        {
            if(polygonObject == null)
            {
                polygonObject = GetComponent<StormPolygonMask>();
                if(polygonObject == null)
                {
                    polygonObject = gameObject.AddComponent<StormPolygonMask>();
                }
            }
            if(polygonObject.polygonData == null)
            {
                return new List<Vector2>();
            }

            StormPolygonData polygonData = polygonObject.polygonData;
            StormPolygonPoint[] polygonPoints = polygonData.GetPoints(0);
            if(polygonPoints == null)
            {
                return new List<Vector2>();
            }
            List<Vector2> points = new List<Vector2>();
            foreach (StormPolygonPoint point in polygonPoints)
            {
                points.Add(new Vector2((float)point.x, (float)point.z));
            }
            return points;
        }

        public void ReGenerateAndInitalize()
        {
            Debug.Log("ReGenerating");
            initalized = false;
            Clear();
            Decorate();
        }

        public void ReGenerate()
        {
            Debug.Log("ReGenerating");
            Clear();
            Decorate();
        }

        public void Clear()
        {
            //Debug.Log("Clearing");
            Transform[] children = GetComponentsInChildren<Transform>();
            for (int i = children.Length - 1; i >= 0; i--)
            {
                if (children[i] != transform)
                {
                    DestroyImmediate(children[i].gameObject);
                }
            }
            if(volumeSegments == null)
            {
                return;
            }
            int xSteps = volumeSegments.GetLength(0);
            int ySteps = volumeSegments.GetLength(1);
            int zSteps = volumeSegments.GetLength(2);
            for (int y = 0; y < ySteps; y++)
            {
                for (int x = 0; x < xSteps; x++)
                {
                    for (int z = 0; z < zSteps; z++)
                    {
                        DecorationSegment tempSegment = volumeSegments[x, y, z];
                        if(tempSegment != null)
                        {
                            tempSegment.occupied = false;
                        }
                    }
                }
            }
        }

        public virtual bool IsSegmentValid(Vector3Int currentPlacementTile, int xSteps, int ySteps, int zSteps)
        {
            if (currentPlacementTile.x < 0 || currentPlacementTile.x >= xSteps || currentPlacementTile.y < 0 || currentPlacementTile.y >= ySteps || currentPlacementTile.z < 0 || currentPlacementTile.z >= zSteps)
            {
                return false;
            }
            return true;
        }

        public void Decorate(bool syncPolygon = false)
        {
            rotationWhenPopualted = transform.rotation.eulerAngles.y;
            //Debug.Log("Decorating");
            if(syncPolygon)
            {
                SyncPolygon(volumePolygon);
            }

            if (randomGen == null)
            {
                randomGen = new XorshiftPlus(seed);
            }
            if (decorationData == null)
            {
                Debug.LogError("No Decoration Data To Use");
                return;
            }

            if (initalized == false)
            {

            }
            Initalize();
            if(xSteps > 2000 || xSteps < 0 || ySteps > 2000 || ySteps < 0 || zSteps > 2000 || zSteps < 0)
            {
                return;
            }

            Populate();
        }

        private void SyncPolygon(List<Vector2> polygon)
        {
            if(polygonObject == null)
            {
                polygonObject = GetComponent<StormPolygonMask>();
            }

            StormPolygon stormPolygon = new StormPolygon();

            for (int i = 0; i < polygon.Count; i++)
            {
                polygon[i] -= new Vector2(transform.position.x, transform.position.z);
                stormPolygon.points.Add(new StormPolygonPoint(polygon[i].x, polygon[i].y));
            }
            polygonObject.polygonData.polygons = new List<StormPolygon> { stormPolygon };
            polygonObject.polygonData.m_polygons = new List<StormPolygon> { stormPolygon };


            polygonObject.polygonData.ApplyToStorm();
        
        }

        public void Initalize()
        {
            objectInstantiationManager = GetComponent<ObjectInstantiationManager>();
            collisionLayers = decorationData.collisionLayers;
            terrainLayer = decorationData.terrainLayer;
            segmentSize = new Vector3(decorationData.segmentSize, decorationData.segmentSize, decorationData.segmentSize);
            checkForOpenings = decorationData.checkForOpenings;
            occupyPathways = decorationData.occupyPathways;
            openingPadding = decorationData.openingPadding;
            openingCheckDistance = decorationData.openingCheckDistance;
            checkForGround = decorationData.checkForGround;
            decorationObjects = decorationData.decorationObjects;

            cornerCheckDepth = decorationData.cornerCheckDepth;
            AreaPopulatorCore.Subdivide(volumePolygon, transform, ref lowestPointGlobal, segmentSize, height, ref gridPadding, ref lowestPointLocal,
             ref xSteps, ref ySteps, ref zSteps, ref volumeSegments, openingPadding, collisionLayers, checkForOpenings, occupyPathways, 
             decorationData.pathWidth, checkForGround, ref corners, ref center, ref xAxis, ref zAxis);
            if(xSteps > 2000 || xSteps < 0 || ySteps > 2000 || ySteps < 0 || zSteps > 2000 || zSteps < 0)
            {
                return;
            }

            initalized = true;
        }


        public void Populate()
        {
#if UNITY_EDITOR
            EditorApplication.QueuePlayerLoopUpdate();
#endif
            Action<GameObject> placementFinished = (parent) =>
            {
                placementDone = true;
            };
            
            List<GameObject> spawnedObjects = new List<GameObject>();
            for (int i = 0; i < decorationObjects.Count; i++)
            {
                placementDone = false;
                // Pass all required parameters to the PlaceObject method
                spawnedObjects.AddRange(AreaPopulatorCore.PlaceObject(
                    decorationObjects[i],
                    transform,
                    objectInstantiationManager,
                    lowestPointGlobal,
                    volumePolygon,
                    volumeSegments,
                    segmentSize,
                    gridPadding,
                    collisionLayers,
                    useStorm,
                    groundedLevels,
                    corners,
                    center,
                    xAxis,
                    zAxis,
                    randomGen,
                    null,
                    null,
                    placementFinished));

            }

            foreach(GameObject spawnedObject in spawnedObjects)
            {
                DecorationVolume[] childVolumes = spawnedObject.GetComponentsInChildren<DecorationVolume>();
                foreach (DecorationVolume childVolume in childVolumes)
                {
                    childVolume.Decorate();
                }
            }

        }
        

        

        public void OnDrawGizmos()
        {
            if (showGizmos == false)
            {
                return;
            }
            List<Vector2> roatedPolygon = StructureCreatorUtils.RotatePolygon(volumePolygon, Vector2.zero, -transform.eulerAngles.y * Mathf.Deg2Rad);


            Gizmos.color = Color.magenta;
            int j = roatedPolygon.Count - 1;
            for (int i = 0; i < roatedPolygon.Count; i++)
            {
                Vector3 currentPoint = transform.position + new Vector3(roatedPolygon[i].x, 0, roatedPolygon[i].y);
                Vector3 lastPoint = transform.position + new Vector3(roatedPolygon[j].x, 0, roatedPolygon[j].y);
                Vector3 currentUpPoint = currentPoint + (Vector3.up * height);
                Vector3 lastUpPoint = lastPoint + (Vector3.up * height);
                Gizmos.DrawLine(currentPoint, currentUpPoint);
                Gizmos.DrawLine(lastUpPoint, currentUpPoint);
                Gizmos.DrawLine(lastPoint, currentPoint);
                j = i;
            }

            if (volumeSegments == null)
            {
                return;
            }
            if (volumeSegments.GetLength(0) == 0)
            {
                return;
            }


            if (rotationWhenPopualted != transform.rotation.eulerAngles.y)
            {
                return;
            }
            if (showCorners)
            {
                Gizmos.color = new Color(1, 0, 1, 0.1f);
                if (corners.Count > 0)
                {
                    for (int i = 0; i < corners.Count; i++)
                    {
                        for (int x = 0; x < corners[i].GetLength(0); x++)
                        {
                            for (int y = 0; y < corners[i].GetLength(1); y++)
                            {
                                for (int z = 0; z < corners[i].GetLength(2); z++)
                                {
                                    DecorationSegment segment = corners[i][x, y, z];
                                    if (segment != null)
                                    {
                                        Vector3 placementLocation = segment.worldPosition;
                                        //StructureCreatorUtils.RotatePointAroundPoint(ref placementLocation, transform.position, transform.rotation.eulerAngles.y);
                                        DrawWireCubeRotated(placementLocation, segmentSize, transform.rotation);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            
            for (int x = 0; x < xSteps; x++)
            {
                for (int y = 0; y < ySteps; y++)
                {
                    for (int z = 0; z < zSteps; z++)
                    {
                        DecorationSegment segment = volumeSegments[x, y, z];
                        if (segment != null)
                        {
                            if (segment.occupied)
                            {
                                Gizmos.color = new Color(1, 0, 0, 1f);
                            }
                            else
                            {
                                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.01f);
                            }
                            Vector3 placementLocation = segment.worldPosition;
                            //StructureCreatorUtils.RotatePointAroundPoint(ref placementLocation, transform.position, transform.rotation.eulerAngles.y);
                            DrawWireCubeRotated(placementLocation, segmentSize, transform.rotation);
                        }

                    }
                }
            }


        }

        public void DrawWireCubeRotated(Vector3 position, Vector3 size, Quaternion rotation)
        {
            Vector3[] points = new Vector3[8];
            points[0] = position + rotation * new Vector3(-size.x, -size.y, -size.z) * 0.5f;
            points[1] = position + rotation * new Vector3(-size.x, -size.y, size.z) * 0.5f;
            points[2] = position + rotation * new Vector3(size.x, -size.y, size.z) * 0.5f;
            points[3] = position + rotation * new Vector3(size.x, -size.y, -size.z) * 0.5f;
            points[4] = position + rotation * new Vector3(-size.x, size.y, -size.z) * 0.5f;
            points[5] = position + rotation * new Vector3(-size.x, size.y, size.z) * 0.5f;
            points[6] = position + rotation * new Vector3(size.x, size.y, size.z) * 0.5f;
            points[7] = position + rotation * new Vector3(size.x, size.y, -size.z) * 0.5f;

            Gizmos.DrawLine(points[0], points[1]);
            Gizmos.DrawLine(points[1], points[2]);
            Gizmos.DrawLine(points[2], points[3]);
            Gizmos.DrawLine(points[3], points[0]);

            Gizmos.DrawLine(points[4], points[5]);
            Gizmos.DrawLine(points[5], points[6]);
            Gizmos.DrawLine(points[6], points[7]);
            Gizmos.DrawLine(points[7], points[4]);

            Gizmos.DrawLine(points[0], points[4]);
            Gizmos.DrawLine(points[1], points[5]);
            Gizmos.DrawLine(points[2], points[6]);
            Gizmos.DrawLine(points[3], points[7]);
        }
    }





}







