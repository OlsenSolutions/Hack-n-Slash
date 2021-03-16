using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ObjectSpawner : MonoBehaviour
{
    enum spawnType { Obstacle, Waypoint, Player, Platform, Enemy };

    [Range(0, 60)]
    public int randomObstaclesFillPercent;
    [Range(0, 6)]
    public float centerObstacleRadius;
    [Range(0, 10)]
    public int waypointsCount;
    public GameObject player;
    public GameObject enemy;
    public GameObject platform;
    public GameObject centerObstaclesHolder;
    public GameObject enemyHolder;
    public GameObject ReloadMapUI;
    public GameObject enemyWaypointsHolder;
    public GameObject[] spawnableObjects;
    public MeshFilter floor;
    public MeshFilter walls;
    public Text monsterCounter;
    System.Random random = new System.Random();
    GameObject obstacle;

    List<Vector3> waypointList;
    void Start()
    {
    }
    public void SpawnObjects()
    {
        SpawnObstacles();
        SpawnWaypoints();
        SpawnPlatform();
    }

    void SpawnObstacles()
    {
        for (int x = 0; x <= randomObstaclesFillPercent; x++)
        {
            Spawn(floor.sharedMesh, centerObstaclesHolder, spawnType.Obstacle);
        }

    }
    void SpawnWaypoints()
    {
        for (int x = 0; x <= waypointsCount; x++)
        {
            Spawn(floor.sharedMesh, enemyWaypointsHolder, spawnType.Waypoint);
        }
    }
    void SpawnPlatform()
    {
        Spawn(floor.sharedMesh, centerObstaclesHolder, spawnType.Platform);

    }

    void SpawnEnemies()
    {
        int count = GameObject.FindWithTag("Player").GetComponent<CharacterStats>().characterDefinition.charLevel * 2;
        waypointList = SetWaypoints();
        SetMonsterCounter(count);
        for (int x = 0; x < count; x++)
        {
            Spawn(floor.sharedMesh, enemyHolder, spawnType.Enemy);
        }
    }
    void Spawn(Mesh mesh, GameObject parent, spawnType type)
    {
        switch (type)
        {
            case spawnType.Obstacle:
                {
                    obstacle = spawnableObjects[random.Next(0, spawnableObjects.Length)];
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

        Vector3 position = GetRandomPointOnMesh(mesh);
        bool validPosition = false;

        while (!validPosition)
        {
            position = GetRandomPointOnMesh(mesh);
            validPosition = true;
            Collider[] colliders = Physics.OverlapSphere(position, centerObstacleRadius);
            foreach (Collider col in colliders)
            {
                if (col.tag == "Obstacle" || col.tag == "BoxCollective" || col.tag == "Wall")
                {
                    validPosition = false;
                }
            }
        }
        if (validPosition)
        {

            switch (type)
            {
                case spawnType.Obstacle:
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

        float[] sizes = GetTriSizes(mesh.triangles, mesh.vertices);
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


