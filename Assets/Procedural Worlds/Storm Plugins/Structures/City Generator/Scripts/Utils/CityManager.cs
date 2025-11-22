using System.Collections.Generic;
using UnityEngine;


#if GENA_PRO
using GeNa.Core;
#endif


using ProceduralWorlds.Storm.StructureGenCore;
using ProceduralWorlds.Storm.BuildingGen;

#if GAIA_2023 || GAIA_2023_PRO
using Gaia;
#endif

using XorshiftPlus = ProceduralWorlds.Storm.StructureGenCore.XorshiftPlus;
#if GAIA_2023
using MultiTerrainOperationType = Gaia.MultiTerrainOperationType;
using ImageProcessing = Gaia.ImageProcessing;
#endif
using UnityEditor;


#region EasyRoads RoadGen
#if EASYROADS3D_PRO
using EasyRoads3Dv3;
#endif
#endregion

namespace ProceduralWorlds.Storm.CityBuilder
{
    public class CityManager : MonoBehaviour
    {
        #region Generation Settings
        public List<Vector2> cityArea = new List<Vector2>();
        public bool autoGenSeed = true;

        public int m_seed;

        public int selectedDistrictInt = 0;
        public List<CityDistrict> m_cityDistricts;

        public GlobalRoadSettings m_defaultMajorRoadSettings;
        public MajorRoadStartType majorRoadStartType;
        [SerializeField]
        public List<RadialRoadPoint> majorRadialPoints = new List<RadialRoadPoint>();

        public GlobalRoadSettings m_defaultMinorRoadSettings;

        [SerializeField]
        public List<RadialRoadPoint> minorRadialPoints = new List<RadialRoadPoint>();

        #region RoadGen
        public CityRoadSpawnType cityRoadSpawnType;

#if EASYROADS3D_PRO
        public GameObject TConnectionObjMajor;
        public GameObject XConnectionObjMajor;
        public GameObject LConnectionObjMajor;
        public GameObject TConnectionObjMinor;
        public GameObject XConnectionObjMinor;
        public GameObject LConnectionObjMinor;

        public List<string> majorRoadSideObjects = new List<string>();
        public List<string> minorRoadSideObjects = new List<string>();

        private ERConnection TConnectionMajor;
        private ERConnection XConnectionMajor;
        private ERConnection LConnectionMajor;
        private ERConnection TConnectionMinor;
        private ERConnection XConnectionMinor;
        private ERConnection LConnectionMinor;
#endif
#endregion

        public Texture2D m_populationMap;
        public bool overrideWaterLevel = false;
        public float waterLevel;
        public Texture2D m_waterMap;
        public Texture2D m_slopeMap;

        public GeneratedBuildingParent buildingParentPrefab;

        public List<StormPolygonLayerSettings> polygonMaskLayerSettings;
        #region Storm Settings
        public bool useStorm;
        #endregion
        #endregion


        public GameObject cityPrefab;

        private BuildingGenerator m_buildingGenerator;


        #region Visualization Settings
        public bool showGizmos;
        public bool autoUpdatePreviewOnVarChange = false;
        public bool showBlockPlots = true;
        [HideInInspector]
        public bool polygonPanelExpanded = true;
        [HideInInspector]
        public bool generationPanelExpanded = true;
        [HideInInspector]
        public bool visualizationPanelExpanded = true;
        public bool showAllowedSlope = true;
        #endregion


        #region Private Variables
        //Private Variables

        Vector3CompareBehavior m_vector3Comparer = new Vector3CompareBehavior();

        private CityDistrict[,] m_cityDistrictMap;
        private Vector2 m_cityDistrictMapOrigin;
        private Vector2 m_cityDistrictMapEnd;

        public RoadNetworkData roadNetworkData;

        public List<BlockInfo> blockInfo = new List<BlockInfo>();

        public List<LandPlot> exteriorPlots = new List<LandPlot>();


        private XorshiftPlus m_randomGenerator;

        public List<Vector3> pointStorage = new List<Vector3>();

        private List<GameObject> spawnedObjects = new List<GameObject>();
#if GENA_PRO
        private Spline roadSpline;
#endif
#endregion

#if Storm_Enabled
        private StormWorld stormWorld;
        private List<Vector3[]> stormRoadMasks = new List<Vector3[]>();
#endif


        #region Visualization Varaibles
        public bool m_previewDirty;
        public bool m_showSeaLevelinStampPreview = true;
        public bool m_showSeaLevelPlane = true;
        #endregion

        private RenderTexture m_cachedPreviewColorRenderTexture;

        public Color slopePreviewColor = Color.red;
        public Color seaColor = Color.blue;

        public float minSlope;
        public float maxSlope;

#if GAIA_2023
        public ImageMask[] imageMasks;
#endif


        private void OnDrawGizmos()
        {
            if (showGizmos == false)
            {
                return;
            }

#if UNITY_EDITOR
            if (Selection.activeObject == gameObject)
            {
                if (showAllowedSlope)
                {
                    DrawMaskMapPreview(false);
                }

                //Water
                if (m_showSeaLevelPlane)
                {
#if GAIA_2023
                    BoundsDouble bounds = new BoundsDouble();
                    if (TerrainHelper.GetTerrainBounds(ref bounds) == true)
                    {
                        bounds.center = new Vector3Double(bounds.center.x, (double)waterLevel, bounds.center.z);
                        bounds.size = new Vector3Double(bounds.size.x, 0.05f, bounds.size.z);
                        Gizmos.color = new Color(Color.blue.r, Color.blue.g, Color.blue.b, Color.blue.a / 4f);
                        Gizmos.DrawCube(bounds.center, bounds.size);
                    }
#endif

                }
            }
#endif

            DrawRoadNetwork();


            if (showBlockPlots)
            {
                DrawBlockPlots();
            }


        }

        public void DrawRoadNetwork()
        {
            if(roadNetworkData == null)
            {
                return;
            }
            if (roadNetworkData.majorRoadPoints.Count == 0 || roadNetworkData.minorRoadPoints.Count == 0)
            {
                return;
            }
            Gizmos.color = Color.green;
            for (int i = 0; i < pointStorage.Count; i++)
            {
                Gizmos.DrawSphere(pointStorage[i], 5f);
            }
            Vector3 lineStart;
            Vector3 lineEnd;

            foreach (LandPlot landPlot in exteriorPlots)
            {
                Gizmos.color = Color.cyan;
                int j = landPlot.lotPoints.Count - 1;
                for (int l = 0; l < landPlot.lotPoints.Count; l++)
                {
                    lineStart = landPlot.lotPoints[j];
                    lineEnd = landPlot.lotPoints[l];
                    Gizmos.DrawLine(lineStart, lineEnd);
                    j = l;
                }
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(landPlot.centroid, new Vector3(Mathf.Cos(landPlot.angleToRoad), 0, Mathf.Sin(landPlot.angleToRoad)));
                Gizmos.DrawSphere(landPlot.centroid, 0.3f);
            }
            
        }

        public void DrawBlockPlots()
        {
            Gizmos.color = Color.cyan;

            Vector3 lineStart;
            Vector3 lineEnd;

            foreach (BlockInfo block in blockInfo)
            {
                if(block.blockLandPlots == null)
                {
                    continue;
                }
                foreach (LandPlot landPlot in block.blockLandPlots)
                {
                    Gizmos.color = Color.cyan;
                    int j = landPlot.lotPoints.Count - 1;
                    for (int l = 0; l < landPlot.lotPoints.Count; l++)
                    {
                        lineStart = landPlot.lotPoints[j];
                        lineEnd = landPlot.lotPoints[l];
                        Gizmos.DrawLine(lineStart, lineEnd);
                        j = l;
                    }
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(landPlot.centroid, new Vector3(Mathf.Cos(landPlot.angleToRoad), 0, Mathf.Sin(landPlot.angleToRoad)));
                    Gizmos.DrawSphere(landPlot.centroid, 0.3f);
                }
            }
        }

        #region Visualization Methods
        public UnityEngine.Terrain GetCurrentTerrain()
        {
#if GAIA_2023
            UnityEngine.Terrain currentTerrain = Gaia.TerrainHelper.GetTerrain(transform.position);
            //Check if the spawner is over a terrain currently

            return currentTerrain;
#else
            return null;
#endif
        }


        public void DrawMaskMapPreview(bool simulate)
        {
            /*
#if GAIA_2023
            SetUpImageMasks();
            if(m_defaultMajorRoadSettings != null && m_defaultMajorRoadSettings.reactToSlope)
            {
                imageMasks[0].m_active = true;
                imageMasks[0].m_slopeMax = 0;
                imageMasks[0].m_slopeMin = m_defaultMajorRoadSettings.baseSlopeCutoffValue / 180;
            }
            else
            {
                imageMasks[0].m_active = false;
            }

            imageMasks[1].m_absoluteHeightMin = waterLevel;
            imageMasks[1].m_absoluteHeightMax = waterLevel + 100;

            List<int> activeRGBAIds = new List<int> { 0, };

            //Set up a multi-terrain operation once, all rules can then draw from the data collected here
            UnityEngine.Terrain currentTerrain = GetCurrentTerrain();
            if (currentTerrain == null)
            {
                //No terrain -> nothing to do
                return;
            }

            GaiaMultiTerrainOperation operation = new GaiaMultiTerrainOperation(currentTerrain, transform, mapSize * 2f);
            operation.GetHeightmap();

            //only re-generate all textures etc. if settings have changed and the preview is dirty, otherwise we can just use the cached textures
            if (m_previewDirty == true)
            {
                //To get a combined preview of different textures on a single mesh we need one color texture each per previewed 
                // rule to determine the color areas on the heightmap mesh
                // We need to iterate over the rules that are previewed, and build those color textures in this process

                //Get additional op data (required for certain image masks)
                operation.GetNormalmap();
                operation.CollectTerrainBakedMasks();
                //Preparing a simple add operation on the image mask shader for the combined heightmap texture
                //Material filterMat = new Material(Shader.Find("Hidden/Gaia/FilterImageMask"));
                //filterMat.SetFloat("_Strength", 1f);
                //filterMat.SetInt("_Invert", 0);

                //Store the currently active render texture here before we start manipulating
                RenderTexture currentRT = RenderTexture.active;

                //Clear texture cache first
                ClearCachedTexture(m_cachedPreviewColorRenderTexture);


                //Initialise our color texture cache at this index with this context
                InitialiseColorTextureCache(operation.RTheightmap);
                //Store result for this rule in our cache render texture array
                Graphics.Blit(ApplyBrush(operation, MultiTerrainOperationType.Heightmap), m_cachedPreviewColorRenderTexture);
                RenderTexture.active = currentRT;

                //Everything processed, check if the preview is not dirty anymore
                m_previewDirty = false;
            }

            //Now draw the preview according to the cached textures
            if (!simulate)
            {
                Material material = GaiaMultiTerrainOperation.GetDefaultGaiaSpawnerPreviewMaterial();
                material.SetInt("_zTestMode", (int)UnityEngine.Rendering.CompareFunction.Always);

                //assign all color textures for the previewed channels in the material
                material.SetTexture("_colorTexture" + 0.ToString(), m_cachedPreviewColorRenderTexture);

                //iterate through spawn rules, and if it is a previewed texture set its color accordingly in the color slot
                int colorIndex = 0;
                material.SetColor("_previewColor" + colorIndex.ToString(), slopePreviewColor);
                colorIndex++;

                for (; colorIndex < GaiaConstants.maxPreviewedTextures; colorIndex++)
                {
                    Color transparentColor = Color.white;
                    transparentColor.a = 0f;
                    material.SetColor("_previewColor" + colorIndex.ToString(), transparentColor);
                }


                Color seaLevelColor = seaColor;
                if (!m_showSeaLevelinStampPreview)
                {
                    seaLevelColor.a = 0f;
                }
                material.SetColor("_seaLevelTintColor", seaLevelColor);
                material.SetFloat("_seaLevel", waterLevel);
                operation.Visualize(MultiTerrainOperationType.Heightmap, operation.RTheightmap, material, 1);

                //Clean up
                operation.CloseOperation();
            }

            //combine all the created color channel textures into the export texture - this texture can then be displayed in preview and exported if required
            //UpdateCachedExportTexture();

            //Clean up temp textures
            GaiaUtils.ReleaseAllTempRenderTextures();
#endif
*/
        }

        /// <summary>
        /// Inizialises or "resets" a color texture in the cache
        /// </summary>
        /// <param name="index">The index at which to initialise.</param>
        /// <param name="rtToInitialiseFrom">A sample render texture with the correct resolution & format settings etc. to initialise from</param>
        private void InitialiseColorTextureCache(RenderTexture rtToInitialiseFrom)
        {
            ClearCachedTexture(m_cachedPreviewColorRenderTexture);
            m_cachedPreviewColorRenderTexture = new RenderTexture(rtToInitialiseFrom);
        }


        private void ClearCachedTexture(RenderTexture cachedRT)
        {
            if (cachedRT != null)
            {
                cachedRT.Release();
                DestroyImmediate(cachedRT);
            }

            cachedRT = new RenderTexture(1, 1, 1);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = cachedRT;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = currentRT;

        }

        private void ReleaseRenderTexture(RenderTexture texture)
        {
            if (texture != null)
            {
                RenderTexture.ReleaseTemporary(texture);
                //texture = null;
            }
        }
#if GAIA_2023
        private RenderTexture ApplyBrush(GaiaMultiTerrainOperation operation, MultiTerrainOperationType opType)
        {
            UnityEngine.Terrain currentTerrain = GetCurrentTerrain();

            RenderTextureDescriptor rtDescriptor;

            switch (opType)
            {
                case MultiTerrainOperationType.Heightmap:
                    rtDescriptor = operation.RTheightmap.descriptor;
                    break;
                case MultiTerrainOperationType.Texture:
                    rtDescriptor = operation.RTtextureSplatmap.descriptor;
                    break;
                case MultiTerrainOperationType.TerrainDetail:
                    rtDescriptor = operation.RTdetailmap.descriptor;
                    break;
                case MultiTerrainOperationType.Tree:
                    rtDescriptor = operation.RTterrainTree.descriptor;
                    break;
                case MultiTerrainOperationType.GameObject:
                    rtDescriptor = operation.RTgameObject.descriptor;
                    break;
                default:
                    rtDescriptor = operation.RTheightmap.descriptor;
                    break;
            }
            //Random write needs to be enabled for certain mask types to function!
            rtDescriptor.enableRandomWrite = true;
            RenderTexture inputTexture = RenderTexture.GetTemporary(rtDescriptor);
            //RenderTexture inputTexture2 = RenderTexture.GetTemporary(rtDescriptor);

            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = inputTexture;
            GL.Clear(true, true, Color.white);
            //RenderTexture.active = inputTexture2;
            //GL.Clear(true, true, Color.white);
            RenderTexture.active = currentRT;

            ImageMask[] maskStack = new ImageMask[0];
            //set up the spawner mask stack, only if it has masks or a biome controller exists with masks
            if (imageMasks.Length > 0)
            {

                maskStack = imageMasks;

                //We start from a white texture, so we need the first mask action in the stack to always be "Multiply", otherwise there will be no result.
                maskStack[0].m_blendMode = ImageMaskBlendMode.Multiply;

                //Iterate through all image masks and set up the current paint context in case the shader uses heightmap data
                foreach (ImageMask mask in maskStack)
                {
                    mask.m_multiTerrainOperation = operation;
                    mask.m_seaLevel = waterLevel;
                    mask.m_maxWorldHeight = waterLevel + 100;
                    mask.m_minWorldHeight = 0;
                }

            }

            //Get the combined masks from the spawner
            RenderTexture outputTexture = RenderTexture.GetTemporary(rtDescriptor);
            Graphics.Blit(ImageProcessing.ApplyMaskStack(inputTexture, outputTexture, maskStack, ImageMaskInfluence.Local), outputTexture);

            //clean up temporary textures
            ReleaseRenderTexture(inputTexture);
            inputTexture = null;

            return outputTexture;
        }
#endif
        private void SetUpImageMasks()
        {
#if GAIA_2023
            if (imageMasks.Length == 2)
            {
                return;
            }
            imageMasks = new ImageMask[2];
            ImageMask slopeMask = new ImageMask();
            slopeMask.m_active = true;
            slopeMask.m_strength = 1;
            slopeMask.m_xScale = 1;
            slopeMask.m_zScale = 1;
            slopeMask.m_strengthTransformCurve = AnimationCurve.Linear(0, 0, 1, 1);
            slopeMask.m_slopeMaskCurve = AnimationCurve.Linear(0, 0, 1, 1);
            slopeMask.m_operation = ImageMaskOperation.SlopeMask;

            ImageMask heightMask = new ImageMask();
            heightMask.m_active = true;
            heightMask.m_strength = 1;
            heightMask.m_xScale = 1;
            heightMask.m_zScale = 1;
            heightMask.HeightMaskUnit = HeightMaskUnit.Meter;
            heightMask.m_heightMaskType = HeightMaskType.WorldSpace;
            heightMask.m_strengthTransformCurve = AnimationCurve.Linear(0, 0, 1, 1);
            heightMask.m_heightMaskCurve = AnimationCurve.Linear(0.0001f, 1, 1, 1);
            Keyframe key = new Keyframe();
            key.value = 0;
            key.time = 0;
            heightMask.m_heightMaskCurve.AddKey(key);

            heightMask.m_operation = ImageMaskOperation.HeightMask;

            imageMasks[0] = slopeMask;
            imageMasks[1] = heightMask;
#endif
        }
        #endregion

#if EASYROADS3D_PRO
        #region EasyRoads RoadGen
        public GameObject CreateNetwork()
        {
            Dictionary<Vector3, RoadPointInformation> allRoadPoints = new Dictionary<Vector3, RoadPointInformation>(roadNetworkData.majorRoadPoints);
            foreach (KeyValuePair<Vector3, RoadPointInformation> kvp in roadNetworkData.minorRoadPoints)
            {
                allRoadPoints.Add(kvp.Key, kvp.Value);
            }

            Dictionary<RoadPointInformation, RoadPointRefs> roadPointRefDictionary = new Dictionary<RoadPointInformation, RoadPointRefs>();
            ERRoadNetwork roadNetwork = new ERRoadNetwork();
            //Currently dont know how to load side objects
            SideObject[] sideObjects = roadNetwork.GetSideObjects();
            TConnectionMajor = ERConnection.Create(TConnectionObjMajor);
            XConnectionMajor = ERConnection.Create(XConnectionObjMajor);
            LConnectionMajor = ERConnection.Create(LConnectionObjMajor);
            TConnectionMinor = ERConnection.Create(TConnectionObjMinor);
            XConnectionMinor = ERConnection.Create(XConnectionObjMinor);
            LConnectionMinor = ERConnection.Create(LConnectionObjMinor);
            List<RoadPointInformation> orderedRoadPointTree = new List<RoadPointInformation>();
            foreach (KeyValuePair<Vector3, RoadPointInformation> kvp in allRoadPoints)
            {
                //this is a bit of a patch because our graph isnt in a usuable order already
                GetOrderedRoadPointTree(kvp.Value, ref orderedRoadPointTree, allRoadPoints);
                break;
            }

            foreach (RoadPointInformation roadPointInfo in orderedRoadPointTree)
            {
                CreatePointRoads(roadNetwork, roadPointInfo, roadPointRefDictionary, allRoadPoints);
            }


            roadNetwork.BuildRoadNetwork();

            return FindAnyObjectByType<ERModularBase>().gameObject;
            
        }

        public void GetOrderedRoadPointTree(RoadPointInformation sourceNode, ref List<RoadPointInformation> orderedRoadPointTree, Dictionary<Vector3, RoadPointInformation> allRoadPoints)
        {
            orderedRoadPointTree.Add(sourceNode);

            foreach (RoadConnection currentConnection in sourceNode.roadConnections)
            {
                RoadPointInformation currentRoadPoint = allRoadPoints[currentConnection.connectionLocation];
                if (!orderedRoadPointTree.Contains(currentRoadPoint))
                {
                    GetOrderedRoadPointTree(currentRoadPoint, ref orderedRoadPointTree, allRoadPoints);
                }
            }
        }

        public void CreatePointRoads(ERRoadNetwork roadNetwork, RoadPointInformation sourceRoadPoint, Dictionary<RoadPointInformation, RoadPointRefs> roadPointRefDictionary, Dictionary<Vector3, RoadPointInformation> allRoadPoints)
        {
            List<(ERRoad, RoadPointInformation)> attachedRoads = GetAttachedRoads(sourceRoadPoint, roadPointRefDictionary, allRoadPoints);

            ERConnection startConnection = null;
            if (attachedRoads.Count == 0)
            {
                //we are on the start node or an island
                if (sourceRoadPoint.roadConnections.Count > 2)
                {
                    foreach (RoadConnection roadConnection in sourceRoadPoint.roadConnections)
                    {
                        RoadPointInformation currentRoadPointInfo = allRoadPoints[roadConnection.connectionLocation];
                        if (startConnection == null)
                        {
                            (ERRoad newRoad, ERConnection newConnection) = CreateRoadAndConnection(roadNetwork, sourceRoadPoint.pointLocation, currentRoadPointInfo.pointLocation, sourceRoadPoint.roadType, sourceRoadPoint.roadConnections.Count);
                            startConnection = newConnection;
                            attachedRoads.Add((newRoad, currentRoadPointInfo));
                        }
                        else
                        {
                            ERRoad newRoad = CreateRoad(roadNetwork, sourceRoadPoint.pointLocation, currentRoadPointInfo.pointLocation, sourceRoadPoint.roadType, startConnection);
                            attachedRoads.Add((newRoad, currentRoadPointInfo));
                        }

                    }
                }
                else
                {
                    //this is being caused because the allroadpoints is not ordered correctly, intersections come after all major road points
                    Debug.Log("Badness");
                }


            }
            else
            {
                if (sourceRoadPoint.roadConnections.Count == 1)
                {
                    //we are on a dead end so do nothing
                }
                else if (sourceRoadPoint.roadConnections.Count == 2)
                {
                    List<RoadPointInformation> unconnectedPoints = GetUnconnectedPoints(sourceRoadPoint, attachedRoads, allRoadPoints);
                    //we are on a no intersection section


                    if (attachedRoads.Count == 1)
                    {

                        float connectionAngle1 = CityBuilderUtils.CalcuateAngleBetweenPoints(sourceRoadPoint.roadConnections[0].connectionLocation, sourceRoadPoint.pointLocation);
                        float connectionAngle2 = CityBuilderUtils.CalcuateAngleBetweenPoints(sourceRoadPoint.pointLocation, sourceRoadPoint.roadConnections[1].connectionLocation);


                        float angleChange = connectionAngle1 - connectionAngle2;
                        angleChange = CityBuilderUtils.ClampToRad(angleChange);
                        angleChange = angleChange * Mathf.Rad2Deg;
                        if ((angleChange >= 80 && angleChange <= 100) || (angleChange >= 260 && angleChange <= 280))
                        {
                            foreach (RoadConnection roadConnection in sourceRoadPoint.roadConnections)
                            {
                                if (roadConnection.connectionLocation == unconnectedPoints[0].pointLocation)
                                {
                                    (ERRoad newRoad, ERConnection newConnection) = CreateRoadAndConnection(roadNetwork, sourceRoadPoint.pointLocation, unconnectedPoints[0].pointLocation, sourceRoadPoint.roadType, sourceRoadPoint.roadConnections.Count);
                                    startConnection = newConnection;
                                    attachedRoads.Add((newRoad, unconnectedPoints[0]));
                                    attachedRoads[0].Item1.ConnectToEndEx(newConnection, newConnection.FindNearestConnectionIndex(CityBuilderUtils.GetHeightVector3AtPoint(attachedRoads[0].Item2.pointLocation)), true);
                                }
                            }
                        }
                        else
                        {
                            attachedRoads[0].Item1.AddMarker(CityBuilderUtils.GetHeightVector3AtPoint(unconnectedPoints[0].pointLocation));
                            attachedRoads.Add((attachedRoads[0].Item1, unconnectedPoints[0]));
                        }

                    }
                    else if (attachedRoads.Count == 2)
                    {
                        (ERRoad road1, RoadPointInformation roadPoint1) = attachedRoads[0];

                        (ERRoad road2, RoadPointInformation roadPoint2) = attachedRoads[1];

                        ERConnection newConnection = null;
                        if (sourceRoadPoint.roadType == RoadType.Major)
                        {
                            newConnection = road1.AttachToEnd(LConnectionMajor);
                        }
                        else
                        {
                            newConnection = road1.AttachToEnd(LConnectionMinor);
                        }

                        if (newConnection != null)
                        {
                            newConnection.SetPosition(CityBuilderUtils.GetHeightVector3AtPoint(sourceRoadPoint.pointLocation));
                        }

                        road2.ConnectToEndEx(newConnection, newConnection.FindNearestConnectionIndex(CityBuilderUtils.GetHeightVector3AtPoint(roadPoint2.pointLocation)), true);

                    }
                }
                else
                {
                    List<RoadPointInformation> unconnectedPoints = GetUnconnectedPoints(sourceRoadPoint, attachedRoads, allRoadPoints);
                    List<(ERRoad, RoadPointInformation)> originalAttachedRoads = new List<(ERRoad, RoadPointInformation)>(attachedRoads);

                    int roadConnections = sourceRoadPoint.roadConnections.Count;
                    RoadPointInformation connectionRoadPoint = null;
                    //get connection pos at appropriate angle
                    if (roadConnections == 3)
                    {
                        Debug.DrawRay(sourceRoadPoint.pointLocation, Vector3.up, Color.red, 40);
                        (RoadPointInformation connectionRoadPointInfo, (ERRoad connectionRoad, ERConnection connection)) = CreateTConnection(sourceRoadPoint, unconnectedPoints, originalAttachedRoads, allRoadPoints, roadNetwork);
                        attachedRoads.Add((connectionRoad, connectionRoadPointInfo));
                        startConnection = connection;
                        connectionRoadPoint = connectionRoadPointInfo;
                    }


                    //go through remaining desired connections and create new roads for them
                    foreach (RoadPointInformation currentRoadPointInfo in unconnectedPoints)
                    {
                        if (connectionRoadPoint != null && connectionRoadPoint == currentRoadPointInfo)
                        {

                        }
                        else if (startConnection == null)
                        {
                            (ERRoad newRoad, ERConnection newConnection) = CreateRoadAndConnection(roadNetwork, sourceRoadPoint.pointLocation, currentRoadPointInfo.pointLocation, sourceRoadPoint.roadType, sourceRoadPoint.roadConnections.Count);
                            startConnection = newConnection;
                            attachedRoads.Add((newRoad, currentRoadPointInfo));
                        }
                        else
                        {
                            ERRoad newRoad = CreateRoad(roadNetwork, sourceRoadPoint.pointLocation, currentRoadPointInfo.pointLocation, sourceRoadPoint.roadType, startConnection);
                            attachedRoads.Add((newRoad, currentRoadPointInfo));
                        }
                    }

                    //go through connecting roads and connect them to the intersection
                    foreach ((ERRoad road, RoadPointInformation roadPoint) in originalAttachedRoads)
                    {
                        if (connectionRoadPoint != null && connectionRoadPoint == roadPoint)
                        {

                        }
                        else if (startConnection == null)
                        {
                            Debug.Log("Was Forced To Create Intersection From Exsiting Road");
                            if (roadConnections == 4)
                            {
                                startConnection = road.AttachToEnd(XConnectionMajor);
                            }
                            else if (roadConnections == 3)
                            {
                                startConnection = road.AttachToEnd(TConnectionMajor);
                            }

                            if (startConnection != null)
                            {
                                startConnection.SetPosition(sourceRoadPoint.pointLocation);
                            }
                        }
                        else
                        {
                            road.ConnectToEndEx(startConnection, startConnection.FindNearestConnectionIndex(CityBuilderUtils.GetHeightVector3AtPoint(roadPoint.pointLocation)), true);
                        }
                    }
                }
            }

            RoadPointRefs roadPointRefs = new RoadPointRefs(attachedRoads, startConnection);

            roadPointRefDictionary.Add(sourceRoadPoint, roadPointRefs);
        }

        public (RoadPointInformation, (ERRoad, ERConnection)) CreateTConnection(RoadPointInformation sourceRoadPoint, List<RoadPointInformation> unconnectedPoints, List<(ERRoad, RoadPointInformation)> attachedRoads, Dictionary<Vector3, RoadPointInformation> allRoadPoints, ERRoadNetwork roadNetwork)
        {
            RoadConnection correctSpawnConnection = null;
            List<RoadConnection> roadConnections = sourceRoadPoint.roadConnections;
            for (int i = 0; i < roadConnections.Count; i++)
            {
                int j = (i + 1) % roadConnections.Count; // Next index, wrapping around
                int k = (i + 2) % roadConnections.Count; // Index two ahead, wrapping around

                float angleDif1 = CityBuilderUtils.ClampToRad(roadConnections[i].connectionAngle - roadConnections[j].connectionAngle) * Mathf.Rad2Deg;
                float angleDif2 = CityBuilderUtils.ClampToRad(roadConnections[i].connectionAngle - roadConnections[k].connectionAngle) * Mathf.Rad2Deg;

                if ((angleDif1 > 179 && angleDif1 < 181) || (angleDif2 > 179 && angleDif2 < 181))
                {
                    continue;
                }
                else
                {
                    correctSpawnConnection = roadConnections[i];
                    break;
                }
            }

            RoadPointInformation correctSpawnRoadPoint = allRoadPoints[correctSpawnConnection.connectionLocation];
            ERConnection connection = null;
            float rotation = (Mathf.PI + correctSpawnConnection.connectionAngle) * Mathf.Rad2Deg;
            if (unconnectedPoints.Contains(correctSpawnRoadPoint))
            {
                ERRoad newRoad = CreateRoad(roadNetwork, sourceRoadPoint.pointLocation, correctSpawnRoadPoint.pointLocation, sourceRoadPoint.roadType);

                if (sourceRoadPoint.roadType == RoadType.Major)
                {
                    connection = newRoad.AttachToStart(TConnectionMajor, 2);
                }
                else
                {
                    connection = newRoad.AttachToStart(TConnectionMinor, 2);
                }
                if (connection != null)
                {
                    connection.SetPosition(CityBuilderUtils.GetHeightVector3AtPoint(sourceRoadPoint.pointLocation));
                    //connection.SetRotation(new Vector3(0, rotation, 0));
                }
                return (correctSpawnRoadPoint, (newRoad, connection));
            }
            else
            {
                foreach ((ERRoad road, RoadPointInformation roadPoint) in attachedRoads)
                {
                    if (roadPoint == correctSpawnRoadPoint)
                    {
                        if (sourceRoadPoint.roadType == RoadType.Major)
                        {
                            connection = road.AttachToEnd(TConnectionMajor, 2);
                        }
                        else
                        {
                            connection = road.AttachToEnd(TConnectionMinor, 2);
                        }


                        if (connection != null)
                        {
                            connection.SetPosition(CityBuilderUtils.GetHeightVector3AtPoint(sourceRoadPoint.pointLocation));
                            //connection.SetRotation(new Vector3(0, rotation, 0));
                        }
                        return (correctSpawnRoadPoint, (road, connection));
                    }

                }
            }
            Debug.LogError("We are returning null");
            return (correctSpawnRoadPoint, (null, connection));
        }

        public List<RoadPointInformation> GetUnconnectedPoints(RoadPointInformation sourceRoadPointInfo, List<(ERRoad, RoadPointInformation)> attachedRoads, Dictionary<Vector3, RoadPointInformation> allRoadPoints)
        {
            List<RoadPointInformation> unconnectedPoints = new List<RoadPointInformation>();
            foreach (RoadConnection roadConnection in sourceRoadPointInfo.roadConnections)
            {
                RoadPointInformation currentRoadPointInformation = allRoadPoints[roadConnection.connectionLocation];
                if (!IsAttached(currentRoadPointInformation, attachedRoads))
                {
                    unconnectedPoints.Add(currentRoadPointInformation);
                }
            }
            return unconnectedPoints;
        }

        public bool IsAttached(RoadPointInformation testRoadPointInfo, List<(ERRoad, RoadPointInformation)> attachedRoads)
        {
            foreach ((ERRoad road, RoadPointInformation roadPointInfo) in attachedRoads)
            {
                if (testRoadPointInfo == roadPointInfo)
                {
                    return true;
                }
            }
            return false;
        }

        public (ERRoad, ERConnection) CreateRoadAndConnection(ERRoadNetwork roadNetwork, Vector3 start, Vector3 end, RoadType roadType, int connectionCount)
        {
            Vector3 heightModdedStart = CityBuilderUtils.GetHeightVector3AtPoint(start);
            Debug.DrawRay(heightModdedStart, Vector3.up, Color.red, 100);
            Vector3[] roadPoints = new Vector3[2] { heightModdedStart, CityBuilderUtils.GetHeightVector3AtPoint(end) };
            ERRoad newRoad = roadNetwork.CreateRoad("Test", roadPoints);


            ERConnection newConnection = null;
            if (connectionCount == 4)
            {
                if (roadType == RoadType.Major)
                {
                    newConnection = newRoad.AttachToStart(XConnectionMajor);
                }
                else
                {
                    newConnection = newRoad.AttachToStart(XConnectionMinor);
                }

            }
            else if (connectionCount == 3)
            {
                if (roadType == RoadType.Major)
                {
                    newConnection = newRoad.AttachToStart(TConnectionMajor, 1);
                }
                else
                {
                    newConnection = newRoad.AttachToStart(TConnectionMinor, 1);
                }

            }
            else if (connectionCount == 2)
            {
                if (roadType == RoadType.Major)
                {
                    newConnection = newRoad.AttachToStart(LConnectionMajor);
                }
                else
                {
                    newConnection = newRoad.AttachToStart(LConnectionMinor);
                }

            }
            if (newConnection != null)
            {
                newConnection.SetPosition(heightModdedStart);
            }

            AddSideObjectsToRoad(newRoad, roadType, roadNetwork);
            return (newRoad, newConnection);
        }

        public ERRoad CreateRoad(ERRoadNetwork roadNetwork, Vector3 start, Vector3 end, RoadType roadType, ERConnection connectionAtStart = null)
        {
            Vector3 heightAdjustedEnd = CityBuilderUtils.GetHeightVector3AtPoint(end);
            Vector3[] roadPoints = new Vector3[2] { CityBuilderUtils.GetHeightVector3AtPoint(start), heightAdjustedEnd };
            ERRoad newRoad = roadNetwork.CreateRoad("Test", roadPoints);
            //newRoad.FollowTerrainContours(true);
            //newRoad.FollowTerrainContourThreshold(1, true);
            if (connectionAtStart != null)
            {
                newRoad.ConnectToStartExt(connectionAtStart, connectionAtStart.FindNearestConnectionIndex(heightAdjustedEnd), true);
            }
            AddSideObjectsToRoad(newRoad, roadType, roadNetwork);
            return newRoad;
        }

        public void AddSideObjectsToRoad(ERRoad road, RoadType roadType, ERRoadNetwork roadNetwork)
        {
            List<string> sideObjectList;
            if (roadType == RoadType.Major)
            {
                sideObjectList = majorRoadSideObjects;
            }
            else
            {
                sideObjectList = minorRoadSideObjects;
            }

            foreach (string sideObjectName in sideObjectList)
            {
                SideObject sideObject = roadNetwork.GetSideObjectByName(sideObjectName);
                if (sideObject != null)
                {
                    road.SideObjectSetActive(sideObject, true);
                }
            }
        }

        public List<(ERRoad, RoadPointInformation)> GetAttachedRoads(RoadPointInformation currentRoadPointInfo, Dictionary<RoadPointInformation, RoadPointRefs> roadPointRefDictionary, Dictionary<Vector3, RoadPointInformation> allRoadPoints)
        {
            List<(ERRoad, RoadPointInformation)> resultList = new List<(ERRoad, RoadPointInformation)>();
            foreach (RoadConnection roadConnection in currentRoadPointInfo.roadConnections)
            {
                RoadPointInformation currentConnectionRoadPointInfo = allRoadPoints[roadConnection.connectionLocation];
                if (roadPointRefDictionary.ContainsKey(currentConnectionRoadPointInfo))
                {
                    RoadPointRefs currentConnectionRoadPointRefs = roadPointRefDictionary[currentConnectionRoadPointInfo];
                    foreach ((ERRoad road, RoadPointInformation roadPointInfo) in currentConnectionRoadPointRefs.attachedRoads)
                    {
                        if (roadPointInfo == currentRoadPointInfo)
                        {
                            resultList.Add((road, currentConnectionRoadPointInfo));
                        }
                    }
                }
            }

            return resultList;
        }

        public class RoadPointRefs
        {
            public List<(ERRoad, RoadPointInformation)> attachedRoads;
            public ERConnection attachedConnection;

            public RoadPointRefs(List<(ERRoad, RoadPointInformation)> attachedRoads)
            {
                this.attachedRoads = attachedRoads;
                this.attachedConnection = null;
            }

            public RoadPointRefs(List<(ERRoad, RoadPointInformation)> attachedRoads, ERConnection attachedConnection)
            {
                this.attachedRoads = attachedRoads;
                this.attachedConnection = attachedConnection;
            }
        }
        #endregion
#endif
    }
}

public enum CityRoadSpawnType
{
    None,
    Gena,
#if EASYROADS3D_PRO
    EasyRoads,
#endif
}