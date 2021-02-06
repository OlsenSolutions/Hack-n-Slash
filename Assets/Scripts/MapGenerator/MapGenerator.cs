using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{

    public int width;
    public int height;
    public GameObject torchlight;
    public List<GameObject> WallAddOnsLow;
    public List<GameObject> WallAddOnsTop;

    public List<GameObject> centerAddOnsBig;
    public List<GameObject> centerAddOnsSmall;
    public GameObject player;
    public GameObject WallsObstacles;
    public GameObject centerObstacles;
    public GameObject EnemyList;
    public GameObject skeletor;
    public List<mapPlace> normals = new List<mapPlace>();

    public HealthManaScript healthManaScript;

    public Image[] hotBarDisplayHolders = new Image[4];
    public GameObject InventoryDisplayHolder;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    [Range(0, 100)]
    public int randomObstaclesFillPercent;
    List<Room> survRooms = new List<Room>();
    int[,] map;

    private bool platformCreated;
    private bool playerCreated;

    private mapPlace platformPlace;

    public GameObject platform;

    public GameObject ReloadMapUI;

    private CharacterStats_SO characterStatsGame;

    void Start()
    {
        platformCreated = false;
        playerCreated = false;
        GenerateMap();
        characterStatsGame = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateMap();
        }
    }

    public void GenerateNextMap(GameObject playerInGame)
    {
        var PlayerStatsNew = CharacterStats_SO.CreateInstance<CharacterStats_SO>();
        var PlayerStatsOld = playerInGame.GetComponent<CharacterStats>().characterDefinition;
        if (PlayerStatsNew != null)
        {
            PlayerStatsNew.setManually = true;
            PlayerStatsNew.saveDataOnClose = false;
            PlayerStatsNew.maxHealth = PlayerStatsOld.maxHealth;
            PlayerStatsNew.currentHealth = PlayerStatsOld.currentHealth;
            PlayerStatsNew.maxMana = PlayerStatsOld.maxMana;
            PlayerStatsNew.currentMana = PlayerStatsOld.currentMana;
            PlayerStatsNew.currentDamage = PlayerStatsOld.currentDamage;
            PlayerStatsNew.maxEncumbrance = PlayerStatsOld.maxEncumbrance;
            PlayerStatsNew.currentEncumbrance = PlayerStatsOld.currentEncumbrance;
            PlayerStatsNew.charExperience = PlayerStatsOld.charExperience;
            PlayerStatsNew.charLevel = PlayerStatsOld.charLevel;
            PlayerStatsNew.charLevelUps = PlayerStatsOld.charLevelUps;
            PlayerStatsNew.currentMagicDamage = PlayerStatsOld.currentMagicDamage;
            PlayerStatsNew.charRenevalPoints = PlayerStatsOld.charRenevalPoints;
            characterStatsGame = PlayerStatsNew;
        }
        if (EnemyList.transform.childCount != 0)
        {
            int childs = EnemyList.transform.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                GameObject.Destroy(EnemyList.transform.GetChild(i).gameObject);
            }
        }

        Destroy(playerInGame);
        GenerateMap();
    }

    public void GenerateNewMap()
    {
        if (EnemyList.transform.childCount != 0)
        {
            int childs = EnemyList.transform.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                GameObject.Destroy(EnemyList.transform.GetChild(i).gameObject);
            }
        }
        GenerateMap();
    }

    public void RestartMap()
    {
        var playerInstance = GameObject.FindGameObjectWithTag("Player");
        GenerateNextMap(playerInstance);
    }

    void GenerateMap()
    {
        ClearMap();
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int borderSize = 10;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, 1);
        if (meshGen.wallsPositions != null)
        {
            FillMap(meshGen.wallsPositions);
        }
        else
        {
            Debug.LogError("NIMO");
        }
    }

    void FillMap(List<Vector3>[] wallsPosition)
    {
        WallPlacer(wallsPosition[0], wallsPosition[1]);
        InsidePlacer();
        GetComponent<MeshGenerator>().BakingNavMesh();
    }

    void ClearMap()
    {
        platformCreated = false;
        playerCreated = false;
        if (WallsObstacles.transform.childCount != 0)
        {
            int childs = WallsObstacles.transform.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                GameObject.Destroy(WallsObstacles.transform.GetChild(i).gameObject);
            }
        }

        if (centerObstacles.transform.childCount != 0)
        {
            int childs = centerObstacles.transform.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                GameObject.Destroy(centerObstacles.transform.GetChild(i).gameObject);
            }
        }
        
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;
        survRooms = survivingRooms;
        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach (Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, 4);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y, false));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY, false));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y, false));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }


    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void WallPlacer(List<Vector3> center, List<Vector3> normal)
    {
        List<Vector3> centerNotUsed = new List<Vector3>();
        List<Vector3> normalNotUsed = new List<Vector3>();
        int index = 0;
        for (int x = 0; x < center.Count; x++)
        {
            index++;
            if (index == 14)
            {
                var wallObstacle = Instantiate(WallAddOnsTop[0], center[x], torchlight.transform.rotation);
                wallObstacle.transform.parent = WallsObstacles.transform;
                wallObstacle.transform.LookAt((center[x] + normal[x]));
            }
            else if (index == 30)
            {
                var torch = Instantiate(torchlight, center[x], torchlight.transform.rotation);
                torch.transform.parent = WallsObstacles.transform;
                torch.transform.LookAt((center[x] + normal[x]));
                index = 0;
            }
            else
            {
                centerNotUsed.Add(center[x]);
                normalNotUsed.Add(normal[x]);
            }
        }
        FillWallObstacles(centerNotUsed, normalNotUsed);

    }

    void FillWallObstacles(List<Vector3> center, List<Vector3> normal)
    {
        var random = new System.Random();

        int index = 0;
        for (int x = 0; x < center.Count; x++)
        {
            index++;
            if (index == 10)
            {

                if (center[x].y == -2)
                {
                    int indexer = random.Next(0, WallAddOnsLow.Count);
                    var obstacle = Instantiate(WallAddOnsLow[indexer], center[x] + (normal[x] / 5), WallAddOnsLow[indexer].transform.rotation);
                    obstacle.transform.parent = WallsObstacles.transform;

                    obstacle.transform.LookAt((center[x] + normal[x]));
                    index = 0;
                }
                else
                {
                    int indexer = random.Next(0, WallAddOnsLow.Count);
                    var obstacle = Instantiate(WallAddOnsLow[indexer], center[x - 1] + (normal[x - 1] / 5), WallAddOnsLow[indexer].transform.rotation);
                    obstacle.transform.parent = WallsObstacles.transform;

                    obstacle.transform.LookAt((center[x - 1] + normal[x - 1]));
                    index = 0;
                }

            }
        }

    }

    void InsidePlacer()
    {
        var random = new System.Random();
        int big = 0;
        int small = 0;

        List<mapPlace> mapList = new List<mapPlace>();
        List<mapPlace> mapListSmall = new List<mapPlace>();
        List<mapPlace> mapListPlayer = new List<mapPlace>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0)
                {
                    var mPlace = new mapPlace(x, y);
                    mapList.Add(mPlace);
                    mapListSmall.Add(mPlace);
                    mapListPlayer.Add(mPlace);

                }
            }
        }

        int mapSize = mapList.Count;
        for (int i = 0; i < mapSize; i++)
        {
            int index = 0;
            index = random.Next(0, mapList.Count - 1);
            mapPlace mPlace = mapList[index];

            for (int z = mPlace.posX - 10; z <= mPlace.posX + 10; z++)
            {
                for (int h = mPlace.posY - 10; h <= mPlace.posY + 10; h++)
                {
                    if (z == mPlace.posX || h == mPlace.posY)
                    {
                        if (z > 0 && z < map.GetLength(0) && h > 0 && h < map.GetLength(1) && map[z, h] != 1 && map[z, h] != 2)
                        {
                            big++;
                            //Debug.DrawLine(new Vector3(mPlace.posX - width / 2, 0, mPlace.posY - height / 2), new Vector3(mPlace.posX - width / 2, 10, mPlace.posY - height / 2), Color.blue, 100);
                            if (big == 41)
                            {
                                //     Debug.DrawLine(new Vector3(mPlace.posX - width / 2, 0, mPlace.posY - height / 2), new Vector3(mPlace.posX - width / 2, 10, mPlace.posY - height / 2), Color.blue, 100);
                                map[mPlace.posX, mPlace.posY] = 2;

                                int r = random.Next(0, 100);
                                if (r < randomObstaclesFillPercent)
                                {
                                    int indexer = random.Next(0, centerAddOnsBig.Count);
                                    var obstacle = Instantiate(centerAddOnsBig[indexer], new Vector3(mPlace.posX - width / 2, -2.85f, mPlace.posY - height / 2), centerAddOnsBig[indexer].transform.rotation);
                                    obstacle.transform.parent = centerObstacles.transform;

                                }
                                big = 0;
                            }
                        }
                    }
                }
            }
            mapList.Remove(mapList[index]);
            big = 0;
        }


        int mapSizeSmall = mapListSmall.Count;
        for (int i = 0; i < mapSizeSmall; i++)
        {
            int index = 0;
            index = random.Next(0, mapListSmall.Count - 1);
            mapPlace mPlace = mapListSmall[index];
            for (int z = mPlace.posX - 5; z <= mPlace.posX + 5; z++)
            {
                for (int h = mPlace.posY - 5; h <= mPlace.posY + 5; h++)
                {
                    if (z == mPlace.posX || h == mPlace.posY)
                    {
                        if (z > 0 && z < map.GetLength(0) && h > 0 && h < map.GetLength(1) && map[z, h] != 1 && map[z, h] != 2)
                        {
                            small++;
                            //  Debug.DrawLine(new Vector3(mPlace.posX - width / 2, 0, mPlace.posY - height / 2), new Vector3(mPlace.posX - width / 2, 10, mPlace.posY - height / 2), Color.blue, 100);

                            if (small == 21)
                            {
                                //   Debug.DrawLine(new Vector3(mPlace.posX - width / 2, 0, mPlace.posY - height / 2), new Vector3(mPlace.posX - width / 2, 10, mPlace.posY - height / 2), Color.blue, 100);
                                map[mPlace.posX, mPlace.posY] = 2;

                                int r = random.Next(0, 100);
                                if (r < randomObstaclesFillPercent)
                                {
                                    int indexer = random.Next(0, centerAddOnsSmall.Count);
                                    var obstacle = Instantiate(centerAddOnsSmall[indexer], new Vector3(mPlace.posX - width / 2, -2.85f, mPlace.posY - height / 2), centerAddOnsSmall[indexer].transform.rotation);
                                    obstacle.transform.parent = centerObstacles.transform;
                                }
                                small = 0;
                            }
                        }
                    }
                }
            }
            mapListSmall.Remove(mapListSmall[index]);
            small = 0;
        }


        int mapSizePlayer = mapListPlayer.Count;
        for (int i = 0; i < mapSizePlayer; i++)
        {
            int platformindex = 0;
            int index = 0;
            index = random.Next(0, mapListPlayer.Count - 1);
            mapPlace mPlace = mapListPlayer[index];

            for (int z = mPlace.posX - 50; z <= mPlace.posX + 50; z++)
            {
                for (int h = mPlace.posY - 50; h <= mPlace.posY + 50; h++)
                {
                    if (z == mPlace.posX || h == mPlace.posY)
                    {
                        if (z > 0 && z < map.GetLength(0) && h > 0 && h < map.GetLength(1) && map[z, h] != 1 && map[z, h] != 2)
                        {
                            platformindex++;
                            // Debug.DrawLine(new Vector3(mPlace.posX - width / 2, 0, mPlace.posY - height / 2), new Vector3(mPlace.posX - width / 2, 10, mPlace.posY - height / 2), Color.blue, 100);
                            if (platformindex > 145)
                            {
                                if (platformCreated == false)
                                {
                                    map[mPlace.posX, mPlace.posY] = 2;
                                    var obstacle = Instantiate(platform, new Vector3(mPlace.posX - width / 2, -2.85f, mPlace.posY - height / 2), platform.transform.rotation);
                                    obstacle.GetComponent<Platform>().ReloadMapUI = ReloadMapUI;
                                    obstacle.transform.parent = centerObstacles.transform;
                                    platformCreated = true;
                                    platformPlace = mPlace;
                                    var playerPrefab = Instantiate(player, new Vector3(mPlace.posX - width / 2, -2.85f, mPlace.posY - height / 2), player.transform.rotation);
                                    playerPrefab.GetComponent<CharacterStats>().charInv.hotBarDisplayHolders = hotBarDisplayHolders;
                                    playerPrefab.GetComponent<CharacterStats>().charInv.InventoryDisplayHolder = InventoryDisplayHolder;
                                    playerPrefab.GetComponent<CharacterStats>().charInv.ClearInventory();

                                    if (characterStatsGame != null)
                                    {
                                        playerPrefab.GetComponent<CharacterStats>().characterDefinition = characterStatsGame;
                                    }

                                    playerPrefab.GetComponent<CharacterBehaviour>().fireballHolder = centerObstacles;
                                    var pointer = GameObject.FindGameObjectWithTag("Pointer");
                                    if (pointer !=null)
                                        pointer.GetComponent<Pointer>().targetPosition = obstacle.transform.position;
                                }

                            }
                        }
                    }
                }
            }
            mapListPlayer.Remove(mapListPlayer[index]);
            platformindex = 0;
        }

        SetWaypoints();

    }

    void SetWaypoints()
    {
        var random = new System.Random();
        int waypoint = 0;
        List<mapPlace> mapList = new List<mapPlace>();

        List<mapPlace> waypoints = new List<mapPlace>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0)
                {
                    var mPlace = new mapPlace(x, y);
                    mapList.Add(mPlace);
                }
            }
        }

        int mapSize = mapList.Count;
        for (int i = 0; i < mapSize; i++)
        {
            int index = 0;
            index = random.Next(0, mapList.Count - 1);
            mapPlace mPlace = mapList[index];
            if (mPlace.posX != platformPlace.posX && mPlace.posY != platformPlace.posY)
            {
                for (int z = mPlace.posX - 16; z <= mPlace.posX + 16; z++)
                {
                    for (int h = mPlace.posY - 16; h <= mPlace.posY + 16; h++)
                    {
                        if (z == mPlace.posX || h == mPlace.posY)
                        {

                            if (z > 0 && z < map.GetLength(0) && h > 0 && h < map.GetLength(1) && map[z, h] != 1 && map[z, h] != 2)
                            {
                                waypoint++;
                                // Debug.LogError(waypoint);
                                // Debug.DrawLine(new Vector3(mPlace.posX - width / 2, 0, mPlace.posY - height / 2), new Vector3(mPlace.posX - width / 2, 10, mPlace.posY - height / 2), Color.blue, 100);
                                if (waypoint == 61)
                                {
                                    //Debug.DrawLine(new Vector3(mPlace.posX - width / 2, 0, mPlace.posY - height / 2), new Vector3(mPlace.posX - width / 2, 10, mPlace.posY - height / 2), Color.blue, 100);
                                    map[mPlace.posX, mPlace.posY] = 2;
                                    mapPlace waypointPoint = new mapPlace();
                                    waypointPoint.posX = mPlace.posX - width / 2;
                                    waypointPoint.posY = mPlace.posY - height / 2;

                                    waypoints.Add(waypointPoint);



                                    waypoint = 0;
                                }
                            }
                        }
                    }
                }
                mapList.Remove(mapList[index]);
                waypoint = 0;
            }
        }
        InstantiateEnemies(waypoints);

    }

    private void InstantiateEnemies(List<mapPlace> waypoints)
    {
        if(GameObject.FindGameObjectWithTag("Player")!=null)
        {
        var waypointsEnemies = waypoints;
        int waypointsEnemiesLenght = 0;
        waypointsEnemiesLenght = waypoints.Count;
        //       Debug.LogError(waypointsEnemiesLenght);
        var transformsList = new List<Vector3>();
            foreach(mapPlace way in waypoints)
            {
                var trans = new Vector3 (way.posX, -2f, way.posY);
                transformsList.Add(trans);
            }
        var waypointsPatrol = transformsList.ToArray();


        foreach (mapPlace place in waypointsEnemies)
        {
            var skeletorI = Instantiate(skeletor, new Vector3(place.posX, 0, place.posY), skeletor.transform.rotation);
            var skeletorStats = EnemyStats_SO.CreateInstance<EnemyStats_SO>();
            skeletorStats.maxHealth = 100;
            skeletorStats.currentHealth = 100;
            skeletorStats.maxMana = 50;
            skeletorStats.currentMana = 50;
            skeletorStats.currentDamage = 2;
            skeletorStats.normalSpeed = 3;
            skeletorStats.experienceAdded = 100;

            skeletorI.GetComponent<EnemyBehaviour>().enemyDefinition = skeletorStats;
            skeletorI.GetComponent<EnemyStats>().enemyDefinition = skeletorStats;
            skeletorI.transform.parent = EnemyList.transform;
            skeletorI.GetComponent<EnemyAI>().patrolTargets = waypointsPatrol;

        }

        healthManaScript.MonsterCounter.text = waypointsEnemiesLenght.ToString();
        EventManager.TriggerEvent(EventManager.MapCreated);
        }
        else
        {
            Debug.LogError("NIMO");
            GenerateNewMap();
    }
    }



    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);

                if (neighbourWallTiles > 4)
                    map[x, y] = 1;
                else if (neighbourWallTiles < 4)
                    map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;
        public bool haveObject;

        public Coord(int x, int y, bool z)
        {
            tileX = x;
            tileY = y;
            haveObject = z;
        }
    }

    public struct mapPlace
    {
        public int posX;
        public int posY;

        public mapPlace(int x, int y)
        {
            posX = x;
            posY = y;
        }
    }


    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public Room()
        {
        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
}