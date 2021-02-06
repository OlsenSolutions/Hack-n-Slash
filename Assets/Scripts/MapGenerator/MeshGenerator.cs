using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.Events;
using System;

public class MeshGenerator : MonoBehaviour
{

    public SquareGrid squareGrid;
    public MeshFilter walls;
    public MeshFilter floor;
    public MeshFilter cave;
    public List<Material> wallMaterials;
    public List<Material> floorMaterials;
    List<Vector3> meshVertices;
    List<int> meshTriangles;
    List<Vector3> floorVertices;
    List<int> floorTriangles;
    public List<Vector3>[] wallsPositions = new List<Vector3>[2];
    List<Vector2> normals = new List<Vector2>();
    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    Dictionary<int, List<Triangle>> triangleFloorDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();
    HashSet<int> checkedFloorVertices = new HashSet<int>();


    public void GenerateMesh(int[,] map, float squareSize)
    {

        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();
        wallsPositions = new List<Vector3>[2];
        squareGrid = new SquareGrid(map, squareSize);

        meshVertices = new List<Vector3>();
        meshTriangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y], meshTriangles, meshVertices, checkedVertices, triangleDictionary);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();


        int tileAmount = 10;
        Vector2[] uvs = new Vector2[meshVertices.Count];
        for (int i = 0; i < meshVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, meshVertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, meshVertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        CreateWallMesh(map, squareSize);
    }

    void CreateWallMesh(int[,] map, float squareSize)
    {

        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 3;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(meshVertices[outline[i]]); // left
                wallVertices.Add(meshVertices[outline[i + 1]]); // right
                wallVertices.Add(meshVertices[outline[i]] - Vector3.up * wallHeight); // bottom left
                wallVertices.Add(meshVertices[outline[i + 1]] - Vector3.up * wallHeight); // bottom right



                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }



        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[wallVertices.Count];
        for (int i = 0; i < wallVertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, wallMesh.vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, wallMesh.vertices[i].y) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        wallMesh.uv = uvs;


        Vector3[] vertices = wallMesh.vertices;
        int[] triangles = wallMesh.triangles;
        List<Vector3> normals = new List<Vector3>();
        wallsPositions[0] = new List<Vector3>();
        wallsPositions[1] = new List<Vector3>();

        wallMesh.RecalculateNormals();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v1 = vertices[triangles[i]];
            Vector3 v2 = vertices[triangles[i + 1]];
            Vector3 v3 = vertices[triangles[i + 2]];

            Vector3 center = (v1 + v2 + v3) / 3;
            Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
            normals.Add(normal);
            normals.Add(normal);

            wallsPositions[0].Add(center);
            wallsPositions[1].Add(normal);
        }
        wallMesh.normals = normals.ToArray();


        wallMesh.RecalculateBounds();
        wallMesh.RecalculateNormals();
        walls.mesh = wallMesh;
        if (walls.gameObject.GetComponent<MeshCollider>() != null)
        {
            walls.GetComponent<MeshCollider>().sharedMesh = wallMesh;
        }
        else
        {
            walls.gameObject.AddComponent<MeshCollider>();
            walls.GetComponent<MeshCollider>().sharedMesh = wallMesh;
        }

        SetMaterials();
    }

    private void SetMaterials()
    {
        var random = new System.Random();
        int floorIndex = random.Next(0, floorMaterials.Count - 1);
        floor.gameObject.GetComponent<MeshRenderer>().material = floorMaterials[floorIndex];
        int wallIndex = random.Next(0, wallMaterials.Count - 1);
        walls.gameObject.GetComponent<MeshRenderer>().material = wallMaterials[wallIndex];
    }

    public void BakingNavMesh()
    {
        floor.GetComponent<NavMeshSurface>().BuildNavMesh();

    }

    void TriangulateSquare(Square square, List<int> triangleAd, List<Vector3> verticeAd, HashSet<int> checkedHash, Dictionary<int, List<Triangle>> triangleDict)
    {
        switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 4:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topRight, square.centreRight, square.centreTop);
                break;
            case 8:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 point:
            case 15:
                MeshFromPoints(verticeAd, triangleAd, triangleDict, square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedHash.Add(square.topLeft.vertexIndex);
                checkedHash.Add(square.topRight.vertexIndex);
                checkedHash.Add(square.bottomRight.vertexIndex);
                checkedHash.Add(square.bottomLeft.vertexIndex);
                break;
        }

    }

    void MeshFromPoints(List<Vector3> verticesAd, List<int> triangleAd, Dictionary<int, List<Triangle>> triangleDict, params Node[] points)
    {
        AssignVertices(points, verticesAd);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2], triangleAd, triangleDict);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3], triangleAd, triangleDict);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4], triangleAd, triangleDict);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5], triangleAd, triangleDict);

    }

    void AssignVertices(Node[] points, List<Vector3> verticesAd)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = verticesAd.Count;
                verticesAd.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c, List<int> trianglesAd, Dictionary<int, List<Triangle>> triangleDict)
    {
        trianglesAd.Add(a.vertexIndex);
        trianglesAd.Add(b.vertexIndex);
        trianglesAd.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle, triangleDict);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle, triangleDict);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle, triangleDict);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle, Dictionary<int, List<Triangle>> triangleDict)
    {
        if (triangleDict.ContainsKey(vertexIndexKey))
        {
            triangleDict[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDict.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < meshVertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }


        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
                    controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }

        }
    }

    public class Square
    {

        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            if (topLeft.active)
                configuration += 8;
            if (topRight.active)
                configuration += 4;
            if (bottomRight.active)
                configuration += 2;
            if (bottomLeft.active)
                configuration += 1;
        }

    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {

        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }

    }
}