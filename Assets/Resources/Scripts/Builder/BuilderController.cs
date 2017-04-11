using UnityEngine;
using Rewired;

public class BuilderController : MonoBehaviour
{
    public int playerID;
    private Player rewiredPlayer;

    public CameraController cameraController;
    private const string BLOCKS_LOCATION = "Prefabs/BuildingBlocks/";
    public Transform buildingBlocksHolder;
    //public Transform builtBlocksHolder { get; private set; }
    private int buildingBlockIndex = -1;

    [System.Flags]
    private enum Direction { None = 0, Right = 0x1, Left = 0x2, Up = 0x4, Down = 0x8, Forward = 0x10, Backward = 0x20, }
    public int x, y, z;//, width, height, depth;
    private Vector3 euler;
    //public Vector3 gridUnit, rotUnit;
    public GameObject[] buildingBlocks;
    private MapBlueprints builtMap;
    //private BuildingBlock[,,] builtBlocks;

    public GameObject pauseMenu;
    private bool paused;

	void Start ()
    {
        UpdatePlayer();
        RefreshAvailableBuildingBlocks();
        ResetMap(50, 50, 7);
        Move(Direction.None);
	}
	
	void FixedUpdate ()
    {
        ApplyInput();
	}

    public void UpdatePlayer()
    {
        if (playerID >= 0 && playerID < ReInput.players.playerCount)
            rewiredPlayer = ReInput.players.GetPlayer(playerID);
    }
    private void ApplyInput()
    {
        Direction positionDirection = Direction.None;
        if (rewiredPlayer.GetButtonDown("BuilderMoveLeft") || rewiredPlayer.GetButtonShortPress("BuilderMoveLeft")) positionDirection |= Direction.Left;
        if (rewiredPlayer.GetButtonDown("BuilderMoveRight") || rewiredPlayer.GetButtonShortPress("BuilderMoveRight")) positionDirection |= Direction.Right;
        if (rewiredPlayer.GetButtonDown("BuilderMoveUp") || rewiredPlayer.GetButtonShortPress("BuilderMoveUp")) positionDirection |= Direction.Up;
        if (rewiredPlayer.GetButtonDown("BuilderMoveDown") || rewiredPlayer.GetButtonShortPress("BuilderMoveDown")) positionDirection |= Direction.Down;
        if (rewiredPlayer.GetButtonDown("BuilderMoveForward") || rewiredPlayer.GetButtonShortPress("BuilderMoveForward")) positionDirection |= Direction.Forward;
        if (rewiredPlayer.GetButtonDown("BuilderMoveBackward") || rewiredPlayer.GetButtonShortPress("BuilderMoveBackward")) positionDirection |= Direction.Backward;
        Move(positionDirection);

        Direction rotationDirection = Direction.None;
        if (rewiredPlayer.GetButtonDown("BuilderRotateYawC") || rewiredPlayer.GetButtonShortPress("BuilderRotateYawC")) rotationDirection |= Direction.Down;
        if (rewiredPlayer.GetButtonDown("BuilderRotateYawCC") || rewiredPlayer.GetButtonShortPress("BuilderRotateYawCC")) rotationDirection |= Direction.Up;
        Rotate(rotationDirection);

        if (rewiredPlayer.GetButtonDown("BuilderNextItem") || rewiredPlayer.GetButtonShortPress("BuilderNextItem")) NextBuildingBlock();
        if (rewiredPlayer.GetButtonDown("BuilderPreviousItem") || rewiredPlayer.GetButtonShortPress("BuilderPreviousItem")) PreviousBuildingBlock();

        if (rewiredPlayer.GetButtonDown("BuilderPlace") || rewiredPlayer.GetButtonShortPress("BuilderPlace")) PlaceBuildingBlock();
        if (rewiredPlayer.GetButtonDown("BuilderDelete") || rewiredPlayer.GetButtonShortPress("BuilderDelete")) DeleteBuildingBlock();

        Vector3 rotationEuler = new Vector3(rewiredPlayer.GetAxis("OrbitPitch"), rewiredPlayer.GetAxis("OrbitYaw"), 0);
        cameraController.RotateCamera(rotationEuler);

        if (rewiredPlayer.GetButtonDown("MenuCancel")) TogglePause();
    }
    public void TogglePause()
    {
        paused = !paused;
        pauseMenu.SetActive(paused);
        rewiredPlayer.controllers.maps.SetMapsEnabled(!paused, "BuilderNavigation");
    }

    private void Move(Direction direction)
    {
        if ((direction & Direction.Left) != 0 && x > 0) x--;
        if ((direction & Direction.Right) != 0 && x < builtMap.width - 1) x++;
        if ((direction & Direction.Up) != 0 && y < builtMap.height - 1) y++;
        if ((direction & Direction.Down) != 0 && y > 0) y--;
        if ((direction & Direction.Forward) != 0 && z < builtMap.depth - 1) z++;
        if ((direction & Direction.Backward) != 0 && z > 0) z--;
        buildingBlocksHolder.position = transform.right * x * builtMap.gridUnit.x + transform.up * y * builtMap.gridUnit.y + transform.forward * z * builtMap.gridUnit.z;
    }
    private void Rotate(Direction direction)
    {
        if ((direction & Direction.Down) != 0) euler.y += builtMap.rotUnit.y;
        if ((direction & Direction.Up) != 0) euler.y -= builtMap.rotUnit.y;
        buildingBlocksHolder.rotation = Quaternion.Euler(euler);
    }

    public void RefreshAvailableBuildingBlocks()
    {
        if (!buildingBlocksHolder)
        {
            buildingBlocksHolder = new GameObject("BuildingBlocks").transform;
            buildingBlocksHolder.parent = transform;
        }

        buildingBlocks = Resources.LoadAll<GameObject>(BLOCKS_LOCATION);
        for(int i = 0; i < buildingBlocks.Length; i++)
        {
            buildingBlocks[i] = Instantiate(buildingBlocks[i]);
            buildingBlocks[i].transform.parent = buildingBlocksHolder;
            buildingBlocks[i].SetActive(false);
        }
        SetBuildingBlockIndex(0);
    }
    private void ResetMap(int w, int h, int d)
    {
        if (builtMap) Destroy(builtMap.gameObject);
        builtMap = new GameObject("Custom Map").AddComponent<MapBlueprints>();
        builtMap.transform.parent = transform;

        builtMap.PrepareMap(w, h, d);

        //builtBlocks = new BuildingBlock[width, height, depth];
    }
    public bool SetBuildingBlockIndex(int index)
    {
        if(buildingBlocks != null && index >= 0 && index < buildingBlocks.Length)
        {
            if (buildingBlockIndex >= 0 && buildingBlockIndex < buildingBlocks.Length) buildingBlocks[buildingBlockIndex].SetActive(false);
            buildingBlockIndex = index;
            buildingBlocks[buildingBlockIndex].SetActive(true);
            return true;
        }
        else if(index == -1)
        {
            if (buildingBlockIndex >= 0 && buildingBlockIndex < buildingBlocks.Length) buildingBlocks[buildingBlockIndex].SetActive(false);
        }
        buildingBlockIndex = -1;
        return false;
    }
    public void NextBuildingBlock()
    {
        if (buildingBlocks.Length > 0)
        {
            int nextIndex = buildingBlockIndex + 1;
            if (nextIndex >= buildingBlocks.Length) nextIndex = 0;
            SetBuildingBlockIndex(nextIndex);
        }
    }
    public void PreviousBuildingBlock()
    {
        if (buildingBlocks.Length > 0)
        {
            int previousIndex = buildingBlockIndex - 1;
            if (previousIndex < 0) previousIndex = buildingBlocks.Length - 1;
            SetBuildingBlockIndex(previousIndex);
        }
    }
    public void PlaceBuildingBlock()
    {
        /*if (buildingBlocks.Length > 0 && x < builtMap.width && y < builtMap.height && z < builtMap.depth && !builtMap.GetBlock(x, y, z).block)
        {
            GameObject builtBlock = Instantiate(buildingBlocks[buildingBlockIndex]);
            builtBlock.transform.position = transform.right * x * gridUnit.x + transform.up * y * gridUnit.y + transform.forward * z * gridUnit.z;
            builtBlock.transform.rotation = Quaternion.Euler(euler);
            builtBlock.transform.parent = builtBlocksHolder;
            builtBlocks[x, y, z] = builtBlock.AddComponent<BuildingBlock>();
        }*/
    }
    public void DeleteBuildingBlock()
    {
        /*if (x < builtBlocks.GetLength(0) && y < builtBlocks.GetLength(1) && z < builtBlocks.GetLength(2) && builtBlocks[x, y, z])
        {
            Destroy(builtBlocks[x, y, z].gameObject);
            builtBlocks[x, y, z] = null;
        }*/
    }
}
