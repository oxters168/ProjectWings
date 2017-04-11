using UnityEngine;
using System.Collections;

public class MapBlueprints : MonoBehaviour
{
    public int width { get; private set; }
    public int height { get; private set; }
    public int depth { get; private set; }
    public Vector3 gridUnit, rotUnit;
    private BuildingBlock[,,] blueprints;

    private GameObject[,,] blocks;
    public GameObject combinedBlocks { get; private set; }

	void Start ()
    {
	
	}
	
	void Update ()
    {
	
	}

    public void PrepareMap(int _width, int _height, int _depth)
    {
        width = _width;
        height = _height;
        depth = _depth;
        blueprints = new BuildingBlock[width, height, depth];
    }
    public BuildingBlock GetBlock(int x, int y, int z)
    {
        if (blueprints != null &&  x < width && y < height && z < depth)
        {
            return blueprints[x, y, z];
        }
        return null;
    }
    public void SetBlock(int blockID, int x, int y, int z, Quaternion rotation)
    {
        if (blueprints != null && x < width && y < height && z < depth)
        {

        }
    }
}
