using System.Collections.Generic;
using UnityEngine;
namespace ProceduralWorlds.Storm.AreaPopulator
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Decoration Data", menuName = "Procedural Worlds/Storm/Area Populator/Decoration Data")]
    public class DecorationData : ScriptableObject
    {
        public LayerMask collisionLayers;
        public LayerMask terrainLayer;
        public bool checkForOpenings;
        public bool occupyPathways;

        public float pathWidth = 0;



        public float openingCheckDistance;
        public float openingPadding;

        public bool checkForGround;

        public float segmentSize;

        public int cornerCheckDepth = 1;


        public int selectedDecorationObjectInt = 0;
        [SerializeField]
        public List<DecorationObjectInfo> decorationObjects = new List<DecorationObjectInfo>();

        #region Editor Panel States
        [HideInInspector]
        public bool areaSettingsPanelExpanded = false;
        [HideInInspector]
        public bool subdivisionSettingsPanelExpanded = false;
        [HideInInspector]
        public bool decorationObjectsPanelExpanded = false;
        #endregion
    }


}
