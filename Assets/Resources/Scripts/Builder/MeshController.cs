using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshController : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    bool hasBeenMorphed;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        //Sphere(12, 0.5f);
    }

    void Update()
    {
        if (hasBeenMorphed) RefreshMesh();
    }

    public void Sphere(int resolution, float radius)
    {
        float lonAngleMultiplier = Random.Range(0.1f, 10f), lonTotalPower = Random.Range(2f, 5f), lonCosinePower = Random.Range(2f, 5f), lonSinePower = 0;
        float latAngleMultiplier = Random.Range(0.1f, 10f), latTotalPower = Random.Range(2f, 5f), latCosinePower = Random.Range(2f, 5f), latSinePower = Random.Range(2f, 5f);

        for (int row = 0; row < resolution; row++)
        {
            float latA = Remap(row, 0, resolution, -Mathf.PI / 2, Mathf.PI / 2);
            float latASuperShape = Supershape(latA, latAngleMultiplier, latTotalPower, latCosinePower, latSinePower);
            float latB = Remap(row + 1, 0, resolution, -Mathf.PI / 2, Mathf.PI / 2);
            float latBSuperShape = Supershape(latB, latAngleMultiplier, latTotalPower, latCosinePower, latSinePower);
            for (int col = 0; col < resolution; col++)
            {
                float lonA = Remap(col, 0, resolution, -Mathf.PI, Mathf.PI);
                float lonASuperShape = Supershape(lonA, lonAngleMultiplier, lonTotalPower, lonCosinePower, lonSinePower);
                float lonB = Remap(col + 1, 0, resolution, -Mathf.PI, Mathf.PI);
                float lonBSuperShape = Supershape(lonB, lonAngleMultiplier, lonTotalPower, lonCosinePower, lonSinePower);

                Vector3 vertexA = DegreesToVector(lonA, latA, radius, lonASuperShape, latASuperShape);
                Vector3 vertexB = DegreesToVector(lonB, latA, radius, lonBSuperShape, latASuperShape);
                Vector3 vertexC = DegreesToVector(lonA, latB, radius, lonASuperShape, latBSuperShape);
                Vector3 vertexD = DegreesToVector(lonB, latB, radius, lonBSuperShape, latBSuperShape);

                AddQuad(vertexA, vertexB, vertexC, vertexD);
            }
        }
    }
    /// <summary>
    /// Adds a quad to the mesh of this gameObject's MeshFilter
    /// </summary>
    /// <param name="vertexA">Bottom left corner of quad</param>
    /// <param name="vertexB">Bottom right corner of quad</param>
    /// <param name="vertexC">Top left corner of quad</param>
    /// <param name="vertexD">Top right corner of quad</param>
    public void AddQuad(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC, Vector3 vertexD)
    {
        int vertexAIndex = AddVertex(vertexA);
        int vertexBIndex = AddVertex(vertexB);
        int vertexCIndex = AddVertex(vertexC);
        int vertexDIndex = AddVertex(vertexD);

        triangles.Add(vertexCIndex);
        triangles.Add(vertexBIndex);
        triangles.Add(vertexAIndex);

        triangles.Add(vertexBIndex);
        triangles.Add(vertexCIndex);
        triangles.Add(vertexDIndex);

        //hasBeenMorphed = true;
    }
    public int AddVertex(Vector3 vertex)
    {
        int vertexIndex = vertices.IndexOf(vertex);
        if (vertexIndex <= -1) vertexIndex = vertices.FindIndex(item => Vector3.Distance(item, vertex) < 0.001f);
        if (vertexIndex <= -1) { vertices.Add(vertex); vertexIndex = vertices.Count - 1; hasBeenMorphed = true; }

        return vertexIndex;
    }
    private void RefreshMesh()
    {
        Mesh mesh = meshFilter.mesh;
        if (mesh == null) { mesh = new Mesh(); mesh.name = name; }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        hasBeenMorphed = false;
    }
    public void ClearMesh()
    {
        vertices.Clear();
        triangles.Clear();
        hasBeenMorphed = true;
    }

    #region Map Generation
    public void GenerateMapMesh(int[,] map, float squareSize)
    {
        ClearMesh();

        Square[,] squareGrid = SquareGrid(map, squareSize);
        for(int x = 0; x < squareGrid.GetLength(0); x++)
        {
            for(int y = 0; y < squareGrid.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid[x, y]);
            }
        }
    }
    private void TriangulateSquare(Square square)
    {
        switch(square.configuration)
        {
            case 0:
                break;

            //1 Point
            case 1:
                MeshFromPoints(square.centerLeft, square.centerBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centerBottom, square.centerRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centerRight, square.centerTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                break;

            //2 Points
            case 3:
                MeshFromPoints(square.centerRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft, square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            //3 Points
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.bottomRight, square.bottomLeft, square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centerBottom, square.centerLeft);
                break;

            //4 Points
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                break;
        }
    }

    private void MeshFromPoints(params AVeryStrangelyNamedNode[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2]);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3]);
        if (points.Length >= 5)
            CreateTriangle(points[0], points[3], points[4]);
        if (points.Length >= 6)
            CreateTriangle(points[0], points[4], points[5]);
    }
    private void AssignVertices(AVeryStrangelyNamedNode[] points)
    {
        for(int i = 0; i < points.Length; i++)
        {
            if(points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = AddVertex(points[i].position);
            }
        }
    }
    private void CreateTriangle(AVeryStrangelyNamedNode a, AVeryStrangelyNamedNode b, AVeryStrangelyNamedNode c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Maps a value from one range to another
    /// </summary>
    /// <param name="value">Value to be mapped</param>
    /// <param name="originFrom">Original range start</param>
    /// <param name="originTo">Original range end</param>
    /// <param name="remappedFrom">Mapped range start</param>
    /// <param name="remappedTo">Mapped range end</param>
    /// <returns>Mapped value</returns>
    public static float Remap(float value, float originFrom, float originTo, float remappedFrom, float remappedTo)
    {
        return (value - originFrom) / originTo * (remappedTo - remappedFrom) + remappedFrom;
    }
    /// <summary>
    /// Converts longitude and latitude values to their respective vertex points on a supershape
    /// </summary>
    /// <param name="lon">Degrees longitude in radians</param>
    /// <param name="lat">Degrees latitude in radians</param>
    /// <param name="radius">Radius of sphere</param>
    /// <returns>Vertex point on supershape</returns>
    public static Vector3 DegreesToVector(float lon, float lat, float radius, float lonRadius, float latRadius)
    {
        //float r1 = Supershape(lon, 7f, 1, 3, 0), r2 = Supershape(lat, 7f, 1, 1, 3);
        float x = radius * lonRadius * Mathf.Cos(lon) * latRadius * Mathf.Cos(lat);
        float y = radius * latRadius * Mathf.Sin(lat);
        float z = radius * lonRadius * Mathf.Sin(lon) * latRadius * Mathf.Cos(lat);
        return new Vector3(x, y, z);
    }
    public static float Supershape(float theta, float angleMultiplier, float totalPower, float cosinePower, float sinePower)
    {
        float a = 1, b = 1;
        float equationPart1 = Mathf.Abs((1 / a) * Mathf.Cos(angleMultiplier * theta / 4));
        equationPart1 = Mathf.Pow(equationPart1, cosinePower);
        float equationPart2 = Mathf.Abs((1 / b) * Mathf.Sin(angleMultiplier * theta / 4));
        equationPart2 = Mathf.Pow(equationPart2, sinePower);
        return Mathf.Pow(equationPart1 + equationPart2, -1 / totalPower);
    }
    public static Square[,] SquareGrid(int[,] map, float squareSize)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        float mapWidth = nodeCountX * squareSize;
        float mapHeight = nodeCountY * squareSize;

        ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
        for(int x = 0; x < controlNodes.GetLength(0); x++)
        {
            for(int y = 0; y < controlNodes.GetLength(1); y++)
            {
                Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, -mapHeight / 2 + y * squareSize + squareSize / 2);
                controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
            }
        }

        Square[,] squares = new Square[nodeCountX - 1, nodeCountY - 1];
        for(int x = 0; x < squares.GetLength(0); x++)
        {
            for(int y = 0; y < squares.GetLength(1); y++)
            {
                squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
            }
        }

        return squares;
    }
    #endregion
}

public class Square
{
    public ControlNode topLeft, topRight, bottomRight, bottomLeft;
    public AVeryStrangelyNamedNode centerTop, centerRight, centerBottom, centerLeft;
    public int configuration;

    public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
    {
        topLeft = _topLeft;
        topRight = _topRight;
        bottomRight = _bottomRight;
        bottomLeft = _bottomLeft;

        centerTop = topLeft.right;
        centerRight = bottomRight.above;
        centerBottom = bottomLeft.right;
        centerLeft = bottomLeft.above;

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
public class AVeryStrangelyNamedNode
{
    public Vector3 position;
    public int vertexIndex = -1;

    public AVeryStrangelyNamedNode(Vector3 _pos)
    {
        position = _pos;
    }
}

public class ControlNode : AVeryStrangelyNamedNode
{
    public bool active;
    public AVeryStrangelyNamedNode above, right;

    public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
    {
        active = _active;
        above = new AVeryStrangelyNamedNode(position + Vector3.up * squareSize / 2f);
        right = new AVeryStrangelyNamedNode(position + Vector3.right * squareSize / 2f);
    }
}