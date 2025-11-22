using UnityEngine;
[CreateAssetMenu(fileName = "New Building Block", menuName = "Procedural Worlds/Storm/Building Generator/Building Block")]
public class BuildingBlockWrapper : ProceduralWorlds.Storm.BuildingGen.BuildingBlock
{
        public new void OnValidate()
        {
            Validate();
        }

        public override void Validate()
        {
            base.Validate();
            BuildingBlockObjectWrapper currentObject = null;
            foreach (BuildingBlockData buildingBlockData in blockObjList)
            {
                if (buildingBlockData.prefab == null)
                {
                    continue;
                }
                currentObject = buildingBlockData.prefab.GetComponent<BuildingBlockObjectWrapper>();
                if (currentObject == null)
                {
                    currentObject = buildingBlockData.prefab.AddComponent<BuildingBlockObjectWrapper>();
                }

                currentObject.sourceBlock = this;
                currentObject.sourcePrefab = buildingBlockData.prefab;
            }
        }
}
