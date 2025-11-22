using System.Collections.Generic;
using UnityEngine;


using System.Linq;
#if GAIA_INPUT_SYSTEM || PW_STORM_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralWorlds.Storm.Terrain
{

    public class StormVTToggleDebug : MonoBehaviour
    {

        #region Public Variables
        /// <summary>
        /// Reference to the storm terrain scriptable object
        /// </summary>
        public StormVTSettings stormVTSettings;

        /// <summary>
        /// Key the screenshotter is bound to
        /// </summary>
        #if GAIA_INPUT_SYSTEM
            public Key m_debugTilesKey = Key.J;
            public Key m_debugTerrainKey = Key.U;
        #else
        #if USE_INPUT_SYSTEM
            public Key m_debugTilesKey = Key.J;
            public Key m_debugTerrainKey = Key.U;
        #else
            public KeyCode m_debugTilesKey = KeyCode.J;
            public KeyCode m_debugTerrainKey = KeyCode.U;
        #endif
        #endif
        #endregion


        //Check whether the button was pressed to enable a debug of the tiles
        private void LateUpdate()
        {
#if GAIA_INPUT_SYSTEM
        if (Keyboard.current[m_debugTilesKey].wasPressedThisFrame)
        {
            ToggleDebug();
        }
        if (Keyboard.current[m_debugTerrainKey].wasPressedThisFrame)
        {
                ToggleTerrainDebug();
        }

#else
#if USE_INPUT_SYSTEM
        if (Keyboard.current[m_debugTilesKey].wasPressedThisFrame)
        {
            ToggleDebug();
        }
        if (Keyboard.current[m_debugTerrainKey].wasPressedThisFrame)
        {
            ToggleTerrainDebug();
        }
#else
        if(Input.GetKeyDown(m_debugTilesKey))
        {
            ToggleDebug();
        }
        if (Input.GetKeyDown(m_debugTerrainKey))
        {
                ToggleTerrainDebug();
        }

#endif
#endif
        }

        /// <summary>
        /// Swap between debug tile display state
        /// </summary>
        void ToggleDebug()
        {
            if (stormVTSettings != null)
            {
                stormVTSettings.m_debugTiles = !stormVTSettings.m_debugTiles;
                Shader.SetGlobalInteger(StormVTShaderIDs.m_stormVT_DebugTiles, stormVTSettings.m_debugTiles ? 1 : 0);
#if UNITY_EDITOR
            EditorUtility.SetDirty(stormVTSettings);
#endif
            }
        }

        void ToggleTerrainDebug()
        {
            if (stormVTSettings != null)
            {
                stormVTSettings.m_toggleTerrain = ! stormVTSettings.m_toggleTerrain;
                stormVTSettings.ForceSettingsUpdate();
#if UNITY_EDITOR
                EditorUtility.SetDirty(stormVTSettings);
#endif
            }
        }

        /// <summary>
        /// Find the storm terrain settings
        /// </summary>
        public void OnValidate()
        {
            FindStormVTSettings();
        }

        /// <summary>
        /// Find the storm terrain settings
        /// </summary>
        public void OnEnable()
        {
            FindStormVTSettings();
        }

        /// <summary>
        /// Find the storm terrain settings
        /// </summary>
        public void OnAwake()
        {
            FindStormVTSettings();
        }

        /// <summary>
        /// Searches the for storm terrains scriptable object, attched to the storm terrain manager
        /// </summary>
        public void FindStormVTSettings()
        {
            if (stormVTSettings == null)
            {
                List<StormVTManager> managers = StormVTUtilities.FindObjectsByType<StormVTManager>(false).ToList();
                if (managers.Count > 0)
                {
                    if (managers[0].stormVTSettings != null)
                    {
                        stormVTSettings = managers[0].stormVTSettings;
                    }
                }
            }
        }

    }
}
