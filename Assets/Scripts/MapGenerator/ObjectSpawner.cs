using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ObjectSpawner : MonoBehaviour
{
    enum spawnType { Torchlight, WallObstacle, CenterObstacle, Waypoint, Player, Platform, Enemy };

    [Range(0, 150)]
    public int randomCenterObstaclesFillCount;
    
    [Range(0, 6)]
    public float centerObstacleRadius;
    [Range(0, 6)]
    public float wallObstacleRadius;
    [Range(0, 10)]
    public int waypointsCount;
    public GameObject player;
    public GameObject enemy;
    public GameObject platform;
    public GameObject centerObstaclesHolder;
    public GameObject wallObstaclesHolder;
    public GameObject enemyHolder;
    public GameObject ReloadMapUI;
    public GameObject enemyWaypointsHolder;
    public GameObject torchlight;
    public GameObject[] spawnableCenterObjects;
    public GameObject[] spawnableWallObjects;
    public MeshFilter floor;
    public MeshFilter walls;
    public List<Vector3> centersWall;
    public List<Vector3> normalsWall;
    public Text monsterCounter;
    System.Random random = new System.Random();
    GameObject obstacle;
    List<Vector3> centersTorchSorted = new List<Vector3>();
    List<Vector3> normalsTorchSorted = new List<Vector3>();
    List<Vector3> centersWallSorted = new List<Vector3>();
    List<Vector3> waypointList;
    int wallPositionReferenceNumber;
    float[] meshSize;

    private bool x = true;
    void Start()
    {
    }
    public void SortAndSpawnObjects()
    {
        centersTorchSorted = new List<Vector3>();
        normalsTorchSorted = new List<Vector3>();
        centersWallSorted = new List<Vector3>();
        meshSize = GetTriSizes(floor.sharedMesh.triangles, floor.sharedMesh.vertices);
        SortWallPositions();
    }

    void SpawnObjects()
    {
        SpawnTorchlights();
        SpawnWallObstacles();
        SpawnCenterObstacles();
        SpawnWaypoints();
        SpawnPlatform();

    }

    void SpawnTorchlights()
    {
        for (int x = 0; x < centersTorchSorted.Count; x++)
        {
            Spawn(wallObstaclesHolder, spawnType.Torchlight, wallObstacleRadius, x);
        }

    }

    void SpawnWallObstacles()
    {
        for (int x = 0; x < centersWallSorted.Count; x++)
        {
            Spawn(wallObstaclesHolder, spawnType.WallObstacle, wallObstacleRadius, x);
        }

    }

    void SpawnCenterObstacles()
    {
        for (int x = 0; x <= randomCenterObstaclesFillCount; x++)
        {
            Spawn(centerObstaclesHolder, spawnType.CenterObstacle, centerObstacleRadius);
        }

    }
    void SpawnWaypoints()
    {
        for (int x = 0; x <= waypointsCount; x++)
        {
            Spawn(enemyWaypointsHolder, spawnType.Waypoint, centerObstacleRadius);
        }
    }
    void SpawnPlatform()
    {
        Spawn(centerObstaclesHolder, spawnType.Platform, centerObstacleRadius);

    }

    void SpawnEnemies()
    {
        int count = GameObject.FindWithTag("Player").GetComponent<CharacterStats>().characterDefinition.charLevel * 2;
        waypointList = SetWaypoints();
        SetMonsterCounter(count);
        for (int x = 0; x < count; x++)
        {
            Spawn(enemyHolder, spawnType.Enemy, centerObstacleRadius);
        }
    }
    void Spawn(GameObject parent, spawnType type, float objectDensity, int wallObstacleCount = 0)
    {
        switch (type)
        {
            case spawnType.Torchlight:
                {
                    obstacle = torchlight;
                    obstacle.hideFlags = HideFlags.HideInHierarchy;

                    break;
                }
            case spawnType.WallObstacle:
                {
                    obstacle = spawnableWallObjects[random.Next(0, spawnableWallObjects.Length - 1)];
                    break;
                }
            case spawnType.CenterObstacle:
                {
                    obstacle = spawnableCenterObjects[random.Next(0, spawnableCenterObjects.Length)];
                    break;
                }
            case spawnType.Waypoint:
                {
                    obstacle = new GameObject("Waypoint");
                    obstacle.hideFlags = HideFlags.HideInHierarchy;
                    break;
                }
            case spawnType.Platform:
                {
                    obstacle = platform;
                    obstacle.hideFlags = HideFlags.HideInHierarchy;
                    break;
                }
            case spawnType.Enemy:
                {
                    obstacle = enemy;
                    break;
                }
            default:
                break;
        }

        if (obstacle == null)
            Destroy(obstacle);

        Vector3 position = GetPosition(type, wallObstacleCount);
        bool validPosition = false;


        if (type == spawnType.CenterObstacle || type == spawnType.Waypoint || type == spawnType.Platform || type == spawnType.Enemy)
        {
            while (!validPosition)
            {
                position = GetPosition(type, wallObstacleCount);
                validPosition = true;
                int i = parent.transform.childCount;
                Collider[] colliders = Physics.OverlapSphere(position, objectDensity);
                foreach (Collider col in colliders)
                {
                    if (col.tag == "Obstacle" || col.tag == "BoxCollective" || col.tag == "Wall")
                    {
                        validPosition = false;
                        break;
                    }
                }
            }
        }
        else
            validPosition = true;
        if (validPosition)
        {

            switch (type)
            {
                case spawnType.Torchlight:
                    {
                        var obs = Instantiate(obstacle, position + obstacle.transform.position, Quaternion.identity);
                        obs.transform.parent = parent.transform;
                        var newHeigh = new Vector3(obs.transform.position.x, 2f, obs.transform.position.z);
                        obs.transform.position = newHeigh;
                        obs.transform.LookAt((centersTorchSorted[wallObstacleCount] + normalsTorchSorted[wallObstacleCount]));
                        break;
                    }
                case spawnType.WallObstacle:
                    {
                        var obs = Instantiate(obstacle, position + obstacle.transform.position, Quaternion.Euler(0, 0, 0));
                        obs.transform.parent = parent.transform;
                        var newHeigh = new Vector3(obs.transform.position.x, 0f, obs.transform.position.z);
                        obs.transform.position = newHeigh;
                        break;
                    }
                case spawnType.CenterObstacle:
                    {
                        var obs = Instantiate(obstacle, position + obstacle.transform.position, Quaternion.identity);
                        obs.transform.parent = parent.transform;

                        break;
                    }
                case spawnType.Waypoint:
                    {
                        var obs = Instantiate(obstacle, position + obstacle.transform.position, Quaternion.identity);
                        obs.transform.parent = parent.transform;
                        obs.gameObject.tag = "Waypoint";
                        break;
                    }
                case spawnType.Platform:
                    {
                        var obs = Instantiate(obstacle, position + obstacle.transform.position, Quaternion.identity);
                        obs.transform.parent = parent.transform;
                        obs.GetComponent<Platform>().ReloadMapUI = ReloadMapUI;
                        GetComponent<PlayerSpawner>().CreatePlayer(obs);
                        break;
                    }
                case spawnType.Enemy:
                    {
                        var obs = Instantiate(obstacle, waypointList[random.Next(0, waypointList.Count)], Quaternion.identity);
                        obs.transform.parent = parent.transform;
                        var stats = SkeletorStats();
                        obs.GetComponent<EnemyBehaviour>().enemyDefinition = stats;
                        obs.GetComponent<EnemyStats>().enemyDefinition = stats;
                        obs.GetComponent<EnemyAI>().patrolTargets = waypointList.ToArray();
                        break;
                    }
                default:
                    break;
            }
        }
    }
    Vector3 GetRandomPointOnMesh(Mesh mesh)
    {
        float[] sizes = meshSize;
        float[] cumulativeSizes = new float[sizes.Length];
        float total = 0;

        for (int i = 0; i < sizes.Length; i++)
        {
            total += sizes[i];
            cumulativeSizes[i] = total;
        }

        float randomsample = Random.value * total;

        int triIndex = -1;

        for (int i = 0; i < sizes.Length; i++)
        {
            if (randomsample <= cumulativeSizes[i])
            {
                triIndex = i;
                break;
            }
        }

        if (triIndex == -1) Debug.LogError("triIndex should never be -1");

        Vector3 a = mesh.vertices[mesh.triangles[triIndex * 3]];
        Vector3 b = mesh.vertices[mesh.triangles[triIndex * 3 + 1]];
        Vector3 c = mesh.vertices[mesh.triangles[triIndex * 3 + 2]];

        float r = Random.value;
        float s = Random.value;

        if (r + s >= 1)
        {
            r = 1 - r;
            s = 1 - s;
        }
        Vector3 pointOnMesh = a + r * (b - a) + s * (c - a);
        return pointOnMesh;

    }

    float[] GetTriSizes(int[] tris, Vector3[] verts)
    {
        int triCount = tris.Length / 3;
        float[] sizes = new float[triCount];
        for (int i = 0; i < triCount; i++)
        {
            sizes[i] = .5f * Vector3.Cross(verts[tris[i * 3 + 1]] - verts[tris[i * 3]], verts[tris[i * 3 + 2]] - verts[tris[i * 3]]).magnitude;
        }
        return sizes;
    }

    EnemyStats_SO SkeletorStats()
    {
        var skeletorStats = new EnemyStats_SO();
        skeletorStats.maxHealth = 100;
        skeletorStats.currentHealth = 100;
        skeletorStats.maxMana = 50;
        skeletorStats.currentMana = 50;
        skeletorStats.currentDamage = 2;
        skeletorStats.normalSpeed = 3;
        skeletorStats.experienceAdded = 100;
        return skeletorStats;
    }

    List<Vector3> SetWaypoints()
    {
        var gameobjectsList = new List<GameObject>();
        var vectorsList = new List<Vector3>();
        gameobjectsList.AddRange(GameObject.FindGameObjectsWithTag("Waypoint"));
        foreach (GameObject waypoint in gameobjectsList)
        {
            vectorsList.Add(waypoint.transform.position);
        }
        return vectorsList;
    }

    void SortWallPositions()
    {
        List<Vector3> centerNotUsed = new List<Vector3>();
        int index = 0;
        for (int x = 0; x < centersWall.Count; x++)
        {
            index++;
            if (index == 30)
            {
                centersTorchSorted.Add(centersWall[x]);
                normalsTorchSorted.Add(normalsWall[x]);
                index = 0;
            }
            else
            {
                centerNotUsed.Add(centersWall[x]);
            }
        }

        index = 0;
        for (int x = 0; x < centerNotUsed.Count; x++)
        {
            index++;
            if (index == 10)
            {
                centersWallSorted.Add(centerNotUsed[x]);
                index = 0;
            }
        }
        SpawnObjects();
    }

    Vector3 GetPosition(spawnType type, int wallCount)
    {
        var position = new Vector3();

        switch (type)
        {
            case spawnType.Torchlight:
                {
                    position = centersTorchSorted[wallCount];
                    break;
                }
            case spawnType.WallObstacle:
                {
                    position = centersWallSorted[wallCount];
                    break;
                }
            default:
                position = GetRandomPointOnMesh(floor.sharedMesh);
                break;
        }


        return position;
    }

    void SetMonsterCounter(int count)
    {
        monsterCounter.text = count.ToString();
    }

    void OnEnable()
    {
        EventManager.StartListening(EventManager.PlayerInstantiated, SpawnEnemies);

    }

    void OnDisable()
    {
        EventManager.StopListening(EventManager.PlayerInstantiated, SpawnEnemies);

    }
}


