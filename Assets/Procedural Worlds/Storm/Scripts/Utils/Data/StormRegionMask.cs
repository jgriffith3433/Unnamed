using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralWorlds.Storm.Utils
{
    /// <summary>
    /// A region mask - a mask that can be painted in a region using a brush
    /// </summary>
    [ExecuteInEditMode]
    public class StormRegionMask : MonoBehaviour
    {
        #region Constants
        public const float MaxDrawDistance = 1000f;
        public const bool HideChildVisualizer = true;
        #endregion

        #region Private variables
        
        // The mask visualizer
        [SerializeField]
        private StormRegionMaskVisualizer m_visualizer;

        // The override region ID
        [SerializeField]
        [StormLayer]
        private string m_overrideID = null;

        /// The regions this mask belongs to
        [SerializeField]
        private StormRegion[] m_regions = Array.Empty<StormRegion>();
        //List of Regions we have published our ID to ... maintained so if we remove a region from the above list, it can be un-published
        private List<StormRegion> m_publishedRegions = new List<StormRegion>();

        // The tile data
        private StormTileData m_stormTileData = null;
        private StormTileRegion m_stormTileRegion = null;

        // Whether the tile data is dirty
        private bool m_tileDataIsDirty = true;
        private bool m_tileRegionIsDirty = true;

        private bool m_tileDataIsAdded = false;
        private bool m_tileRegionIsAdded = false;


        // Tracking for movement and scaling 

        // Whether the transform has moved
        private bool m_wasMoved = false;

        // The position of the visualization
        private Vector3 m_visPosition;

        // The rotation of the visualization
        private Quaternion m_visRotation;

        // The scale of the visualization
        private Vector3 m_visScale = Vector3.one;

        // The local to world matrix for the last update
        private Matrix4x4 m_lastL2W; 

        #endregion

        #region Public properties

        /// <summary>
        /// Whether to always show the visualization
        /// </summary>
        [SerializeField]
        private bool m_shadeVisualization = false;

        /// <summary>
        /// Whether the visualization is always shown
        /// </summary>
        public bool ShadeVisualization
        {
            get { return m_shadeVisualization; }
            set { 
                m_shadeVisualization = value;
                UpdateVisualization();
            }
        }

        /// <summary>
        /// The layers to use when finding the ground
        /// </summary>
        [SerializeField]
        private LayerMask m_groundLayers = ~0;
        public LayerMask GroundLayers
        {
            get { return m_groundLayers; }
            set { 
                m_groundLayers = value;
                UpdateVisualization();
            }
        }

        /// <summary>
        /// The points that make the region mask up
        /// </summary>
        [SerializeField]
        private List<StormRegionMaskNode> m_controlPoints = new List<StormRegionMaskNode>();
        public List<StormRegionMaskNode> ControlPoints
        {
            get { return m_controlPoints; }
            private set { 
                m_controlPoints = value;
                UpdateVisualization();
                UpdateStormData();
            }
        }

        /// <summary>
        /// The ID of the region this mask belongs to
        /// </summary>
        [StormLayer]
        [SerializeField]
        private string m_regionID;
        public string RegionID
        {
            get {
                if (OverrideRegionID)
                {
                    if (m_overrideID == null)
                        m_overrideID = name;
                    return m_overrideID;
                }
                if (m_regionID == null)
                    m_regionID = Guid.NewGuid().ToString();
                return m_regionID;
            }
            set {
                if (OverrideRegionID)
                {
                    if (m_overrideID != value)
                    {
                        m_overrideID = value;
                        foreach (StormRegion r in m_regions)
                            if (r != null)
                                StormAPI.UpdateRegion(r);
                    }
                }
                else
                {
                    if (m_regionID != value)
                    {
                        m_regionID = value;
                        foreach (StormRegion r in m_regions)
                            if (r != null)
                                StormAPI.UpdateRegion(r);
                    }
                }
            }
        }

        /// <summary>
        /// Whether to override the region ID
        /// </summary>
        public bool OverrideRegionID = false;

        /// <summary>
        /// The falloff of the region in world units
        /// </summary>
        [SerializeField]
        private float m_falloff = 0;
        public float RegionFalloff 
        { 
            get { return m_falloff; } 
            set {
                m_tileDataIsDirty = true;
                m_falloff = value; 
                UpdateVisualization();
                UpdateStormData();
            }
        }

        /// <summary>
        /// The strength of the region
        /// </summary>
        [SerializeField]
        private float m_strength = 1;
        public float RegionStrength
        { 
            get { return m_strength; } 
            set {
                m_tileDataIsDirty = true;
                m_strength = value; 
                UpdateVisualization();
                UpdateStormData();
            }
        }

        /// <summary>
        /// The radius of the brush in world units
        /// </summary>
        [SerializeField]
        private float m_brushRadius = 10f;
        public float BrushRadius
        { 
            get { return m_brushRadius; } 
            set { 
                m_brushRadius = value; 
            }
        }

        /// <summary>
        /// The color of the mask
        /// </summary>
        [SerializeField]
        private Color m_maskColor = new Color(1,0,0,0.25f);
        public Color MaskColor
        {
            get { return m_maskColor; }
            set { 
                m_maskColor = value; 
                UpdateVisualization();
            }
        }

        /// <summary>
        /// The selected node index
        /// </summary>
        [SerializeField]
        private int m_selectedNodeIndex = 0;
        public int SelectedNodeIndex
        {
            get { return m_selectedNodeIndex; }
            set { 
                m_selectedNodeIndex = value; 
            }
        }

        /// <summary>
        /// The maximum node ID
        /// </summary>
        [SerializeField]
        private int m_maxNodeID = int.MinValue;
        public int MaxNodeID
        {
            get { return m_maxNodeID; }
            set { 
                m_maxNodeID = value; 
            }
        }

        #endregion

        #region Events

        public delegate void VisualizationUpdated(StormRegionMask regionMask);
        public event VisualizationUpdated OnVisualizationUpdated;
        public delegate void Destroyed(StormRegionMask regionMask);
        public event Destroyed OnDestroyed;

        #endregion

        #region Unity events

        /// <summary>
        /// When the opject is enabled
        /// </summary>
        private void OnEnable()
        {
            // Transform state - used to detect position, rotation and scale changes
            m_lastL2W = transform.localToWorldMatrix;
            m_visPosition = transform.position;
            m_visRotation = transform.rotation;
            m_visScale = transform.lossyScale;

            SetDirty();
            if (TileData.HasData())
            {
                m_tileDataIsAdded = true;
                StormAPI.AddStormTile(TileData);
            }
            if (TileRegion.Regions.Length > 0)
            {
                m_tileRegionIsAdded = true;
                StormAPI.AddStormTile(TileRegion);
            }
            UpdateRegions();
            StormAPI.OnStormInstanceChanged -= OnStormInstanceChanged;
            StormAPI.OnStormInstanceChanged += OnStormInstanceChanged;
        }

        /// <summary>
        /// When the object is disabled
        /// </summary>
        private void OnDisable()
        {
            RemoveRegions();
            StormAPI.OnStormInstanceChanged -= OnStormInstanceChanged;
            m_tileDataIsAdded = false;
            m_tileRegionIsAdded = false;
            StormAPI.RemoveStormTile(m_stormTileData);
            StormAPI.RemoveStormTile(m_stormTileRegion);
        }

        /// <summary>
        /// Hande the update event
        /// </summary>
        //private void Update()
        //{
            //HandleTransformChange();
        //}

        /// <summary>
        /// Clean up data on destroy
        /// </summary>
        private void OnDestroy()
        {
            RemoveRegions();
            m_tileDataIsAdded = false;
            m_tileRegionIsAdded = false;
            if (m_stormTileData != null)
            {
                StormAPI.RemoveStormTile(m_stormTileData);
                m_stormTileData.Dispose();
                m_stormTileData = null;
            }
            if (m_stormTileRegion != null)
            {
                StormAPI.RemoveStormTile(m_stormTileRegion);
                m_stormTileRegion = null;
            }
            if (OnDestroyed != null)
            {
                OnDestroyed(this);
            }
        }
        
        #endregion

        #region Main operations

        /// <summary>
        /// Handle transform level updates
        /// </summary>
        public void ProcessTransformUpdate()
        {
            //Ignore if we moved the transform ourselves
            if (m_wasMoved)
            {
                SetDirty();
                UpdateStormData();
                transform.hasChanged = false;
                m_visPosition = transform.position;
                m_visRotation = transform.rotation;
                m_visScale = transform.localScale;
                m_wasMoved = false;
                return;
            }

            //Check for changes
            //Position change
            Vector3 deltaPosition = transform.position - m_visPosition;
            if (deltaPosition != Vector3.zero)
            {
                //Put the transform back on the ground collider
                Ray ray = new Ray(transform.position + Vector3.up * 500f, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1000f, m_groundLayers))
                {
                    transform.position = hit.point;
                }

                //Now move the control points
                deltaPosition = transform.position - m_visPosition;
                foreach (StormRegionMaskNode node in ControlPoints)
                {
                    //Stick them to the ground
                    node.Position += deltaPosition;
                    ray = new Ray(node.Position + Vector3.up * 500f, Vector3.down);
                    if (Physics.Raycast(ray, out hit, 1000f, m_groundLayers))
                    {
                        node.Position = hit.point;
                    }
                }
                SetDirty();
                UpdateVisualization();
                UpdateStormData();
                transform.hasChanged = false;
                m_visPosition = transform.position;
                return;
            }

            //Rotation change
            Quaternion deltaRotation = transform.rotation * Quaternion.Inverse(m_visRotation);
            if (deltaRotation != Quaternion.identity)
            {
                // Rotate all the control points around the transform position by the delta rotation
                foreach (StormRegionMaskNode node in ControlPoints)
                {
                    Vector3 delta = node.Position - transform.position;
                    node.Position = transform.position + deltaRotation * delta;
                    
                    //Stick it to the ground
                    Ray ray = new Ray(node.Position + Vector3.up * 500f, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 1000f, m_groundLayers))
                    {
                        node.Position = hit.point;
                    }
                }
                UpdateVisualization();
                UpdateStormData();
                transform.hasChanged = false;
                m_visRotation = transform.rotation;
                return;
            }

            //Scale change
            Vector3 deltaScale = transform.localScale - m_visScale;
            if (deltaScale != Vector3.zero)
            {
                // Scale all control points relative to the transform position
                foreach (StormRegionMaskNode node in ControlPoints)
                {
                    // Get the vector from transform to node
                    Vector3 delta = node.Position - transform.position;

                    // Scale the delta vector by the relative scale change
                    Vector3 scaledDelta = new Vector3(
                        delta.x * (transform.localScale.x / m_visScale.x),
                        delta.y * (transform.localScale.y / m_visScale.y),
                        delta.z * (transform.localScale.z / m_visScale.z)
                    );

                    // Set the new position
                    node.Position = transform.position + scaledDelta;

                    //Stick it to the ground
                    Ray ray = new Ray(node.Position + Vector3.up * 500f, Vector3.down);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 1000f, m_groundLayers))
                    {
                        node.Position = hit.point;
                    }
                }
                UpdateVisualization();
                UpdateStormData();
                transform.hasChanged = false;
                m_visScale = transform.localScale;
                return;
            }   

            transform.hasChanged = false;
        }

        /// <summary>
        /// Process transform updates - keeping here for now
        /// </summary>
        public void HandleTransformChange()
        {
            //Check for a transform change
            Matrix4x4 current = transform.localToWorldMatrix;
            if (current == m_lastL2W)
            {
                return; // nothing changed
            }

            //Transform has changed, so we need to update the control points
            RaycastHit hit;
            Matrix4x4 delta = current * m_lastL2W.inverse;
            for (int i = 0; i < m_controlPoints.Count; ++i)
            {
                Vector3 deltaPos = delta.MultiplyPoint3x4(m_controlPoints[i].Position);

                //Relocate it onto the ground
                Ray ray = new Ray(deltaPos + Vector3.up * 500f, Vector3.down);
                if (Physics.Raycast(ray, out hit, 1000f, m_groundLayers))
                {
                    deltaPos = hit.point;
                }

                //Save the update
                m_controlPoints[i].Position = deltaPos;
            }
            SetDirty();

            m_lastL2W = current;
            UpdateVisualization();
            UpdateStormData();
        }

        /// <summary>
        /// Add a new paint node to the mask
        /// </summary>
        /// <param name="position"></param>
        /// <param name="isPainting"></param>
        /// <returns>True if point was added. False otherwise</returns>
        public bool AddPoint(Vector3 position, bool isPainting = false)
        {
            //Check that it is not too close to the nearest nodes when in paint mode
            if (isPainting && TooClose(position))
            {
                return false;
            }   

            //Move the game object here if it is empty and signal that we moved it
            if (ControlPoints.Count == 0)
            {
                transform.position = position;
                m_wasMoved = true;
            }

            //Create and add the new node
            StormRegionMaskNode node = new StormRegionMaskNode();
            node.ID = ++MaxNodeID;
            node.Position = position;

            if (SelectedNodeIndex >= ControlPoints.Count - 1)
            {
                ControlPoints.Add(node);
                SelectedNodeIndex = ControlPoints.Count-1;
            }
            else
            {
                SelectedNodeIndex++;
                ControlPoints.Insert(SelectedNodeIndex, node);
            }

            //Paint it
            m_tileDataIsDirty = true;
            UpdateVisualization();
            UpdateStormData();
            return true;
        }

        /// <summary>
        /// Remove paint nodes within the brush radius of the point
        /// </summary>
        /// <param name="point"></param>
        /// <returns>True if point was removed. False otherwise</returns>
        public bool RemovePoint(Vector3 point)
        {
            int minIndexRemoved = int.MaxValue;
            for (int i = 0; i < ControlPoints.Count;)
            {
                StormRegionMaskNode node = ControlPoints[i];
                if (Vector3.Distance(node.Position, point) < BrushRadius)
                {
                    ControlPoints.RemoveAt(i);
                    if (i < minIndexRemoved)
                    {
                        minIndexRemoved = i;
                    }
                }
                else
                {
                    i++;
                }
            }

            //Exit if nothing was removed
            if (minIndexRemoved == int.MaxValue)
            {
                return false;
            }

            //Reset the selected node to be one before the minimum index removed
            SelectedNodeIndex = minIndexRemoved - 1;
            if (SelectedNodeIndex < 0)
            {
                SelectedNodeIndex = 0;
            }

            //Paint it
            m_tileDataIsDirty = true;
            UpdateVisualization();
            UpdateStormData();
            return true;
        }

        /// <summary>
        /// Select a node by its index
        /// </summary>
        /// <param name="index"></param>
        public void SelectNodeByIndex(int index)
        {
            if (index < 0 || index >= ControlPoints.Count)
            {
                //Reset the selected node index 
                SelectedNodeIndex = 0;
                //Signal error
                Debug.LogWarning("Node index out of bounds:" + index);
                return;
            }
            //Select the new node index
            SelectedNodeIndex = index;
        }

        /// <summary>
        /// Select a node by its ID
        /// </summary>
        /// <param name="nodeID">Node ID to select</param>
        public void SelectNodeByID(int nodeID)
        {
            //Fallback position to the first node
            SelectedNodeIndex = 0;

            //If there are no nodes then return
            if (ControlPoints.Count == 0)
            {
                return;
            }

            //Find and select the new node by its ID
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (ControlPoints[i].ID == nodeID)
                {
                    SelectedNodeIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Select a node by the node itself
        /// </summary>
        /// <param name="node">Node to select</param>
        public void SelectNode(StormRegionMaskNode node)
        {
            SelectNodeByID(node.ID);
        }

        /// <summary>
        /// Check that the point is not too close to any other point. Based on radius of brush.
        /// Influences the density of nodes when painting. Ignored at other times.
        /// </summary>
        /// <param name="position">position to check</param>
        /// <param name="radius">radius to check</param>
        /// <returns></returns>
        private bool TooClose(Vector3 position, bool checkAll = false)
        {
            //If no nodes, then we are good
            if (ControlPoints.Count == 0)
            {
                return false;
            }

            //Set the minimum distance to half the brush radius
            float minDistance = BrushRadius / 2f;

            //Check against the selected node for a quick check
            //Debug.Log("Check all:" + checkAll + " Selected node index:" + SelectedNodeIndex + " Control points count:" + m_controlPoints.Count);
            if (Vector3.Distance(ControlPoints[SelectedNodeIndex].Position, position) < minDistance)
            {
                //Debug.LogWarning("Too close to selected node");
                return true;
            }

            //Now check all other nodes if required
            if (checkAll)
            {
                for (int i = 0; i < ControlPoints.Count; i++)
                {
                    if (Vector3.Distance(ControlPoints[i].Position, position) < minDistance)
                    {
                        return true;
                    }
                }
            }

            //We are good
            return false;
        }

        #endregion

        #region Gizmos handling for outline based visualization

        /// <summary>
        /// Visualize the brush hits in the editor
        /// </summary>
        void OnDrawGizmos()
        {
            VisualizeMask();
        }

        /// <summary>
        /// Visualize the brush hits in the editor
        /// </summary>
        void OnDrawGizmosSelected()
        {
            VisualizeMask();
        }

        /// <summary>
        /// Visualize the mask in the editor
        /// </summary>
        private void VisualizeMask()
        {
            if (ControlPoints.Count < 3)
            {
                return;
            }
            Color oldColor = Gizmos.color;
            Color newColor = MaskColor;
            newColor.a = 1f;
            Gizmos.color = newColor;

            for (int i = 0; i < ControlPoints.Count - 1; i++)
            {
                if (IsOnScreen(ControlPoints[i].Position) || IsOnScreen(ControlPoints[i + 1].Position))
                {
                    Gizmos.DrawLine(ControlPoints[i].Position, ControlPoints[i + 1].Position);
                }
            }
            if (ControlPoints.Count > 2)
            {
                if (IsOnScreen(ControlPoints[ControlPoints.Count - 1].Position) || IsOnScreen(ControlPoints[0].Position))
                {
                    Gizmos.DrawLine(ControlPoints[ControlPoints.Count - 1].Position, ControlPoints[0].Position);
                }
            }
            Gizmos.color = oldColor;
        }

        /// <summary>
        /// Check that the position is on screen and within a certain distance
        /// </summary>
        /// <param name="position"></param>
        /// <returns>True if on screen and within range</returns>
        public bool IsOnScreen(Vector3 position)
        {
            Vector3 onScreen = Camera.current.WorldToViewportPoint(position);
            return onScreen.z > 0f && onScreen.x > 0f &&
                   onScreen.y > 0f && onScreen.x < 1f &&
                   onScreen.y < 1f && onScreen.z < MaxDrawDistance;
        }

        /// <summary>
        /// Check that the position is on screen and within a certain distance
        /// </summary>
        /// <param name="position"></param>
        /// <param name="maxDistance"></param>
        /// <returns>True if on screen and within range</returns>
        public bool IsOnScreen(Vector3 position, float maxDistance = float.MaxValue)
        {
            Vector3 onScreen = Camera.current.WorldToViewportPoint(position);
            return onScreen.z > 0f && onScreen.x > 0f &&
                   onScreen.y > 0f && onScreen.x < 1f &&
                   onScreen.y < 1f && onScreen.z < maxDistance;
        }

        public void SetDirty()
        {
            m_tileDataIsDirty = true;
            m_tileRegionIsDirty = true;
        }
        #endregion

        #region Area mask handling region based visualization

        /// <summary>
        /// Centers the transform of the mask
        /// </summary>
        public void CenterTransform()
        {
            Vector3 centre = CalculateVisualizationCentre(transform.position);
            transform.position = centre;
            m_wasMoved = true;
            UpdateVisualization();
            UpdateStormData();
        }

        /// <summary>
        /// Generates the center of all of the points in the list
        /// </summary>
        Vector3 CalculateVisualizationCentre(Vector3 position)
        {
            Vector3 centre = position;
            if (ControlPoints.Count == 0)
            {
                return centre;
            }

            Bounds b = new Bounds(ControlPoints[0].Position, Vector3.zero);
            foreach (StormRegionMaskNode node in ControlPoints)
            {
                b.Encapsulate(node.Position);
            }
            centre = b.center;

            //Pin it to the groung collider 
            RaycastHit hit;
            Ray ray = new Ray(centre + Vector3.up * 1500f, Vector3.down);
            if (Physics.Raycast(ray,out hit, 3000f, m_groundLayers))
            {
                centre = hit.point;
            }
            return centre;
        }


        /// <summary>
        /// Generates the centroid of all of the points in the list,
        /// </summary>
        Vector3 CalculateVisualizationCentroid(Vector3 position)
        {
            Vector3 centroid = position;
            if (ControlPoints.Count == 0)
            {
                return centroid;
            }

            foreach (StormRegionMaskNode node in ControlPoints)
            {
                centroid += node.Position;
            }
            centroid /= ControlPoints.Count;

            //Pin it to the groung collider 
            RaycastHit hit;
            Ray ray = new Ray(centroid + Vector3.up * 1500f, Vector3.down);
            if (Physics.Raycast(ray,out hit, 3000f, m_groundLayers))
            {
                centroid = hit.point;
            }
            return centroid;
        }

        /// <summary>
        /// Update the visualization on each terrain
        /// </summary>
        public void UpdateVisualization()
        {
            if (!Application.isEditor)
            {
                return;
            }

            //Get or create the visualizer
            if (m_visualizer == null)
            {
                //Try and find in children
                m_visualizer = transform.GetComponentInChildren<StormRegionMaskVisualizer>();
                if (m_visualizer == null)
                {
                    //Create the visualizer
                    GameObject visualizerGO = new GameObject("Visualizer");
                    if (HideChildVisualizer)
                    {
                        visualizerGO.hideFlags = HideFlags.HideAndDontSave;
                    }
                    visualizerGO.transform.parent = transform;
                    m_visualizer = visualizerGO.AddComponent<StormRegionMaskVisualizer>();
                }
            }

            //Update the visualizer
            if (m_visualizer != null)
            {
                m_visualizer.Points = ControlPoints.Select(node => node.Position).ToList();
                m_visualizer.VisualizationColor = MaskColor;
                m_visualizer.ShowVisualization = ShadeVisualization;
            }

            //Call the updated event
            if (OnVisualizationUpdated != null)
            {
                OnVisualizationUpdated(this);
            }
        }

        /// <summary>
        /// Increment the color of the mask
        /// </summary>
        /// <param name="percentage">Amount to increment it by</param>
        private void IncrementColor(float percentage = 0.10f)
        {
            float h, s, v;
            Color.RGBToHSV(MaskColor, out h, out s, out v);
            h += percentage;
            if (h > 1.0f)
            {
                h = 1f - h;
            }
            MaskColor = Color.HSVToRGB(h, 1f, 1f);
        }

        /// <summary>
        /// Show the visualization meshes
        /// </summary>
        public void ShowVisualizationMeshes()
        {
            if (m_visualizer == null)
            {
                UpdateVisualization();
            }
            if (m_visualizer != null)
            {
                m_visualizer.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide the visualization meshes
        /// </summary>
        public void HideVisualizationMeshes()
        {
            if (m_visualizer != null)
            {
                m_visualizer.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Region methods
      
        /// <summary>
        /// Add a region link
        /// </summary>
        /// <param name="region">region to add</param>
        public void AddRegionLink(StormRegion region)
        {
            if (!m_regions.Contains(region))
            {
                StormUtils.Add(ref m_regions, region);
                region.AddRegionMask(this);
                UpdateRegions();
            }
        }

        /// <summary>
        /// Remove a region link
        /// </summary>
        /// <param name="region">region to remove</param>
        public void RemoveRegionLink(StormRegion region)
        {
            for (int idx = 0; idx < m_regions.Length; ++idx)
            {
                if (m_regions[idx] == region)
                {
                    region.RemoveRegionMask(this);
                    StormUtils.RemoveAt(ref m_regions, idx);
                    UpdateRegions();
                }
            }
        }

        /// <summary>
        /// Add all regions to the mask
        /// </summary>
        public void UpdateRegions()
        {
            for (int r = 0; r < m_regions.Length; ++r)
            {
                if (m_regions[r] != null)
                {
                    m_regions[r].AddRegionMask(this);
                    if (!m_publishedRegions.Contains(m_regions[r]))
                    {
                        m_publishedRegions.Add(m_regions[r]);
                        m_tileRegionIsDirty = true;
                    }
                }
            }
            for (int r = 0; r < m_publishedRegions.Count; ++r)
            {
                if (!m_regions.Contains(m_publishedRegions[r]))
                {
                    m_publishedRegions[r].RemoveRegionMask(this);
                    m_publishedRegions.RemoveAt(r--);
                    m_tileRegionIsDirty = true;
                }
            }
            if (m_tileRegionIsDirty)
                UpdateStormData();
        }

        /// <summary>
        /// Remove all regions from the mask
        /// </summary>
        void RemoveRegions()
        {
            m_publishedRegions.ForEach((r) => r.RemoveRegionMask(this));
            m_publishedRegions.Clear();
        }
        #endregion

        #region Storm Tile Methods

        /// <summary>
        /// The tile data for the mask
        /// </summary>
        public StormTileData TileData 
        {
            get 
            {
                bool created = false;
                if (m_stormTileData == null)
                {
                    m_stormTileData = new StormTileData();
                    m_tileDataIsDirty = true;
                    created = true;
                }
                if (m_tileDataIsDirty)
                {
                    m_stormTileData.m_myObject = this.gameObject;
                    //first we need to find the min/max of our tile in world space
                    Vector3 posMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    Vector3 posMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
                    int pointCount = ControlPoints.Count;
                    for (int p = 0; p < pointCount; ++p)
                    {
                        Vector3 pos = ControlPoints[p].Position;
                        posMin = Vector3.Min(posMin, pos);
                        posMax = Vector3.Max(posMax, pos);
                    }
                    //set our tile position and size
                    posMin -= new Vector3(RegionFalloff * 2f, 0, RegionFalloff * 2f);
                    posMax += new Vector3(RegionFalloff * 2f, 0, RegionFalloff * 2f);
                    m_stormTileData.Position = posMin;
                    m_stormTileData.Size = posMax - posMin;
                    if (created)
                        m_stormTileData.LastBounds = m_stormTileData.Bounds;
                    else if (m_stormTileData.LastBounds != m_stormTileData.Bounds)
                    {
                        //our bounds changed. Region list should update their bounds as well
                        m_tileRegionIsDirty = true;
                    }
                    //now create our point list as relative positions from 0-1
                    Vector2[] Points = new Vector2[pointCount];
                    for (int p = 0; p < pointCount; ++p)
                        Points[p] = new Vector2((ControlPoints[p].Position.x - posMin.x) / m_stormTileData.Size.x, (ControlPoints[p].Position.z - posMin.z) / m_stormTileData.Size.z);

                    if (m_stormTileData.PolygonData.Length != 1)
                        Array.Resize(ref m_stormTileData.PolygonData, 1);
                    if (m_stormTileData.PolygonData[0] == null)
                        m_stormTileData.PolygonData[0] = new PolygonData();
                    PolygonData polyData = m_stormTileData.PolygonData[0];
                    //set our polygon data
                    if (polyData.Data?.Length != 1)
                        Array.Resize(ref polyData.Data, 1);
                    polyData.LayerName = RegionID;
                    polyData.Data = new PolyPointSet[] { new PolyPointSet(Points) };
                    polyData.Falloffs = new float[] { RegionFalloff };
                    polyData.Values = new float[] { RegionStrength };
                    polyData.SetDirty();
                    m_tileDataIsDirty = false;
                }
                return m_stormTileData;
            }
        }
        public StormTileRegion TileRegion
        {
            get {
                bool created = false;
                if (m_stormTileRegion == null)
                {
                    m_stormTileRegion = new StormTileRegion();
                    m_tileRegionIsDirty = true;
                    created = true;
                }
                if (m_tileRegionIsDirty)
                {
                    m_stormTileRegion.m_myObject = this.gameObject;
                    m_stormTileRegion.Position = TileData.Position;
                    m_stormTileRegion.Size = TileData.Size;
                    if (created)
                        m_stormTileRegion.LastBounds = m_stormTileRegion.Bounds;
                    if (m_stormTileRegion.Regions?.Length != m_regions.Length)
                        Array.Resize(ref m_stormTileRegion.Regions, m_regions.Length);
                    for (int r = 0; r < m_regions.Length; ++r)
                        m_stormTileRegion.Regions[r] = m_regions[r];
                    m_tileRegionIsDirty = false;
                }
                return m_stormTileRegion;
            }
        }

        /// <summary>
        /// Tell Storm to get a new version of the tile data
        /// </summary>
        public void UpdateStormData()
        {
            bool wasDataDirty = m_tileDataIsDirty;
            bool wasRegionDirty = m_tileRegionIsDirty; //cache these values since they are reset when we call TileRegion and TileData get functions
            if (!m_tileRegionIsAdded)
            {
                if (TileRegion.Regions.Length > 0)
                {
                    m_tileRegionIsAdded = true;
                    StormAPI.AddStormTile(TileRegion);
                    TileRegion.LastBounds = TileRegion.Bounds;
                }
            }
            else
            {
                if (TileRegion.Regions.Length == 0)
                {
                    StormAPI.RemoveStormTile(TileRegion);
                    m_tileRegionIsAdded = false;
                }
                else if (wasRegionDirty)
                    TileRegion.DoRegionsChanged();
            }
            if (!m_tileDataIsAdded)
            {
                if (TileData.HasData())
                {
                    m_tileDataIsAdded = true;
                    StormAPI.AddStormTile(TileData);
                    TileData.LastBounds = TileData.Bounds;
                }
            }
            else
            {
                if (!TileData.HasData())
                {
                    StormAPI.RemoveStormTile(TileData);
                    m_tileDataIsAdded = false;
                }
                else if (wasDataDirty)
                    TileData.DoLayerChanged(RegionID); //this should send SetDirty and UpdateTile commands
            }
        }

        /// <summary>
        /// Handle the storm instance changed event
        /// </summary>
        /// <param name="stormWorld">storm world</param>
        void OnStormInstanceChanged(MonoBehaviour stormWorld)
        {
            m_tileDataIsAdded = false;
            m_tileRegionIsAdded = false;
            m_tileDataIsDirty = true;
            m_tileRegionIsDirty = true;
            if (stormWorld != null)
            {
                if (TileData.HasData())
                {
                    m_tileDataIsAdded = true;
                    StormAPI.AddStormTile(TileData);
                }
                if (TileRegion.Regions.Length > 0)
                {
                    m_tileRegionIsAdded = true;
                    StormAPI.AddStormTile(TileRegion);
                }
            }
        }

        #endregion
    }
}