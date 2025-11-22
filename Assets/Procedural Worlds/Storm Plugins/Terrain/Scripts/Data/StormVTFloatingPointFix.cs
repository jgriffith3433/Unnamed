using ProceduralWorlds.Storm.Utils;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace ProceduralWorlds.Storm.Terrain
{


    public class StormVTFloatingPointFix : MonoBehaviour
    {

        /// <summary>
        /// Settings for the storm virtual terrain
        /// </summary>
        public StormVTSettings stormVTSettings;
        
        /// <summary>
        /// Event to subscribe to for floating points updated
        /// </summary>
        public UnityEvent OnFloatingPointUpdated = new UnityEvent();

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static StormVTFloatingPointFix instance = null;


        /// <summary>
        /// Returns the current Floating Point Fix Instance in the scene
        /// </summary>
        public static StormVTFloatingPointFix Instance
        {
            get
            {
                if (instance == null)
                    instance = (StormVTFloatingPointFix)FindAnyObjectByType(typeof(StormVTFloatingPointFix));
                if (instance == null && Application.isPlaying)
                    Debug.LogError("No Floating Point Fix Instance could be found, please add a Floating Point Fix component to the main player / camera object.");
                return instance;
            }
        }
        /// <summary>
        /// Returns true if the Floating Point Fix is active in this scene
        /// </summary>
        public static bool IsActive
        {
            get
            {
                if (instance == null)
                    instance = (StormVTFloatingPointFix)FindAnyObjectByType(typeof(StormVTFloatingPointFix));
                return instance != null;
            }
        }


        /// <summary>
        /// The distance from origin at which the floating point fix shift will be performed. Whenever the object that contains the floating point fix component
        /// moves further than this distance from 0,0,0 all floating point fix members and sectors will be shifted back to the origin. 
        /// </summary>
        public float threshold = 1000.0f;

        /// <summary>
        /// This vector3 represents the cumulative offset so far
        /// </summary>
        public double3 totalOffset = double3.zero;


        /// <summary>
        /// Makes sure we have a singleton and sets the sectors in the scene up for the floating point fix mode.
        /// </summary>
        void OnEnable()
        {
            //Singleton pattern
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            if (stormVTSettings != null)
            {
                threshold = stormVTSettings.m_floatingPointRange;
            }

            StormVTGaia.Instance.DisableGaiaFPF();
        }


        /// <summary>
        /// Converts a Vector3 to its "real" / original position as if the floating point fix would not exist.
        /// </summary>
        /// <param name="currentPosition">The </param>
        /// <returns></returns>
        public double3 ConvertToOriginalSpace(double3 position)
        {
            return position += totalOffset;
        }

        /// <summary>
        /// Update the threshold value if changed
        /// </summary>
        private void OnValidate()
        {
            if (stormVTSettings != null)
            {
                threshold = stormVTSettings.m_floatingPointRange;
            }
        }

        /// <summary>
        /// Update the threshold value if changed
        /// </summary>
        private void OnAwake()
        {
            if (stormVTSettings != null)
            {
                threshold = stormVTSettings.m_floatingPointRange;
            }
        }

        /// <summary>
        /// Each update check if the camera has moved and if we should perform a floating point remap
        /// </summary>
        void LateUpdate()
        {
            
            if (stormVTSettings.m_floatingPointFix)
            {
                
                Vector3Double currentPosition = gameObject.transform.position;
                currentPosition.y = 0;

                if (currentPosition.magnitude > threshold)
                {
                    totalOffset += (double3)((float3)gameObject.transform.position);
                

                    totalOffset.y = 0f;

                    OnFloatingPointUpdated?.Invoke();

                    //Move Storm vegetation - if any

                    if (StormAPI.IsStormWorldActive)
                    {
                        Debug.Log($"Setting Storm offset X {totalOffset.x} Y{totalOffset.y} Z {totalOffset.z}");
                        StormAPI.ShiftWorldOffset(totalOffset);
                    }

                    //Move terrains in scene

                    //Check for gaia loaded scene setup
                    if (StormVTGaia.Instance.IsGaiaTerrainLoadedScene())
                    {
                        if (StormVTGaia.Instance.IsGaiaFloatingPointFixScene())
                        {
                            Debug.Log($"Setting Gaia offset X {totalOffset.x} Y{totalOffset.y} Z {totalOffset.z}");
                            StormVTGaia.Instance.SetGaiaFloatingPointFixOrigin(totalOffset.x, totalOffset.y, totalOffset.z);
                            gameObject.transform.position = new Vector3(0f, transform.position.y, 0f);
                        }
                        else 
                        {
                            gameObject.transform.position = new Vector3(0f, transform.position.y, 0f);
                            List<StormVTTerrainInfo> terrainInfos = StormVTUtilities.FindObjectsByType<StormVTTerrainInfo>(true).ToList();
                            for (int i = 0; i < terrainInfos.Count; i++)
                            {
                                terrainInfos[i].gameObject.transform.position = (float3)((float3)terrainInfos[i].m_terrrainOrigin - totalOffset);
                            }
                        }
                    }
                    else //Not a Gaia Loaded Scene
                    {

                        //No Gaia Floating Point Fix - we must perform the move ourselves
                        if (!StormVTGaia.Instance.IsGaiaFloatingPointFixScene())
                        {
                            gameObject.transform.position = new Vector3(0f, transform.position.y, 0f);
                            List<StormVTTerrainInfo> terrainInfos = StormVTUtilities.FindObjectsByType<StormVTTerrainInfo>(true).ToList();
                            for (int i = 0; i < terrainInfos.Count; i++)
                            {
                                terrainInfos[i].gameObject.transform.position = (float3)((float3)terrainInfos[i].m_terrrainOrigin - totalOffset);
                            }
                        }
                        
                    }

                }
            }
        }
        
    }

}
