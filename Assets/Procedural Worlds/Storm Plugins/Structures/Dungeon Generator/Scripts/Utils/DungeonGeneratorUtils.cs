using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProceduralWorlds.Storm.BuildingGen;
using ProceduralWorlds.Storm.StructureGenCore;
namespace ProceduralWorlds.Storm.DungeonGen
{
    public static class DungeonGeneratorUtils
    {
        /// <summary>
        /// returns true if the point lies within the polygon, false if it does not
        /// </summary>
        /// <param name="polygon">a list of the polygon vertexes</param>
        /// <param name="point">the point to test</param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<Vector2> polygon, Vector2 point)
        {
            bool inside = false;
            //For each edge (In this case for each point of the polygon and the previous one)
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; i++)
            {
                //If a line from the point into infinity on the right crosses this edge
                if (((polygon[i].y > point.y) != (polygon[j].y > point.y))
                        && (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
                {
                    inside = !inside;
                }
                j = i;
            }
            return inside;
        }

        public static GridCellMapData GenerateGrid(float gridSize, Vector2 startPoint, Bounds bounds, ref List<Vector2Int> availibleCellsList, List<Vector2> availibeArea = null)
        {
            GridCellMapData gridCellMapData = new GridCellMapData();
            int xSteps = Mathf.FloorToInt(bounds.extents.x*2 / gridSize);
            int ySteps = Mathf.FloorToInt(bounds.extents.z*2 / gridSize);

            GridCell[,] gridCellMap = new GridCell[xSteps, ySteps];
            for (int i = 0; i < xSteps; i++)
            {
                for (int k = 0; k < ySteps; k++)
                {
                    Vector2 cellPos = new Vector2(startPoint.x + i*gridSize, startPoint.y+k*gridSize);


                    GridCell gridCell = new GridCell();
                    gridCell.position = cellPos;
                    if (availibeArea != null)
                    {
                        if (!IsPointInPolygon(availibeArea, cellPos))
                        {
                            gridCell.cellStatus = CellStatus.Forbidden;

                        }
                    }
                    availibleCellsList.Add(new Vector2Int(i, k));
                    gridCellMap[i, k] = gridCell;
                }
            }
            gridCellMapData.gridCellMap = gridCellMap;
            gridCellMapData.origin = startPoint;
            gridCellMapData.bounds = new Vector2(bounds.extents.x*2, bounds.extents.z*2);
            gridCellMapData.gridSize = gridSize;
            return gridCellMapData;
        }

        public static Bounds GetLevelBounds(DungeonLayer parentLayer, DungeonLevel level, Vector3 startPoint)
        {
            //THIS METHOD IS CURRENTLY BROKEN, NEED TO FIX
            Bounds bounds = new Bounds();
            bounds.extents = new Vector3(parentLayer.levelSize, 2.5f, parentLayer.levelSize);
            return bounds;

        }

        private static bool IsCheckCellValid(bool[,] checkGrid, Vector2Int checkPos, Vector3 checkPosWorld, Vector3 checkSize, LayerMask checkLayers)
        {
            //SHOULD BE RETURNING FALSE HERE IN THE IFS BUT IM HOTFIXING
            if ((checkPos.x < 0 || checkPos.y < 0) || (checkPos.x >= checkGrid.GetLength(0) || checkPos.y >= checkGrid.GetLength(1)))
            {
                Debug.DrawLine(checkPosWorld, checkPosWorld + new Vector3(0, 100, 0), Color.red, 1000f);
                return false;

            }
            if (Physics.CheckBox(checkPosWorld, checkSize, Quaternion.identity, checkLayers))
            {
                Debug.DrawLine(checkPosWorld, checkPosWorld + new Vector3(0, 100, 0), Color.red, 1000f);
                return false;
            }
            Debug.DrawLine(checkPosWorld, checkPosWorld + new Vector3(0, 100, 0), Color.green, 1000f);
            return true;
        }

        public static List<Vector2Int> CreateGridRectangle(Vector2Int center, Vector2Int lengthWidth)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            Vector2Int bottomleftPoint = new Vector2Int(center.x - lengthWidth.x / 2, center.y - lengthWidth.y / 2);
            for (int i = 0; i < lengthWidth.x; i++)
            {
                for (int k = 0; k < lengthWidth.y; k++)
                {
                    cells.Add(bottomleftPoint + new Vector2Int(i, k));
                }
            }
            return cells;
        }

        public static List<Vector2Int> CreateGridCircle(Vector2Int center, int radius)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            cells.Add(center);

            int xSteps = 0;
            int ySteps = 0;

            int steps = 180;
            float radSteps = (Mathf.PI * 2) / steps;
            for (int i = 0; i < steps; i++)
            {
                Vector2Int currentPoint = center;
                float currentRad = radSteps * i;
                xSteps = Mathf.RoundToInt(Mathf.Cos(currentRad) * radius);
                
                ySteps = Mathf.RoundToInt(Mathf.Sin(currentRad) * radius);
                for (int k = 0; k < Mathf.Abs(xSteps); k++)
                {
                    if(xSteps >= 0)
                    {
                        currentPoint += new Vector2Int(1, 0);
                    }
                    else
                    {
                        currentPoint -= new Vector2Int(1, 0);
                    }
                    
                    if (!cells.Contains(currentPoint))
                    {
                        cells.Add(currentPoint);
                    }
                }
                for (int k = 0; k < Mathf.Abs(ySteps); k++)
                {
                    if (ySteps >= 0)
                    {
                        currentPoint += new Vector2Int(0, 1);
                    }
                    else
                    {
                        currentPoint -= new Vector2Int(0, 1);
                    }
                    if (!cells.Contains(currentPoint))
                    {
                        cells.Add(currentPoint);
                    }
                }
            }


            return cells;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dungeonLayer"></param>
        /// <param name="level"></param>
        /// <param name="levelConnector">Setting this bool to true will cause any rooms generated to have a second floor going down</param>
        /// <returns></returns>
        public static DungeonChamber CreateNewChamber(ref DungeonLayer dungeonLayer, int level, int chamberId, XorshiftPlus randomGen, bool levelConnector = false, CustomChamber customChamber = null, Transform parent = null, bool mirror = false)
        {
            DungeonChamber newChamber = new DungeonChamber();
            //this is currently hardcoded, intend to randomize this in the future
            newChamber.chamberArchType = MultiLevelChamberArchType.ConstrainedFloor;
            newChamber.chamberType = MultiLevelChamberType.FullLevel;
            newChamber.chamberID = chamberId;
            DungeonLevel dungeonLevel = dungeonLayer.layerLevels[level];
            GridCellMapData gridCellMapData = dungeonLevel.gridCellMapData;
            GridCell[,] gridCellMap = gridCellMapData.gridCellMap;
            newChamber.chamberTheme = dungeonLayer.defaultChamberTheme;
            ChamberFormationProfile chamberFormationProfile = dungeonLayer.defaultChamberFormationProfile;
            newChamber.chamberConnectionType = ChamberConnectionType.Open;
            if(customChamber != null /* && customChamber.procedural*/)
            {
                chamberFormationProfile = customChamber.chamberFormationProfile;
                newChamber.chamberTheme = customChamber.chamberTheme;
                newChamber.chamberConnectionType = customChamber.chamberConnectionType;
                newChamber.definedEnteranceType = customChamber.definedEnteranceType;
                newChamber.allowedFloors = customChamber.allowedFloors;
                newChamber.hallsAvoidChamber = customChamber.hallsAvoidChamber;
            }
            

            //how many shapes will we add to the inital one?
            Vector2Int shapeIterationRange = Vector2Int.one;

            Vector2Int circleRadisRange = Vector2Int.one;

            Vector2Int rectangleLengthRange = Vector2Int.one;
            Vector2Int rectangleWidthRange = Vector2Int.one;

            Vector2Int levelRange = Vector2Int.one;
            float rectangleChance = 1;

            if (chamberFormationProfile != null)
            {
                GetChamberFormationData(chamberFormationProfile, out shapeIterationRange, out circleRadisRange, out rectangleLengthRange, out rectangleWidthRange, out levelRange, out rectangleChance, dungeonLayer.gridSize);
            }
            else
            {
                //Debug.LogWarning("chamberFormationProfile was null");
            }
            

            int desiredChamberLevels = RandomIntInRange(levelRange, randomGen);

            List<Vector2Int> availableCells = dungeonLayer.layerLevels[level].availibleCells;

            int placementAttempts = 0;
            bool acceptPlacement = false;
            List<Vector2Int> cells = new List<Vector2Int>();

            Vector2Int startLocation = Vector2Int.zero;
            startLocation = availableCells[0];



            while (acceptPlacement == false)
            {
                if (placementAttempts >= 40)
                {
                    //Debug.LogWarning("placement failed");
                    return null;
                }
                //get the startLocation
                int startCellId = randomGen.Next(0, availableCells.Count-1);
                startLocation = availableCells[startCellId];

                //clear any cells that may have existed before
                cells.Clear();
                
                if(customChamber == null)
                {

                    List<Vector2Int> checkCells = new List<Vector2Int>();

                    AddInitalRoomShapeToCells(ref cells, randomGen, startLocation, rectangleChance, rectangleWidthRange, rectangleLengthRange, circleRadisRange, ref checkCells);

                    //check if this shape fits in the dungeon
                    //set accept placement to true
                    acceptPlacement = true;

                    for (int i = 0; i < checkCells.Count; i++)
                    {
                        //THIS COULD BE OPTIMIZED WITH BOUNDS CHECKS I THINK!!
                        if (!CheckIfTileIsValid(checkCells[i], dungeonLevel.gridCellMapData, false, mirror))
                        {
                            acceptPlacement = false;
                            break;
                        }
                    }

                }
                else
                {
                    if(customChamber.procedural == false)
                    {
                        startLocation = customChamber.origin;
                        for (int l = 0; l < customChamber.levelHeight; l++)
                        {
                            DungeonLevel dungeonLevel2 = dungeonLayer.layerLevels[level - l];

                            for (int i = 0; i < customChamber.bounds.x; i++)
                            {
                                for (int k = 0; k < customChamber.bounds.y; k++)
                                {
                                    Vector2Int currentPos = startLocation + new Vector2Int(i, k);
                                    
                                    cells.Add(currentPos);
                                    if (CheckIfTileIsValid(currentPos, dungeonLevel.gridCellMapData))
                                    {
                                        availableCells.Remove(currentPos);
                                    }
                                }
                            }
                            AddChamberToLevel(ref newChamber, ref dungeonLevel, gridCellMap, new List<Vector2Int>(cells));

                            cells.Clear();
                        }
                        Vector3 offset = new Vector3(((float)customChamber.bounds.x / 2) * dungeonLayer.gridSize, 0, ((float)customChamber.bounds.y / 2) * dungeonLayer.gridSize);
                        //ADDED NEGATIVE 1006 TO Y BECAUSE DUNGEON IS NOT GENERATED FROM SURFACE IN THIS VERSION
                        Vector3 spawnPoint = offset + new Vector3(gridCellMapData.origin.x + (startLocation.x * dungeonLayer.gridSize) - dungeonLayer.gridSize / 2, dungeonLevel.yLevel, gridCellMapData.origin.y + (startLocation.y * dungeonLayer.gridSize) - dungeonLayer.gridSize / 2);
                        if(customChamber.roomPrefab != null)
                        {
                            //GameObject temp = Object.Instantiate(customChamber.roomPrefab, spawnPoint, Quaternion.identity, parent);
                            newChamber.spawnPoint = spawnPoint;
                            newChamber.roomPrefab = customChamber.roomPrefab;
                            newChamber.custom = true;
                            newChamber.hallsAddExtraCell = true;
                        }
                        else
                        {
                            newChamber.custom = false;
                        }
                        return newChamber;
                    }
                    else
                    {
                        int maxLength = Mathf.Max(rectangleLengthRange.x, rectangleLengthRange.y);
                        int maxWidth = Mathf.Max(rectangleWidthRange.x, rectangleWidthRange.y);
                        Vector2Int xArea = new Vector2Int(maxWidth, gridCellMap.GetLength(0) - maxWidth);
                        Vector2Int yArea = new Vector2Int(maxLength, gridCellMap.GetLength(1) - maxLength);
                        startLocation = new Vector2Int(randomGen.Next(xArea.x, xArea.y), randomGen.Next(yArea.x, yArea.y));
                        List<Vector2Int> checkCells = new List<Vector2Int>();
                        AddInitalRoomShapeToCells(ref cells, randomGen, startLocation, rectangleChance, rectangleWidthRange, rectangleLengthRange, circleRadisRange, ref checkCells);

                        //check if this shape fits in the dungeon
                        //set accept placement to true
                        acceptPlacement = true;
                        for (int i = 0; i < customChamber.levelHeight; i++)
                        {
                            int currentLevel = customChamber.startingLevel - (i);
                            DungeonLevel currentDungeonLevel = dungeonLayer.layerLevels[currentLevel];
                            for (int k = 0; k < checkCells.Count; k++)
                            {
                                //THIS COULD BE OPTIMIZED WITH BOUNDS CHECKS I THINK!!
                                if (!CheckIfTileIsValid(checkCells[k], currentDungeonLevel.gridCellMapData))
                                {
                                    acceptPlacement = false;
                                    break;
                                }
                            }
                        }

                    }
                    
                }
                placementAttempts++;
            }
            if(customChamber == null || customChamber.procedural)
            {
                cells = GetRoomShape(shapeIterationRange, rectangleChance, rectangleLengthRange, rectangleWidthRange, circleRadisRange, dungeonLevel.gridCellMapData, randomGen, cells);
            }


            AddChamberToLevel(ref newChamber, ref dungeonLevel, gridCellMap, cells);
            DungeonChamber mirrorChamber = null;
            if (mirror)
            {
                mirrorChamber = new DungeonChamber();
                mirrorChamber.chamberTheme = newChamber.chamberTheme;
                mirrorChamber.chamberArchType = newChamber.chamberArchType;
                mirrorChamber.chamberType = newChamber.chamberType;

                mirrorChamber.chamberConnectionType = newChamber.chamberConnectionType;
                mirrorChamber.definedEnteranceType = newChamber.definedEnteranceType;
                mirrorChamber.allowedFloors = newChamber.allowedFloors;
                mirrorChamber.hallsAvoidChamber = newChamber.hallsAvoidChamber;

                AddMirorredChamberToLevel(ref mirrorChamber, ref dungeonLevel, gridCellMap, cells);
            }




            if (customChamber == null)
            {
                if (levelConnector == true)
                {
                    desiredChamberLevels = 2;
                }
                if (desiredChamberLevels > 1)
                {
                    int upwardsExpantions = 0;
                    int downwardsExpantions = 0;
                    if (level == 0)
                    {

                        upwardsExpantions = 0;
                        downwardsExpantions = desiredChamberLevels - 1;
                    }
                    else
                    {
                        //should add settings here thatm allow more control over the split between downwards and upwards expansions
                        int possibleUpwardsExpansions = -level + dungeonLayer.layerLevels.Count - 1;
                        int possibleDownwardsExpansions = dungeonLayer.layerLevels.Count - 1 - level;
                        bool upDownSwitch = false;
                        if (randomGen.Next() < 0.5f)
                        {

                            upDownSwitch = false;
                        }
                        else
                        {
                            upDownSwitch = true;
                        }
                        if (levelConnector == true)
                        {
                            upDownSwitch = false;
                        }

                        for (int i = 0; i < desiredChamberLevels - 1; i++)
                        {
                            if (upDownSwitch == true)
                            {
                                if (possibleUpwardsExpansions > 0)
                                {
                                    upwardsExpantions += 1;
                                }
                                else if (possibleDownwardsExpansions > 0)
                                {
                                    downwardsExpantions += 1;
                                }
                            }
                            else
                            {
                                if (possibleDownwardsExpansions > 0)
                                {
                                    downwardsExpantions += 1;
                                }
                                else if (possibleUpwardsExpansions > 0)
                                {
                                    upwardsExpantions += 1;
                                }
                            }

                            upDownSwitch = !upDownSwitch;
                        }
                    }

                    List<Vector2Int> previousUpwardsExpansionCells = cells;
                    List<Vector2Int> previousDownwardsExpansionCells = cells;
                    for (int i = 0; i < upwardsExpantions; i++)
                    {
                        DungeonLevel expantionLevel = dungeonLayer.layerLevels[level - (i + 1)];
                        GridCell[,] expansionGridCellMap = expantionLevel.gridCellMapData.gridCellMap;
                        if (newChamber.chamberArchType == MultiLevelChamberArchType.ConstrainedFloor)
                        {
                            if (newChamber.chamberType == MultiLevelChamberType.FullLevel)
                            {
                                List<Vector2Int> removalList = new List<Vector2Int>();
                                for (int k = 0; k < previousUpwardsExpansionCells.Count; k++)
                                {
                                    if (!CheckIfTileIsValid(previousUpwardsExpansionCells[k], expantionLevel.gridCellMapData))
                                    {
                                        removalList.Add(previousUpwardsExpansionCells[k]);
                                    }
                                }
                                for (int k = 0; k < removalList.Count; k++)
                                {
                                    previousUpwardsExpansionCells.Remove(removalList[k]);
                                }
                                AddChamberToLevel(ref newChamber, ref expantionLevel, expansionGridCellMap, previousUpwardsExpansionCells);
                                if (mirror)
                                {
                                    AddMirorredChamberToLevel(ref mirrorChamber, ref expantionLevel, expansionGridCellMap, previousUpwardsExpansionCells);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < downwardsExpantions; i++)
                    {
                        DungeonLevel expantionLevel = dungeonLayer.layerLevels[level + (i + 1)];
                        GridCell[,] expansionGridCellMap = expantionLevel.gridCellMapData.gridCellMap;
                        if (newChamber.chamberArchType == MultiLevelChamberArchType.ConstrainedFloor)
                        {
                            if (newChamber.chamberType == MultiLevelChamberType.FullLevel)
                            {
                                List<Vector2Int> removalList = new List<Vector2Int>();
                                for (int k = 0; k < previousDownwardsExpansionCells.Count; k++)
                                {
                                    if (!CheckIfTileIsValid(previousDownwardsExpansionCells[k], expantionLevel.gridCellMapData))
                                    {
                                        removalList.Add(previousDownwardsExpansionCells[k]);
                                    }
                                }
                                for (int k = 0; k < removalList.Count; k++)
                                {
                                    previousDownwardsExpansionCells.Remove(removalList[k]);
                                }
                                AddChamberToLevel(ref newChamber, ref expantionLevel, expansionGridCellMap, previousDownwardsExpansionCells);
                                if (mirror)
                                {
                                    AddMirorredChamberToLevel(ref mirrorChamber, ref expantionLevel, expansionGridCellMap, previousDownwardsExpansionCells);
                                }
                            }
                        }
                    }
                }
            }
            else if(customChamber.procedural)
            {
                if(customChamber.levelHeight > 1)
                {
                    for (int i = 0; i < customChamber.levelHeight-1; i++)
                    {
                        int currentLevel = customChamber.startingLevel - (i+1);
                        Debug.Log("Currenty Level = " + currentLevel);
                        dungeonLevel = dungeonLayer.layerLevels[currentLevel];
                        gridCellMap = dungeonLevel.gridCellMapData.gridCellMap;
                        List<Vector2Int> newCells = new List<Vector2Int>(cells);
                        AddChamberToLevel(ref newChamber, ref dungeonLevel, gridCellMap, newCells);
                    }

                }
            }


            return newChamber;
        }

        public static void AddInitalRoomShapeToCells(ref List<Vector2Int> cells, XorshiftPlus randomGen, Vector2Int center, float rectChance, Vector2Int rectWidthRange, Vector2Int rectLengthRange, Vector2Int circleRadRange, ref List<Vector2Int> checkCells)
        {
            float random = randomGen.Next();
            if (random < rectChance)
            {
                int rectangleLength = RandomIntInRange(rectLengthRange, randomGen);
                int rectangleWidth = RandomIntInRange(rectWidthRange, randomGen);
                cells = CreateGridRectangle(center, new Vector2Int(rectangleWidth, rectangleLength));
                if(checkCells != null)
                {
                    checkCells = CreateGridRectangle(center, new Vector2Int(rectangleWidth + 2, rectangleLength + 2));
                }
            }
            else
            {
                int circleRadius = RandomIntInRange(circleRadRange, randomGen);
                cells = CreateGridCircle(center, circleRadius);
                if (checkCells != null)
                {
                    checkCells = CreateGridCircle(center, circleRadius + 2);
                }
            }
            
        }
        public static DungeonChamberStory AddChamberToLevel(ref DungeonChamber parentChamber, ref DungeonLevel dungeonLevel, GridCell[,] gridCellMap, List<Vector2Int> cells, bool connectToNetwork = true)
        {
            DungeonChamberStory newStory = new DungeonChamberStory();
            GetPositionListBounds(cells, ref newStory.bottomLeft, ref newStory.topRight);
            newStory.storyCells = cells;
            newStory.level = dungeonLevel.levelNumber;
            newStory.chamberCenter = GetAdveragePosition(cells);
            newStory.parentChamber = parentChamber;
            parentChamber.chamberStories.Add(newStory);

            for (int i = 0; i < cells.Count; i++)
            {
                GridCell cell = gridCellMap[cells[i].x, cells[i].y];
                cell.cellStatus = CellStatus.Room;
                cell.parentStory = newStory;
            }

            if (connectToNetwork == true)
            {
                if (dungeonLevel.chambersOnLevelDictionary.ContainsKey(newStory.chamberCenter))
                {
                    return null;
                }
                dungeonLevel.chambersOnLevelDictionary.Add(newStory.chamberCenter, newStory);
                dungeonLevel.chamberCenters.Add(newStory.chamberCenter);
            }

            dungeonLevel.roomsOnLevel.Add(newStory);
            return newStory;
        }

        public static DungeonChamberStory AddMirorredChamberToLevel(ref DungeonChamber parentChamber, ref DungeonLevel dungeonLevel, GridCell[,] gridCellMap, List<Vector2Int> cells, bool connectToNetwork = true)
        {
            DungeonChamberStory newStory = new DungeonChamberStory();
            List<Vector2Int> mirroredCells = new List<Vector2Int>();
            foreach (Vector2Int cell in cells)
            {
                int x = gridCellMap.GetLength(0) - cell.x -1;
                mirroredCells.Add(new Vector2Int(x, cell.y));
            }
            GetPositionListBounds(mirroredCells, ref newStory.bottomLeft, ref newStory.topRight);
            newStory.storyCells = mirroredCells;
            newStory.level = dungeonLevel.levelNumber;
            newStory.chamberCenter = GetAdveragePosition(mirroredCells);
            newStory.parentChamber = parentChamber;
            parentChamber.chamberStories.Add(newStory);

            for (int i = 0; i < mirroredCells.Count; i++)
            {
                GridCell cell = gridCellMap[mirroredCells[i].x, mirroredCells[i].y];
                cell.cellStatus = CellStatus.Room;
                cell.parentStory = newStory;
            }

            if (connectToNetwork == true)
            {
                if (dungeonLevel.chambersOnLevelDictionary.ContainsKey(newStory.chamberCenter))
                {
                    return null;
                }
                dungeonLevel.chambersOnLevelDictionary.Add(newStory.chamberCenter, newStory);
                dungeonLevel.chamberCenters.Add(newStory.chamberCenter);
            }

            dungeonLevel.roomsOnLevel.Add(newStory);
            return newStory;
        }
            

        public static void GetChamberFormationData(ChamberFormationProfile chamberFormationProfile, out Vector2Int shapeIterationRange, out Vector2Int circleRadiusRange, out Vector2Int rectangleLengthRange, out Vector2Int rectangleWidthRange, out Vector2Int chamberLevelsRange, out float rectangleChance, float gridSize)
        {
            Vector2Int gridCircleRaduisRange = new Vector2Int(Mathf.RoundToInt(chamberFormationProfile.circleRadisRange.x / gridSize), Mathf.RoundToInt(chamberFormationProfile.circleRadisRange.y / gridSize));
            Vector2Int gridRectangleLengthRange = new Vector2Int(Mathf.RoundToInt(chamberFormationProfile.rectangleLengthRange.x / gridSize), Mathf.RoundToInt(chamberFormationProfile.rectangleLengthRange.y / gridSize));
            Vector2Int gridRectangleWidthRange = new Vector2Int(Mathf.RoundToInt(chamberFormationProfile.rectangleWidthRange.x / gridSize), Mathf.RoundToInt(chamberFormationProfile.rectangleWidthRange.y / gridSize));
            shapeIterationRange = chamberFormationProfile.shapeIterationRange;
            circleRadiusRange = gridCircleRaduisRange;
            rectangleLengthRange = gridRectangleLengthRange;
            rectangleWidthRange = gridRectangleWidthRange;
            chamberLevelsRange = chamberFormationProfile.chamberLevelRange;
            rectangleChance = chamberFormationProfile.rectangleChance;
        }
        public static DungeonChamber CreateNewChamberBetweenLayers(DungeonLayer dungeonLayer, DungeonLevel higherLevel, DungeonLevel lowerLevel, int chamberId, XorshiftPlus randomGen)
        {
            DungeonChamber newChamber = new DungeonChamber();
            //this is currently hardcoded, intend to randomize this in the future
            newChamber.chamberArchType = MultiLevelChamberArchType.ConstrainedFloor;
            newChamber.chamberType = MultiLevelChamberType.FullLevel;
            newChamber.chamberID = chamberId;
            DungeonLevel dungeonLevel = higherLevel;
            GridCell[,] gridCellMap = dungeonLevel.gridCellMapData.gridCellMap;
            ChamberFormationProfile chamberFormationProfile = null;
            chamberFormationProfile = dungeonLayer.defaultChamberFormationProfile;


            //how many shapes will we add to the inital one?
            Vector2Int shapeIterationRange;

            Vector2Int circleRadisRange;

            Vector2Int rectangleLengthRange;
            Vector2Int rectangleWidthRange;
            Vector2Int chamberLevelRange;
            float rectangleChance;

            GetChamberFormationData(chamberFormationProfile, out shapeIterationRange, out circleRadisRange, out rectangleLengthRange, out rectangleWidthRange, out chamberLevelRange, out rectangleChance, dungeonLayer.gridSize);

            List<Vector2Int> availableCells = higherLevel.availibleCells;
            //remaining odds are on a circle


            int placementAttempts = 0;
            bool acceptPlacement = false;
            List<Vector2Int> cells = new List<Vector2Int>();
            while (acceptPlacement == false)
            {
                if (placementAttempts >= 40)
                {
                    return null;
                }
                //get the startLocation
                int startCellId = randomGen.Next(0, availableCells.Count);
                Vector2Int startLocation = availableCells[startCellId];

                //clear any cells that may have existed before
                cells.Clear();
                List<Vector2Int> checkCells = new List<Vector2Int>();
                AddInitalRoomShapeToCells(ref cells, randomGen, startLocation, rectangleChance, rectangleWidthRange, rectangleLengthRange, circleRadisRange, ref checkCells);


                //check if this shape fits in the dungeon
                //set accept placement to true
                acceptPlacement = true;

                for (int i = 0; i < cells.Count; i++)
                {
                    //THIS COULD BE OPTIMIZED WITH BOUNDS CHECKS I THINK!!
                    if (!CheckIfTileIsValid(cells[i], dungeonLevel.gridCellMapData))
                    {
                        acceptPlacement = false;
                        break;
                    }
                }
                placementAttempts++;
            }
            cells = GetRoomShape(shapeIterationRange, rectangleChance, rectangleLengthRange, rectangleWidthRange, circleRadisRange, dungeonLevel.gridCellMapData, randomGen, cells, false);



            AddChamberToLevel(ref newChamber, ref higherLevel, gridCellMap, cells, false);

            AddChamberToLevel(ref newChamber, ref lowerLevel, lowerLevel.gridCellMapData.gridCellMap, cells, false);

            return newChamber;
        }

        public static List<CellInfo[,]> ConvertDungeonChamber(DungeonChamber dungeonChamber, DungeonLayer dungeonLayer, List<DungeonLevel> levelMaps, out List<List<BorderCell>> exitLists, out float yLevel)
        {
            dungeonChamber.chamberStories = dungeonChamber.chamberStories.OrderBy(n => n.level).ToList();
            dungeonChamber.chamberStories.Reverse();
            levelMaps = levelMaps.OrderBy(n => n.yLevel).ToList();
            levelMaps.Reverse();
            yLevel = levelMaps[dungeonChamber.chamberStories[0].level].yLevel;
            List<CellInfo[,]> cellTables = new List<CellInfo[,]>();

            GridCell[,] gridMap = null;
            GridCellMapData gridCellMapData = null;
            exitLists = new List<List<BorderCell>>();
            float gridSize = dungeonLayer.gridSize;
            //OKAY WTF WHY IS THE LOWEST POINT THE BOUNDS AND THE BOUNDS THE LOWEST POINT
            Vector2Int lowestPoint = dungeonChamber.bounds;
            Vector2Int highestPoint = dungeonChamber.highestPoint;
            Vector2Int chamberBounds = dungeonChamber.lowestPoint;
            GetChamberDimensions(dungeonChamber, out chamberBounds, out lowestPoint, out highestPoint);
            for (int i = 0; i < dungeonChamber.chamberStories.Count; i++)
            {
                DungeonChamberStory chamberStory = dungeonChamber.chamberStories[i];
                gridCellMapData = levelMaps[chamberStory.level].gridCellMapData;
                gridMap = gridCellMapData.gridCellMap;
                CellInfo[,] cellTable = new CellInfo[chamberBounds.x, chamberBounds.y];
                for (int k = 0; k < chamberStory.storyCells.Count; k++)
                {
                    Vector2Int localTablePos = chamberStory.storyCells[k] - lowestPoint;
                    GridCell gridCell = gridMap[chamberStory.storyCells[k].x, chamberStory.storyCells[k].y];
                    CellInfo cellInfo = new CellInfo();
                    cellInfo.cellTablePos = localTablePos;
                    cellInfo.cellCenter = gridCell.position;
                    cellInfo.cellRoomData = null;
                    cellTable[localTablePos.x, localTablePos.y] = cellInfo;


                }

                for (int k = 0; k < cellTable.GetLength(0); k++)
                {
                    for (int l = 0; l < cellTable.GetLength(1); l++)
                    {
                        if (cellTable[k,l] == null)
                        {
                            CellInfo cellInfo = new CellInfo();


                            cellInfo.forbidden = true;
                            cellTable[k, l] = cellInfo;

                        }
                    }
                }
                exitLists.Add(chamberStory.exitCellsLocal);
                cellTables.Add(cellTable);
            }
            
            return cellTables;
        }

        public static List<CellInfo[,]> ConvertDungeonHallway(Hallway dungeonHallway, DungeonLevel dungeonLevel, out List<List<BorderCell>> exitLists, out float yLevel, float gridSize)
        {
            yLevel = dungeonLevel.yLevel;
            List<CellInfo[,]> cellTables = new List<CellInfo[,]>();

            GridCell[,] gridMap = null;
            GridCellMapData gridCellMapData = null;
            exitLists = new List<List<BorderCell>>();
            Vector2Int lowestPoint;
            Vector2Int highestPoint;
            Vector2Int chamberBounds;
            GetHallwayDimensions(dungeonHallway, out chamberBounds, out lowestPoint, out highestPoint);

            gridCellMapData = dungeonLevel.gridCellMapData;
            gridMap = gridCellMapData.gridCellMap;
            CellInfo[,] cellTable = new CellInfo[chamberBounds.x, chamberBounds.y];

            foreach (Vector2Int globalTablePos in dungeonHallway.hallwayCells)
            {
                Vector2Int localTablePos = globalTablePos - lowestPoint;
                GridCell gridCell = gridMap[globalTablePos.x, globalTablePos.y];
                CellInfo cellInfo = new CellInfo();
                cellInfo.cellTablePos = localTablePos;
                cellInfo.cellCenter = gridCell.position;
                cellInfo.cellRoomData = null;
                cellTable[localTablePos.x, localTablePos.y] = cellInfo;
            }


            for (int k = 0; k < cellTable.GetLength(0); k++)
            {
                for (int l = 0; l < cellTable.GetLength(1); l++)
                {
                    if (cellTable[k, l] == null)
                    {
                        CellInfo cellInfo = new CellInfo();


                        cellInfo.forbidden = true;
                        cellTable[k, l] = cellInfo;

                    }
                }
            }
            List<BorderCell> exitCellsLocal = new List<BorderCell>();
            foreach (BorderCell exitCell in dungeonHallway.exitCells)
            {
                exitCellsLocal.Add(new BorderCell(exitCell.cellPos - lowestPoint, exitCell.gridDirection));
            }
            exitLists.Add(exitCellsLocal);
            cellTables.Add(cellTable);

            return cellTables;
        }


        public static void SetChamberExits(ref DungeonChamberStory chamberStory, DungeonLayer dungeonLayer, DungeonLevel dungeonLevel, XorshiftPlus random)
        {
            DungeonChamber parentChamber = chamberStory.parentChamber;
            
            GridCellMapData gridCellMapData = dungeonLevel.gridCellMapData;
            
            
            GridCell[,] gridMap = gridCellMapData.gridCellMap;

            List<Vector2Int> storyCells = chamberStory.storyCells;

            Vector2Int chamberBounds;
            Vector2Int lowestPoint;
            Vector2Int highestPoint;
            GetChamberDimensions(parentChamber, out chamberBounds, out lowestPoint, out highestPoint);


            chamberStory.borderCells = GetBorderCells(storyCells, gridMap, gridCellMapData, lowestPoint, parentChamber);
            chamberStory.edgeCells = GetEdgeCells(storyCells, gridMap, gridCellMapData, lowestPoint, parentChamber);
            if (parentChamber.allowedFloors.Count-1 >= chamberStory.level)
            {
                if (!parentChamber.allowedFloors[chamberStory.level])
                {
                    chamberStory.connectToNetwork = false;
                    return;
                }
            }
            List<BorderCell> exitCellsLocal = new List<BorderCell>();
            List<BorderCell> exitCellsGlobal = new List<BorderCell>();
            switch (parentChamber.chamberConnectionType)
            {
                case ChamberConnectionType.Open:
                    GetOpenExitCells(out exitCellsLocal, out exitCellsGlobal, chamberStory.borderCells, lowestPoint);
                    break;
                case ChamberConnectionType.None:
                    GetClosedExitCells(out exitCellsLocal, out exitCellsGlobal);
                    break;
                case ChamberConnectionType.SingleDoor:
                    GetSingleDoorExitCells(out exitCellsLocal, out exitCellsGlobal, chamberStory.borderCells, lowestPoint, random);
                    break;
                case ChamberConnectionType.DefinedEntrance:
                    GetDefinedEntranceExitCells(parentChamber, chamberStory, out exitCellsLocal, out exitCellsGlobal, lowestPoint, highestPoint);
                    break;
            }
            chamberStory.exitCellsLocal = exitCellsLocal;
            chamberStory.exitCellsGlobal = exitCellsGlobal;
        }
        public static void GetOpenExitCells(out List<BorderCell> localExitCells, out List<BorderCell> globalExitCells, List<BorderCell> borderCells, Vector2Int lowestPoint)
        {
            localExitCells = new List<BorderCell>(borderCells);
            globalExitCells = new List<BorderCell>();
            for (int i = 0; i < borderCells.Count; i++)
            {
                globalExitCells.Add(new BorderCell(borderCells[i].cellPos + lowestPoint, borderCells[i].gridDirection));
            }

        }
        public static void GetClosedExitCells(out List<BorderCell> localExitCells, out List<BorderCell> globalExitCells)
        {
            localExitCells = new List<BorderCell>();
            globalExitCells = new List<BorderCell>();
        }
        public static void GetSingleDoorExitCells(out List<BorderCell> localExitCells, out List<BorderCell> globalExitCells, List<BorderCell> borderCells, Vector2Int lowestPoint, XorshiftPlus random)
        {
            localExitCells = new List<BorderCell>();
            globalExitCells = new List<BorderCell>();
            BorderCell exitCell = borderCells[random.Next(0, borderCells.Count - 1)];

            Vector2Int localPos = exitCell.cellPos;
            Vector2Int globalPos = localPos + lowestPoint;
            localExitCells.Add(new BorderCell(localPos, exitCell.gridDirection));
            globalExitCells.Add(new BorderCell(globalPos, exitCell.gridDirection));
        }

        public static void GetDefinedEntranceExitCells(DungeonChamber parentChamber, DungeonChamberStory chamberStory, out List<BorderCell> localExitCells, out List<BorderCell> globalExitCells, Vector2Int lowestPoint, Vector2Int highestPoint)
        {
            localExitCells = new List<BorderCell>();
            globalExitCells = new List<BorderCell>();
            int x = 1;
            int z = 1;
            Vector2Int exitPos = Vector2Int.zero;
            if(parentChamber.definedEnteranceType == DefinedEnteranceType.ZAxisCenterPlus)
            {
                x = chamberStory.chamberCenter.x;
                z = highestPoint.y - 1;
                exitPos = new Vector2Int(x, z);
                localExitCells.Add(new BorderCell(exitPos- lowestPoint, GridDirection.Up));
                globalExitCells.Add(new BorderCell(exitPos, GridDirection.Up));
            }
            else if(parentChamber.definedEnteranceType == DefinedEnteranceType.ZAxisCenterMinus)
            {
                x = chamberStory.chamberCenter.x;
                z = lowestPoint.y;
                exitPos = new Vector2Int(x, z);
                localExitCells.Add(new BorderCell(exitPos - lowestPoint, GridDirection.Down));
                globalExitCells.Add(new BorderCell(exitPos, GridDirection.Down));
            }
            else if(parentChamber.definedEnteranceType == DefinedEnteranceType.XAxisCenterPlus)
            {
                x = highestPoint.x - 1;
                z = chamberStory.chamberCenter.y;
                exitPos = new Vector2Int(x, z);
                localExitCells.Add(new BorderCell(exitPos - lowestPoint, GridDirection.Right));
                globalExitCells.Add(new BorderCell(exitPos, GridDirection.Right));
            }
            else if(parentChamber.definedEnteranceType == DefinedEnteranceType.XAxisCenterMinus)
            {
                x = lowestPoint.x;
                z = chamberStory.chamberCenter.y;
                exitPos = new Vector2Int(x, z);
                localExitCells.Add(new BorderCell(exitPos - lowestPoint, GridDirection.Left));
                globalExitCells.Add(new BorderCell(exitPos, GridDirection.Left));
            }
            else if(parentChamber.definedEnteranceType == DefinedEnteranceType.All)
            {
                int x1 = chamberStory.chamberCenter.x;
                int z1 = highestPoint.y - 1;
                Vector2Int exitPos1 = new Vector2Int(x1, z1);
                localExitCells.Add(new BorderCell(exitPos1 - lowestPoint, GridDirection.Up));
                globalExitCells.Add(new BorderCell(exitPos1, GridDirection.Up));
                int x2 = chamberStory.chamberCenter.x;
                int z2 = lowestPoint.y;
                Vector2Int exitPos2 = new Vector2Int(x2, z2);
                localExitCells.Add(new BorderCell(exitPos2 - lowestPoint, GridDirection.Down));
                globalExitCells.Add(new BorderCell(exitPos2, GridDirection.Down));
                int x3 = highestPoint.x - 1;
                int z3 = chamberStory.chamberCenter.y;
                Vector2Int exitPos3 = new Vector2Int(x3, z3);
                localExitCells.Add(new BorderCell(exitPos3 - lowestPoint, GridDirection.Right));
                globalExitCells.Add(new BorderCell(exitPos3, GridDirection.Right));
                int x4 = lowestPoint.x;
                int z4 = chamberStory.chamberCenter.y;
                Vector2Int exitPos4 = new Vector2Int(x4, z4);
                localExitCells.Add(new BorderCell(exitPos4 - lowestPoint, GridDirection.Left));
                globalExitCells.Add(new BorderCell(exitPos4, GridDirection.Left));
            }

        }

        public static List<BorderCell> GetBorderCells(List<Vector2Int> storyCells, GridCell[,] gridMap, GridCellMapData gridCellMapData, Vector2Int lowestPoint, DungeonChamber parentChamber)
        {
            List<BorderCell> borderCells = new List<BorderCell>();
            for (int i = 0; i < storyCells.Count; i++)
            {
                GridCell gridCell = gridMap[storyCells[i].x, storyCells[i].y];
                //not sure about the use of the local table pos yet, must investigate
                Vector2Int localTablePos = storyCells[i] - lowestPoint;
                Vector2Int north = new Vector2Int(0, 1) + storyCells[i];
                Vector2Int south = new Vector2Int(0, -1) + storyCells[i];
                Vector2Int east = new Vector2Int(1, 0) + storyCells[i];
                Vector2Int west = new Vector2Int(-1, 0) + storyCells[i];


                if (CheckIfTileIsValid(north, gridCellMapData, true))
                {
                    GridCell northCell = gridMap[north.x, north.y];
                    if (northCell.cellStatus == CellStatus.Hallway || (northCell.cellStatus == CellStatus.Room && northCell.parentStory != gridCell.parentStory))
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Up);
                        borderCells.Add(hallwayBorder);
                    }
                }
                if (CheckIfTileIsValid(south, gridCellMapData, true))
                {
                    GridCell southCell = gridMap[south.x, south.y];
                    if (southCell.cellStatus == CellStatus.Hallway || (southCell.cellStatus == CellStatus.Room && southCell.parentStory != gridCell.parentStory))
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Down);
                        borderCells.Add(hallwayBorder);
                    }
                }
                if (CheckIfTileIsValid(east, gridCellMapData, true))
                {
                    GridCell eastCell = gridMap[east.x, east.y];
                    if (eastCell.cellStatus == CellStatus.Hallway || (eastCell.cellStatus == CellStatus.Room && eastCell.parentStory != gridCell.parentStory))
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Right);
                        borderCells.Add(hallwayBorder);
                    }
                }
                if (CheckIfTileIsValid(west, gridCellMapData, true))
                {
                    GridCell westCell = gridMap[west.x, west.y];
                    if (westCell.cellStatus == CellStatus.Hallway || (westCell.cellStatus == CellStatus.Room && westCell.parentStory != gridCell.parentStory))
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Left);
                        borderCells.Add(hallwayBorder);
                    }
                }
            }
            return borderCells;
        }
        public static List<BorderCell> GetEdgeCells(List<Vector2Int> storyCells, GridCell[,] gridMap, GridCellMapData gridCellMapData, Vector2Int lowestPoint, DungeonChamber parentChamber)
        {
            List<BorderCell> borderCells = new List<BorderCell>();
            for (int i = 0; i < storyCells.Count; i++)
            {
                GridCell gridCell = gridMap[storyCells[i].x, storyCells[i].y];
                //not sure about the use of the local table pos yet, must investigate
                Vector2Int localTablePos = storyCells[i] - lowestPoint;
                Vector2Int north = new Vector2Int(0, 1) + storyCells[i];
                Vector2Int south = new Vector2Int(0, -1) + storyCells[i];
                Vector2Int east = new Vector2Int(1, 0) + storyCells[i];
                Vector2Int west = new Vector2Int(-1, 0) + storyCells[i];


                if (CheckIfTileIsValid(north, gridCellMapData, true))
                {
                    GridCell northCell = gridMap[north.x, north.y];
                    if (northCell.parentStory != gridCell.parentStory)
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Up);
                        borderCells.Add(hallwayBorder);
                    }
                }
                if (CheckIfTileIsValid(south, gridCellMapData, true))
                {
                    GridCell southCell = gridMap[south.x, south.y];
                    if (southCell.parentStory != gridCell.parentStory)
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Down);
                        borderCells.Add(hallwayBorder);
                    }
                }
                if (CheckIfTileIsValid(east, gridCellMapData, true))
                {
                    GridCell eastCell = gridMap[east.x, east.y];
                    if (eastCell.parentStory != gridCell.parentStory)
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Right);
                        borderCells.Add(hallwayBorder);
                    }
                }
                if (CheckIfTileIsValid(west, gridCellMapData, true))
                {
                    GridCell westCell = gridMap[west.x, west.y];
                    if (westCell.parentStory != gridCell.parentStory)
                    {
                        BorderCell hallwayBorder = new BorderCell(localTablePos, GridDirection.Left);
                        borderCells.Add(hallwayBorder);
                    }
                }
            }
            return borderCells;
        }

        public static void GetChamberDimensions(DungeonChamber chamber, out Vector2Int chamberBounds, out Vector2Int lowestPoint, out Vector2Int highestPoint)
        {
            lowestPoint = chamber.chamberStories[0].bottomLeft;
            highestPoint = chamber.chamberStories[0].topRight;
            for (int i = 0; i < chamber.chamberStories.Count; i++)
            {
                DungeonChamberStory chamberStory = chamber.chamberStories[i];
                Vector2Int bottomLeft = chamberStory.bottomLeft;
                Vector2Int topRight = chamberStory.topRight;
                if (bottomLeft.x < lowestPoint.x)
                {
                    lowestPoint.x = bottomLeft.x;
                }
                if (bottomLeft.y < lowestPoint.y)
                {
                    lowestPoint.y = bottomLeft.y;
                }

                if (topRight.x > highestPoint.x)
                {
                    highestPoint.x = topRight.x;
                }
                if (topRight.y > highestPoint.y)
                {
                    highestPoint.y = topRight.y;
                }
            }
            highestPoint += Vector2Int.one;
            chamberBounds = highestPoint - lowestPoint;
            chamber.bounds = chamberBounds;
            chamber.highestPoint = highestPoint;
            chamber.lowestPoint = lowestPoint;
        }


        public static void GetHallwayDimensions(Hallway hallway, out Vector2Int chamberBounds, out Vector2Int lowestPoint, out Vector2Int highestPoint)
        {
            lowestPoint = new Vector2Int(100000, 100000);
            highestPoint = new Vector2Int(-100000, -100000);
            foreach (Vector2Int cellPos in hallway.hallwayCells)
            {
                if (cellPos.x < lowestPoint.x)
                {
                    lowestPoint.x = cellPos.x;
                }
                if (cellPos.y < lowestPoint.y)
                {
                    lowestPoint.y = cellPos.y;
                }

                if (cellPos.x > highestPoint.x)
                {
                    highestPoint.x = cellPos.x;
                }
                if (cellPos.y > highestPoint.y)
                {
                    highestPoint.y = cellPos.y;
                }
            }
            highestPoint += Vector2Int.one;
            chamberBounds = highestPoint - lowestPoint;
        }

        #region A* PathFinding
        public static List<PathNode> FindShortestPath(GridCell[,] gridTable, Vector2Int startPos, Vector2Int endPos)
        {
            DungeonChamberStory startPosStorey = gridTable[startPos.x, startPos.y].parentStory;
            DungeonChamberStory endPosStorey = gridTable[endPos.x, endPos.y].parentStory;

            PathNode startNode = new PathNode();
            PathNode endNode = new PathNode();
            int gridLength = gridTable.GetLength(1);
            int gridWidth = gridTable.GetLength(0);
            List<PathNode> openList;
            List<PathNode> closedList = new List<PathNode>();
            PathNode[,] nodeGrid = new PathNode[gridWidth, gridLength];
            openList = new List<PathNode>();
            for (int i = 0; i < gridTable.GetLength(0); i++)
            {
                for (int k = 0; k < gridTable.GetLength(1); k++)
                {
                    PathNode pathNode = new PathNode();
                    pathNode.position = new Vector2Int(i,k);
                    pathNode.gCost = int.MaxValue;
                    pathNode.gridCell = gridTable[i, k];
                    DungeonChamberStory parentChamberStory = pathNode.gridCell.parentStory;
                    if (pathNode.gridCell.cellStatus != CellStatus.Room || startPosStorey == parentChamberStory || endPosStorey == parentChamberStory)
                    {
                        pathNode.isWalkable = true;
                    }
                    else
                    {
                        pathNode.isWalkable = false;
                    }
                    if (parentChamberStory != null)
                    {
                        /*
                        if (parentChamberStory.parentChamber.hallsAvoidChamber == false)
                        {
                            pathNode.isWalkable = true;
                        }    
                        */
                    }

                    pathNode.CalculateFCost();
                    pathNode.cameFromNode = null;
                    nodeGrid[i, k] = pathNode;
                    if (pathNode.position == endPos)
                    {
                        pathNode.isWalkable = true;
                        endNode = pathNode;

                    }
                    if (pathNode.position == startPos)
                    {
                        pathNode.isWalkable = true;
                        startNode = pathNode;
                        openList.Add(startNode);

                    }

                }
            }
            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            startNode.CalculateFCost();

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNode neighborNode in GetNeighbourList(currentNode, nodeGrid, gridLength, gridWidth))
                {
                    if (closedList.Contains(neighborNode))
                    {
                        continue;
                    }
                    if (!neighborNode.isWalkable)
                    {
                        closedList.Add(neighborNode);
                        continue;
                    }

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighborNode);
                    if (tentativeGCost < neighborNode.gCost)
                    {
                        neighborNode.cameFromNode = currentNode;
                        neighborNode.gCost = tentativeGCost;
                        neighborNode.hCost = CalculateDistanceCost(neighborNode, endNode);
                        neighborNode.CalculateFCost();

                        if (!openList.Contains(neighborNode))
                        {
                            openList.Add(neighborNode);
                        }
                    }

                }
            }

            //out of nodes on the openlist, we have searched everything
            Debug.Log("Failed To Create Path");
            return null;
        }
        private static List<PathNode> GetNeighbourList(PathNode currentNode, PathNode[,] nodeGrid, int gridLength, int gridWidth)
        {

            List<PathNode> neighborList = new List<PathNode>();
            int currentNodeX = currentNode.position.x;
            int currentNodeY = currentNode.position.y;

            PathNode pathNode1 = GetNode(currentNodeX - 1, currentNodeY, nodeGrid, gridLength, gridWidth);
            if (pathNode1 != null)
            {
                neighborList.Add(pathNode1);
            }


            PathNode pathNode2 = GetNode(currentNodeX, currentNodeY + 1, nodeGrid, gridLength, gridWidth);
            if (pathNode2 != null)
            {
                neighborList.Add(pathNode2);
            }


            PathNode pathNode3 = GetNode(currentNodeX + 1, currentNodeY, nodeGrid, gridLength, gridWidth);
            if (pathNode3 != null)
            {
                neighborList.Add(pathNode3);
            }


            PathNode pathNode4 = GetNode(currentNodeX, currentNodeY - 1, nodeGrid, gridLength, gridWidth);
            if (pathNode4 != null)
            {
                neighborList.Add(pathNode4);
            }

            return neighborList;
        }
        private static PathNode GetNode(int nodeX, int nodeZ, PathNode[,] nodeGrid, int gridLength, int gridWidth)
        {
            PathNode pathNode = null;
            if (nodeX < gridWidth && nodeX >= 0 && nodeZ < gridLength && nodeZ >= 0)
            {
                pathNode = nodeGrid[nodeX, nodeZ];
            }
            return pathNode;
        }

        private static List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);
            PathNode currentNode = endNode;
            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }

        private static int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.position.x - b.position.x);
            int yDistance = Mathf.Abs(a.position.y - b.position.y);
            int remaining = Mathf.Abs(xDistance - yDistance);
            return 14 * Mathf.Min(xDistance, yDistance) + 10 * remaining;

        }
        private static PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFcostNode = pathNodeList[0];
            for (int i = 0; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].fCost < lowestFcostNode.fCost)
                {
                    lowestFcostNode = pathNodeList[i];
                }

            }
            return lowestFcostNode;
        }
        #endregion

        public static List<Vector2Int> GetRoomShape(Vector2Int shapeIterationRange, float rectangleChance, Vector2Int rectangleLengthRange, Vector2Int rectangleWidthRange, Vector2Int circleRadisRange, GridCellMapData gridCellMapData, XorshiftPlus randomGen, List<Vector2Int> originCells = null, bool mirror = false)
        {
            if(originCells == null)
            {
                Debug.LogError("null cells list is currently not permited");
            }
            else
            {
                int shapeIterations = RandomIntInRange(shapeIterationRange, randomGen);
                for (int i = 0; i < shapeIterations; i++)
                {
                    float shapeChance = randomGen.Next();
                    //might want to change the start location to be around the edge of the previous shapes, its currently anywhere inside
                    List<Vector2Int> newShapeCells = new List<Vector2Int>();
                    Vector2Int startCell = originCells[randomGen.Next(0, originCells.Count-1)];
                    if (shapeChance < rectangleChance)
                    {
                        int rectangleLength = RandomIntInRange(rectangleLengthRange, randomGen);
                        int rectangleWidth = RandomIntInRange(rectangleWidthRange, randomGen);
                        newShapeCells = CreateGridRectangle(startCell, new Vector2Int(rectangleWidth, rectangleLength));
                    }
                    else
                    {
                        int circleRadius = RandomIntInRange(circleRadisRange, randomGen);
                        newShapeCells = CreateGridCircle(startCell, circleRadius);

                    }
                    //check if any of the cells are outside the bounds of the dungeon and if they are colliding with any other rooms 

                    for (int k = 0; k < newShapeCells.Count; k++)
                    {
                        if (originCells.Contains(newShapeCells[k]) == false)
                        {
                            if (CheckIfTileIsValid(newShapeCells[k], gridCellMapData, false, mirror))
                            {
                                originCells.Add(newShapeCells[k]);
                            }

                        }

                    }
                }
                
            }
            return originCells;


        }

        public static int RandomIntInRange(Vector2Int range, XorshiftPlus randomGen)
        {
            
            return randomGen.Next(range.x, range.y);
        }

        public static bool CheckIfTileIsValid(Vector2Int tile, GridCellMapData gridCellMapData, bool onlyFilterForbidden = false, bool halfX = false)
        {
            float gridSize = gridCellMapData.gridSize;
            GridCell[,] gridCellMap = gridCellMapData.gridCellMap;
            Vector2 gridTopRight = gridCellMapData.origin + gridCellMapData.bounds;
            Vector2 tileWorldPos = gridCellMapData.origin + new Vector2(tile.x * gridSize, tile.y * gridSize);
            
            if (tileWorldPos.x < gridCellMapData.origin.x || tileWorldPos.y < gridCellMapData.origin.y || tileWorldPos.x > gridTopRight.x || tileWorldPos.y > gridTopRight.y) 
            {
                return false;
            }
            int xLength = gridCellMap.GetLength(0);
            if (halfX)
            {
                xLength = xLength / 2;
            }
            if ((tile.x < 0 || tile.y < 0) || (tile.x >= xLength || tile.y >= gridCellMap.GetLength(1)))
            { 
                return false;
            }
            if (onlyFilterForbidden)
            {
                if (gridCellMap[tile.x, tile.y].cellStatus == CellStatus.Forbidden)
                {
                    return false;
                }
            }
            else
            {
                if (gridCellMap[tile.x, tile.y].cellStatus != CellStatus.Free)
                {
                    return false;
                }
            }


            return true;
        }

        public static bool CheckIfTileIsTaken(Vector2Int tile, GridCellMapData gridCellMapData, bool checkHalls)
        {
            float gridSize = gridCellMapData.gridSize;
            GridCell[,] gridCellMap = gridCellMapData.gridCellMap;
            Vector2 gridTopRight = gridCellMapData.origin + gridCellMapData.bounds;
            Vector2 tileWorldPos = gridCellMapData.origin + new Vector2(tile.x * gridSize, tile.y * gridSize);
            if (tileWorldPos.x < gridCellMapData.origin.x || tileWorldPos.y < gridCellMapData.origin.y || tileWorldPos.x > gridTopRight.x || tileWorldPos.y > gridTopRight.y)
            {
                return false;
            }
            if ((tile.x < 0 || tile.y < 0) || (tile.x >= gridCellMap.GetLength(0) || tile.y >= gridCellMap.GetLength(1)))
            {

                return false;
            }
            if (gridCellMap[tile.x, tile.y].cellStatus == CellStatus.Room)
            {
                return true;
            }
            if(checkHalls == true)
            {
                if (gridCellMap[tile.x, tile.y].cellStatus == CellStatus.Hallway)
                {
                    return true;
                }
               
            }

            return false;
        }

        public static void GetPositionListBounds(List<Vector2Int> positionList, ref Vector2Int bottomLeft, ref Vector2Int topRight)
        {
            bottomLeft = new Vector2Int(100000, 100000);
            topRight = new Vector2Int(-100000, -100000);

            for (int i = 0; i < positionList.Count; i++)
            {
                int x = positionList[i].x;
                int y = positionList[i].y;
                if(x > topRight.x)
                {
                    topRight.x = x;
                }
                if(x < bottomLeft.x)
                {
                    bottomLeft.x = x;
                }
                if(y > topRight.y)
                {
                    topRight.y = y;
                }
                if(y < bottomLeft.y)
                {
                    bottomLeft.y = y;
                }
            }
        }

        public static Hallway CreateNewHallwayBetweenTwoPoints(Vector2Int point1, Vector2Int point2, int width, GridCellMapData gridCellMapData, bool useAStar, List<Hallway> spawnedHallways)
        {
            GridCell[,] gridCellMap = gridCellMapData.gridCellMap;

            List<Vector2Int> cells = new List<Vector2Int>();

            DungeonChamberStory startCellStorey = gridCellMap[point1.x, point1.y].parentStory;
            DungeonChamberStory endCellStorey = gridCellMap[point2.x, point2.y].parentStory;

            if(startCellStorey == null || endCellStorey == null)
            {
                Debug.LogWarning("Hallway Could not find parent storeys for start and end, returning null");
                return null;
            }
            if (useAStar == true)
            {
                List<PathNode> pathNodes = FindShortestPath(gridCellMap, point1, point2);
                if(pathNodes == null)
                {
                    Debug.LogError("no path between room");
                    return null;
                }
                for (int i = 0; i < pathNodes.Count; i++)
                {
                    Vector2Int cellPos = pathNodes[i].position;
                    cells.Add(cellPos);
                }
            }
            else
            {
                int xSteps = point2.x - point1.x;
                int ySteps = point2.y - point1.y;
                Vector2Int currentPoint = point1;
                cells.Add(currentPoint);
                for (int i = 0; i < Mathf.Abs(xSteps); i++)
                {
                    if (xSteps < 0)
                    {
                        currentPoint += new Vector2Int(-1, 0);
                    }
                    else
                    {
                        currentPoint += new Vector2Int(1, 0);
                    }
                    if (gridCellMap[currentPoint.x, currentPoint.y].cellStatus != CellStatus.Room)
                    {
                        gridCellMap[currentPoint.x, currentPoint.y].cellStatus = CellStatus.Hallway;
                    }

                    cells.Add(currentPoint);
                }
                for (int k = 0; k < Mathf.Abs(ySteps); k++)
                {
                    if (ySteps < 0)
                    {
                        currentPoint += new Vector2Int(0, -1);
                    }
                    else
                    {
                        currentPoint += new Vector2Int(0, 1);
                    }
                    if (gridCellMap[currentPoint.x, currentPoint.y].cellStatus != CellStatus.Room)
                    {
                        gridCellMap[currentPoint.x, currentPoint.y].cellStatus = CellStatus.Hallway;
                    }
                    cells.Add(currentPoint);
                }
            }

            int j = cells.Count - 1;
            BorderCell startCell = null;
            BorderCell endCell = null;

            List<Vector2Int> trimmedCells = new List<Vector2Int>();

            bool addLastCell = endCellStorey.parentChamber.hallsAddExtraCell;
            bool addFirstCell = startCellStorey.parentChamber.hallsAddExtraCell;
            bool startCellFound = false;
            for (int i = 0; i < cells.Count; i++)
            {
                DungeonChamberStory lastChamberStory = null;
                Vector2Int lastCellPos = cells[j];
                if (j != cells.Count - 1)
                {
                    lastChamberStory = gridCellMap[lastCellPos.x, lastCellPos.y].parentStory;
                }
                Vector2Int cellPos = cells[i];
                DungeonChamberStory currentChamberStory = gridCellMap[cellPos.x, cellPos.y].parentStory;
                if (lastChamberStory == startCellStorey && (currentChamberStory == null || currentChamberStory == endCellStorey))
                {

                    //the last cell should be the start cell
                    BorderCell startChamberBorderCell = new BorderCell(lastCellPos, GetPathExitCellDireciton(cellPos, lastCellPos, true));

                    startCellStorey.AddExitCell(startChamberBorderCell);

                    if (addFirstCell)
                    {
                        startCell = new BorderCell(lastCellPos, GetSpecialLastCellExitDirection(lastCellPos, gridCellMap));
                        trimmedCells.Add(lastCellPos);
                    }
                    else
                    {
                        startCell = new BorderCell(cellPos, GetPathExitCellDireciton(cellPos, lastCellPos, false));
                    }
                    startCellFound = true;
                }

                if((lastChamberStory == null || lastChamberStory == startCellStorey) && currentChamberStory == endCellStorey)
                {
                    //the current cell should be the end cell
                    BorderCell endChamberBorderCell = new BorderCell(cellPos, GetPathExitCellDireciton(cellPos, lastCellPos, false));
                    endCellStorey.AddExitCell(endChamberBorderCell);
                    if (addLastCell)
                    {
                        trimmedCells.Add(cellPos);
                        endCell = new BorderCell(cellPos, GetSpecialLastCellExitDirection(cellPos, gridCellMap));
                    }
                    else
                    {
                        endCell = new BorderCell(lastCellPos, GetPathExitCellDireciton(cellPos, lastCellPos, true));
                    }

                    break;
                }

                if (startCellFound)
                {
                    trimmedCells.Add(cellPos);
                }
                j = i;
            }

            if (startCell == null || endCell == null)
            {
                Debug.LogError("Hallway Creation Failed");
                return null;
            }

            List<Hallway> mergeableHallways = GetMergeableHallways(trimmedCells, gridCellMap);
            if (mergeableHallways.Count != 0)
            {
                Hallway parentHallway = mergeableHallways[0];
                for (int i = 1; i < mergeableHallways.Count; i++)
                {
                    Hallway hallwayToMerge = mergeableHallways[i];
                    if (spawnedHallways.Contains(hallwayToMerge))
                    {
                        spawnedHallways.Remove(hallwayToMerge);
                    }
                    parentHallway.exitCells.AddRange(hallwayToMerge.exitCells);
                    parentHallway.hallwayCells.AddRange(hallwayToMerge.hallwayCells);
                }

                parentHallway.exitCells.Add(startCell);
                parentHallway.exitCells.Add(endCell);
                parentHallway.hallwayCells.AddRange(trimmedCells);
                UpdateHallwayCellDatas(parentHallway.hallwayCells, parentHallway, gridCellMap);
                return null;
            }
            else
            {
                List<BorderCell> exitCells = new List<BorderCell>() { startCell, endCell };

                Hallway newHallway = new Hallway(trimmedCells, exitCells);
                UpdateHallwayCellDatas(trimmedCells, newHallway, gridCellMap);
                return newHallway;
            }

            /*
             * THIS IS OLD EXPANSION CODE
            if (width > 1)
            {
                //expansion is currently very expensive, i do not recommend doing this unless absolutly nessisary 
                ExpandPathListWidth(pathNodes, ref cells, gridCellMapData, width - 1);
            }
            */


        }

        public static void UpdateHallwayCellDatas(List<Vector2Int> cellPositions, Hallway hallway, GridCell[,] gridCellMap)
        {
            foreach (Vector2Int cellPos in cellPositions)
            {
                GridCell cell = gridCellMap[cellPos.x, cellPos.y];
                cell.cellStatus = CellStatus.Hallway;
                cell.parentHallway = hallway;
            }
        }

        private static List<Hallway> GetMergeableHallways(List<Vector2Int> cellPositions, GridCell[,] gridCellMap)
        {
            List<Hallway> mergeableHallways = new List<Hallway>();
            foreach (Vector2Int cellPos in cellPositions)
            {
                foreach (Hallway hallway in GetHallwaysNextToCell(cellPos, gridCellMap))
                {
                    if (!mergeableHallways.Contains(hallway))
                    {
                        mergeableHallways.Add(hallway);
                    }
                }
            }
            return mergeableHallways;
        }
        private static List<Hallway> GetHallwaysNextToCell(Vector2Int cellPos, GridCell[,] gridCellMap)
        {
            List<Hallway> results = new List<Hallway>();
            Vector2Int northPos = cellPos + new Vector2Int(0, 1);
            Vector2Int southPos = cellPos + new Vector2Int(0, -1);
            Vector2Int eastPos = cellPos + new Vector2Int(1, 0);
            Vector2Int westPos = cellPos + new Vector2Int(-1, 0);


            GridCell cell = gridCellMap[cellPos.x, cellPos.y];
            if (cell.parentHallway != null && !results.Contains(cell.parentHallway))
            {
                results.Add(cell.parentHallway);
            }


            if (IsCellWithinGrid(northPos, gridCellMap))
            {

                cell = gridCellMap[northPos.x, northPos.y];
                if (cell.parentHallway != null && !results.Contains(cell.parentHallway))
                {
                    results.Add(cell.parentHallway);
                }

            }
            if (IsCellWithinGrid(southPos, gridCellMap))
            {
                cell = gridCellMap[southPos.x, southPos.y];
                if (cell.parentHallway != null && !results.Contains(cell.parentHallway))
                {
                    results.Add(cell.parentHallway);
                }

            }
            if (IsCellWithinGrid(eastPos, gridCellMap))
            {
                cell = gridCellMap[eastPos.x, eastPos.y];
                if (cell.parentHallway != null && !results.Contains(cell.parentHallway))
                {
                    results.Add(cell.parentHallway);
                }

            }
            if (IsCellWithinGrid(westPos, gridCellMap))
            {
                cell = gridCellMap[westPos.x, westPos.y];
                if (cell.parentHallway != null && !results.Contains(cell.parentHallway))
                {
                    results.Add(cell.parentHallway);
                }

            }

            return results;
        }

        public static bool IsCellWithinGrid(Vector2Int cell, GridCell[,] gridCellMap)
        {
            if(cell.x < 0  || cell.y < 0 || cell.x >= gridCellMap.GetLength(0) || cell.y >= gridCellMap.GetLength(1))
            {
                return false;
            }
            return true;
        }

        private static GridDirection GetPathExitCellDireciton(Vector2Int currentCellPos, Vector2Int lastCellPos, bool reverse = true)
        {
            Vector2Int deltaCellPos = currentCellPos - lastCellPos;
            if(deltaCellPos.x > 0)
            {
                if (reverse)
                {
                    return GridDirection.Right;
                }
                else
                {
                    return GridDirection.Left;
                }
                
            }
            else if(deltaCellPos.x < 0)
            {
                if (reverse)
                {
                    return GridDirection.Left;
                }
                else
                {
                    return GridDirection.Right;
                }
            }
            else if(deltaCellPos.y > 0)
            {
                if (reverse)
                {
                    return GridDirection.Up;
                }
                else
                {
                    return GridDirection.Down;
                }
            }
            else if(deltaCellPos.y < 0)
            {
                if (reverse)
                {
                    return GridDirection.Down;
                }
                else
                {
                    return GridDirection.Up;
                }
            }
            else
            {
                return GridDirection.Down;
            }
        }

        private static GridDirection GetSpecialLastCellExitDirection(Vector2Int cellPos, GridCell[,] gridCellMap) 
        { 
            Vector2Int north = cellPos + new Vector2Int(0, 1);
            Vector2Int south = cellPos + new Vector2Int(0, -1);
            Vector2Int east = cellPos + new Vector2Int(1, 0);
            Vector2Int west = cellPos + new Vector2Int(-1, 0);
            if (!IsCellWithinGrid(north, gridCellMap))
            {
                return GridDirection.Up;
            }
            if (!IsCellWithinGrid(south, gridCellMap)) 
            {
                return GridDirection.Down;
            }
            if (!IsCellWithinGrid(east, gridCellMap))
            {
                return GridDirection.Right;
            }
            if (!IsCellWithinGrid(west, gridCellMap))
            {
                return GridDirection.Left;
            }

            return GridDirection.Up;
        }
        //i feel like i will have to re-write this later
        public static void ExpandPathListWidth(List<PathNode> pathNodes, ref List<Vector2Int> cells, GridCellMapData gridCellMapData, int expansionAmount)
        {
            //hard coded for now, will change later
            Direction2D expansionDirection = Direction2D.PositiveX;

            int l = 1;
            int j = pathNodes.Count-1;
            for (int i = 0; i < pathNodes.Count; i++)
            {
                if (l >= pathNodes.Count)
                {
                    l = 0;
                }
                GridCell[,] gridCellMap = gridCellMapData.gridCellMap;
                Vector2Int cellPos = pathNodes[i].position;
                GridCell cell = gridCellMap[cellPos.x, cellPos.y];
                
                Vector2Int nextCellPos = pathNodes[l].position;
                Vector2Int lastCellPos = pathNodes[j].position;
                int nextXDif = nextCellPos.x - cellPos.x;
                int nextYDif = nextCellPos.y - cellPos.y;

                int lastXDif = lastCellPos.x - cellPos.x;
                int lastYDif = lastCellPos.y - cellPos.y;


                if (((nextYDif > 0 && lastYDif < 0) || (nextYDif < 0 && lastYDif > 0)) && nextXDif == 0 && lastXDif == 0)
                {
                    //false is left and down, true is right and up
                    bool leftDownRightUpSwitch = false;
                    if (expansionDirection == Direction2D.PositiveX || expansionDirection == Direction2D.PositiveZ)
                    {
                        //right Up
                        leftDownRightUpSwitch = true;
                    }
                    else if(expansionDirection == Direction2D.NegativeX || expansionDirection == Direction2D.NegativeZ)
                    {
                        //left down
                        leftDownRightUpSwitch = false;
                    }
                    
                    Vector2Int widthPoint1 = cellPos;
                    Vector2Int widthPoint2 = cellPos;
                    for (int k = 0; k < expansionAmount; k++)
                    {
                        if (leftDownRightUpSwitch == false)
                        {
                            //left
                            widthPoint1 += new Vector2Int(-1, 0);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint1, gridCellMapData))
                            {
                                gridCellMap[widthPoint1.x, widthPoint1.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint1);
                                
                            } 
                            

                        }
                        else
                        {
                            //right
                            widthPoint2 += new Vector2Int(1, 0);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint2, gridCellMapData))
                            {
                                gridCellMap[widthPoint2.x, widthPoint2.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint2);

                            }
                        }
                        leftDownRightUpSwitch = !leftDownRightUpSwitch;
                    }
                }
                else if (((nextXDif > 0 && lastXDif < 0) || (nextXDif < 0 && lastXDif > 0)) && nextYDif == 0 && lastYDif == 0)
                {
                    //up and down
                    bool leftDownRightUpSwitch = false;
                    if (expansionDirection == Direction2D.PositiveX || expansionDirection == Direction2D.PositiveZ)
                    {
                        //right Up
                        leftDownRightUpSwitch = true;
                    }
                    else if (expansionDirection == Direction2D.NegativeX || expansionDirection == Direction2D.NegativeZ)
                    {
                        //left down
                        leftDownRightUpSwitch = false;
                    }

                    Vector2Int widthPoint1 = cellPos;
                    Vector2Int widthPoint2 = cellPos;
                    for (int k = 0; k < expansionAmount; k++)
                    {
                        if (leftDownRightUpSwitch == false)
                        {
                            //down
                            widthPoint1 += new Vector2Int(0, -1);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint1, gridCellMapData))
                            {
                                gridCellMap[widthPoint1.x, widthPoint1.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint1);

                            }


                        }
                        else
                        {
                            //up
                            widthPoint2 += new Vector2Int(0, 1);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint2, gridCellMapData))
                            {
                                gridCellMap[widthPoint2.x, widthPoint2.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint2);

                            }
                        }
                        leftDownRightUpSwitch = !leftDownRightUpSwitch;
                    }
                }
                else if (lastYDif < 0 && lastXDif == 0 && nextYDif == 0 && nextXDif > 0)
                {
                    //left and up
                    
                    bool leftDownRightUpSwitch = false;
                    if (expansionDirection == Direction2D.PositiveX || expansionDirection == Direction2D.PositiveZ)
                    {
                        //right Up
                        leftDownRightUpSwitch = true;
                    }
                    else if (expansionDirection == Direction2D.NegativeX || expansionDirection == Direction2D.NegativeZ)
                    {
                        //left down
                        leftDownRightUpSwitch = false;
                    }

                    Vector2Int widthPoint1 = cellPos;
                    Vector2Int widthPoint2 = cellPos;
                    for (int k = 0; k < expansionAmount; k++)
                    {
                        if (leftDownRightUpSwitch == false)
                        {
                            //left
                            widthPoint1 += new Vector2Int(-1, 0);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint1, gridCellMapData))
                            {
                                gridCellMap[widthPoint1.x, widthPoint1.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint1);

                            }


                        }
                        else
                        {
                            //right
                            widthPoint2 += new Vector2Int(0, 1);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint2, gridCellMapData))
                            {
                                gridCellMap[widthPoint2.x, widthPoint2.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint2);

                            }
                        }
                        leftDownRightUpSwitch = !leftDownRightUpSwitch;
                    }
                }
                else if (lastYDif < 0 && lastXDif == 0 && nextYDif == 0 && nextXDif < 0)
                {
                    //right and up
                    
                    bool leftDownRightUpSwitch = false;
                    if (expansionDirection == Direction2D.PositiveX || expansionDirection == Direction2D.PositiveZ)
                    {
                        //right Up
                        leftDownRightUpSwitch = true;
                    }
                    else if (expansionDirection == Direction2D.NegativeX || expansionDirection == Direction2D.NegativeZ)
                    {
                        //left down
                        leftDownRightUpSwitch = false;
                    }

                    Vector2Int widthPoint1 = cellPos;
                    Vector2Int widthPoint2 = cellPos;
                    for (int k = 0; k < expansionAmount; k++)
                    {
                        if (leftDownRightUpSwitch == false)
                        {
                            //up
                            widthPoint1 += new Vector2Int(0, 1);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint1, gridCellMapData))
                            {
                                gridCellMap[widthPoint1.x, widthPoint1.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint1);

                            }


                        }
                        else
                        {
                            //right
                            widthPoint2 += new Vector2Int(1, 0);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint2, gridCellMapData))
                            {
                                gridCellMap[widthPoint2.x, widthPoint2.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint2);

                            }
                        }
                        leftDownRightUpSwitch = !leftDownRightUpSwitch;
                    }
                }
                else if (lastYDif > 0 && lastXDif == 0 && nextYDif == 0 && nextXDif > 0)
                {
                    
                    //right and down
                    bool leftDownRightUpSwitch = false;
                    if (expansionDirection == Direction2D.PositiveX || expansionDirection == Direction2D.PositiveZ)
                    {
                        //right Up
                        leftDownRightUpSwitch = true;
                    }
                    else if (expansionDirection == Direction2D.NegativeX || expansionDirection == Direction2D.NegativeZ)
                    {
                        //left down
                        leftDownRightUpSwitch = false;
                    }

                    Vector2Int widthPoint1 = cellPos;
                    Vector2Int widthPoint2 = cellPos;
                    for (int k = 0; k < expansionAmount; k++)
                    {
                        if (leftDownRightUpSwitch == false)
                        {
                            //down
                            widthPoint1 += new Vector2Int(0, -1);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint1, gridCellMapData))
                            {
                                gridCellMap[widthPoint1.x, widthPoint1.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint1);

                            }


                        }
                        else
                        {
                            //right
                            widthPoint2 += new Vector2Int(1, 0);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint2, gridCellMapData))
                            {
                                gridCellMap[widthPoint2.x, widthPoint2.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint2);

                            }
                        }
                        leftDownRightUpSwitch = !leftDownRightUpSwitch;
                    }
                }
                else if (lastYDif > 0 && lastXDif == 0 && nextYDif == 0 && nextXDif < 0)
                {
                    
                    //left and down
                    bool leftDownRightUpSwitch = false;
                    if (expansionDirection == Direction2D.PositiveX || expansionDirection == Direction2D.PositiveZ)
                    {
                        //right Up
                        leftDownRightUpSwitch = true;
                    }
                    else if (expansionDirection == Direction2D.NegativeX || expansionDirection == Direction2D.NegativeZ)
                    {
                        //left down
                        leftDownRightUpSwitch = false;
                    }

                    Vector2Int widthPoint1 = cellPos;
                    Vector2Int widthPoint2 = cellPos;
                    for (int k = 0; k < expansionAmount; k++)
                    {
                        if (leftDownRightUpSwitch == false)
                        {
                            //down
                            widthPoint1 += new Vector2Int(0, -1);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint1, gridCellMapData))
                            {
                                gridCellMap[widthPoint1.x, widthPoint1.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint1);

                            }


                        }
                        else
                        {
                            //left
                            widthPoint2 += new Vector2Int(-1, 0);
                            //check that this is point is valid and if it is add it to the lists/mark it as a hall
                            if (CheckIfTileIsValid(widthPoint2, gridCellMapData))
                            {
                                gridCellMap[widthPoint2.x, widthPoint2.y].cellStatus = CellStatus.Hallway;
                                cells.Add(widthPoint2);

                            }
                        }
                        leftDownRightUpSwitch = !leftDownRightUpSwitch;
                    }
                }


                l++;
                j = i;
            }
        }

        public static Vector2Int GetAdveragePosition(List<Vector2Int> positionList)
        {
            Vector2Int center = Vector2Int.zero;
            for (int i = 0; i < positionList.Count; i++)
            {
                center += positionList[i];

            }
            center = center / positionList.Count;

            return center;
        }



    }
    public class PathNode
    {
        public bool isWalkable = true;
        public Vector2Int position;
        public PathNode cameFromNode;
        public GridCell gridCell;
        public int hCost;
        public int gCost;
        public int fCost;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }

    public class GridCell
    {
        public Vector2 position;
        public CellStatus cellStatus;
        public DungeonChamberStory parentStory;
        public Hallway parentHallway;
    }

    public class GridCellMapData
    {
        public GridCell[,] gridCellMap;
        public Vector2 origin;
        public Vector2 bounds;
        public float gridSize;
    }

    public enum CellStatus
    {
        Free,
        Room,
        Hallway,
        Forbidden
    }

    public enum Direction2D
    {
        PositiveX,
        NegativeX,
        PositiveZ,
        NegativeZ
    }



    public class Hallway
    {
        public List<Vector2Int> hallwayCells;
        public List<BorderCell> exitCells;

        public Hallway(List<Vector2Int> hallwayCells, List<BorderCell> exitCells)
        {
            this.hallwayCells = hallwayCells;
            this.exitCells = exitCells;
        }
    }


}



