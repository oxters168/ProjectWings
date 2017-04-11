using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshController))]
public class MapGenerator : MonoBehaviour
{
    public bool randomSeed;
    public string seed;
    public float fillPercent;
    public float width;
    public float height;
    public int resolution;
    private int[,] map;

    private MeshController meshController;

	void Start ()
    {
        meshController = GetComponent<MeshController>();
        GenerateMap();
	}
	
	void Update ()
    {
        if (Input.GetMouseButtonDown(0)) GenerateMap();
	}

    /*void OnDrawGizmos()
    {
        if (map != null)
        {
            for(int x = 0; x < resolution; x++)
            {
                for(int y = 0; y < resolution; y++)
                {
                    Gizmos.color = map[x, y] == 1 ? Color.black : Color.white;
                    Vector3 position = new Vector3(-resolution / 2 + x + 0.5f, -resolution / 2 + y + 0.5f, 0);
                    Gizmos.DrawCube(position, Vector3.one);
                }
            }
        }
    }*/

    public void GenerateMap()
    {
        map = new int[resolution, resolution];
        //FillOuterWalls();
        FillMapRandomly();
        for(int i = 0; i < 5; i++)
        {
            Smooth();
        }

        map = AddBorders(map, 2);
        meshController.GenerateMapMesh(map, 2);
    }
    private int[,] AddBorders(int[,] map, int borderSize)
    {
        int[,] borderedMap = new int[map.GetLength(0) + borderSize * 2, map.GetLength(1) + borderSize * 2];

        for(int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for(int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < map.GetLength(0) + borderSize && y >= borderSize && y < map.GetLength(1) + borderSize) borderedMap[x, y] = map[x - borderSize, y - borderSize];
                else borderedMap[x, y] = 1;
            }
        }

        return borderedMap;
    }
    private void FillOuterWalls()
    {
        for(int edgeIndex = 0; edgeIndex < resolution; edgeIndex++)
        {
            map[edgeIndex, 0] = 1;
            map[edgeIndex, resolution - 1] = 1;
            map[0, edgeIndex] = 1;
            map[resolution - 1, edgeIndex] = 1;
        }
    }
    private void FillMapRandomly()
    {
        if (randomSeed) seed = Time.time.ToString();
        System.Random randomNumberGenerator = new System.Random(seed.GetHashCode());

        int outerWallFill = resolution * 2 + (resolution - 2) * 2;
        int amountFilled = 0;
        while ((float)amountFilled / ((resolution * resolution) - outerWallFill) < fillPercent)
        {
            int coordX = randomNumberGenerator.Next(0, resolution), coordY = randomNumberGenerator.Next(0, resolution);
            if (map[coordX, coordY] != 1) amountFilled++;
            map[coordX, coordY] = 1;
        }
    }
    private void Smooth()
    {
        int smoothDistance = 1;
        for(int coordX = 0; coordX < resolution; coordX++)
        {
            for(int coordY = 0; coordY < resolution; coordY++)
            {
                int walls = FilledNeighbours(coordX, coordY, smoothDistance);
                if (walls > (smoothDistance * 8) / 2)
                    map[coordX, coordY] = 1;
                else if (walls < (smoothDistance * 8) / 2)
                    map[coordX, coordY] = 0;
            }
        }
    }
    private int FilledNeighbours(int coordX, int coordY, int distance)
    {
        int neighboursFilled = 0;
        for(int neighbourX = -distance; neighbourX <= distance; neighbourX++)
        {
            for(int neighbourY = -distance; neighbourY <= distance; neighbourY++)
            {
                if (coordX + neighbourX >= 0 && coordX + neighbourX < resolution && coordY + neighbourY >= 0 && coordY + neighbourY < resolution)
                {
                    if ((neighbourX != 0 || neighbourY != 0))
                    {
                        neighboursFilled += map[coordX + neighbourX, coordY + neighbourY];
                    }
                }
                //else neighboursFilled++;
            }
        }
        return neighboursFilled;
    }
}
