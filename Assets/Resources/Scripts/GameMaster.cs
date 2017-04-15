using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public readonly string[] enths = new string[] { "th", "st", "nd", "rd", "th", "th", "th", "th", "th", "th", "th", "th", "th", "th", "th", "th", "th", "th", "th", "th" };
    public VehicleController[] vehiclePrefabs;
    public GameObject[] mapPrefabs;
    public Camera followCameraPrefab;
    //public VehicleStatsController vehicleStatsUIPrefab;
    public GameObject vehicleHolsterPrefab; //Move to MapController
    public PowerupBox powerupBoxPrefab; //Move to MapController
    //public ParticleSystem smokePrefab, firePrefab;
    //public ExplosionController explosionPrefab;

    //public GarageController garage;

    public GameObject pauseMenu;
    public bool paused { get; private set; }
    public bool mouseInPlay;

    public bool splitscreen;
    public bool ingame { get; private set; }
    public GameMode gameMode;
    public MapController mapInPlay;
    public VehicleController[] shipsInPlay;
    private List<VehicleController> matchResults;
    public int startFreezeTime = 3;

    public int laps = 1;
    private int waypointsToWin;

    public AstarPath aStar;

    public int humans, bots;

    void Update()
    {
        if (ingame && Rewired.ReInput.players.SystemPlayer.GetButtonDown("Pause")) TogglePause();
        //StartCoroutine(PauseChecker());
    }

    /*private IEnumerator PauseChecker()
    {
        if(ingame)
        {
            if (Rewired.ReInput.players.SystemPlayer.GetButtonDown("UICancel")) TogglePause();
            foreach(ShipController ship in shipsInPlay)
            {
                if(ship.driver is HumanDriver)
                {
                    if (((HumanDriver)ship.driver).rewiredPlayer.GetButtonDown("UICancel")) TogglePause();
                }
                yield return null;
            }
        }
    }*/
    public void StartGame()
    {
        mapInPlay = Instantiate(Resources.Load<MapController>("Prefabs/Maps/TestMap"));
        mapInPlay.transform.parent = transform;
        mapInPlay.gameMaster = this;

        matchResults = new List<VehicleController>();
        if (gameMode == GameMode.Race || gameMode == GameMode.Team_Race) waypointsToWin = mapInPlay.waypointCount * laps + 1;

        aStar.Scan();

        SetPlayers(humans, bots);
        StartCoroutine(StartFreeze());

        Cursor.visible = false;
        Cursor.lockState = mouseInPlay ? CursorLockMode.Locked : CursorLockMode.None;
        ingame = true;
    }
    private IEnumerator StartFreeze()
    {
        if (startFreezeTime > 0)
        {
            FreezeAllContestants(true);
            yield return new WaitForSeconds(startFreezeTime);
            FreezeAllContestants(false);
        }
    }
    public void FreezeAllContestants(bool freeze)
    {
        if (shipsInPlay != null)
        {
            foreach (VehicleController contestant in shipsInPlay)
            {
                if (contestant) contestant.Freeze(freeze);
            }
        }
    }
    private void SetPlayers(int humanPlayers, int cpuPlayers)
    {
        bool checkWaypoints = false;
        if (gameMode == GameMode.Race || gameMode == GameMode.Team_Race) checkWaypoints = true;

        List<Transform> cameraTargets = new List<Transform>();
        Camera playerCamera = Instantiate(followCameraPrefab);

        shipsInPlay = new VehicleController[humanPlayers + cpuPlayers];
        int shipIndex = 0;
        int vehiclePrefabIndex = 1;
        for(int i = 0; i < humanPlayers; i++)
        {
            VehicleController humanShip = Instantiate(vehiclePrefabs[vehiclePrefabIndex++ % vehiclePrefabs.Length]);
            humanShip.transform.parent = transform;

            cameraTargets.Add(humanShip.transform);
            playerCamera.GetComponent<CameraController>().targets = cameraTargets.ToArray();
            if (splitscreen)
            {
                playerCamera.transform.parent = transform;
                playerCamera.rect = new Rect(CalculateCamRect(humanPlayers, i));
                cameraTargets = new List<Transform>();
                playerCamera = Instantiate(followCameraPrefab);
            }

            humanShip.mapController = mapInPlay;
            humanShip.followingCamera = playerCamera.GetComponent<CameraController>();
            humanShip.driver = humanShip.gameObject.AddComponent<HumanDriver>();
            ((HumanDriver)humanShip.driver).SetPlayer(i);

            if (checkWaypoints) humanShip.driver.waypointCleared += WaypointCleared;

            mapInPlay.AddContestant(humanShip);
            shipsInPlay[shipIndex++] = humanShip;
        }
        for(int i = 0; i < cpuPlayers; i++)
        {
            VehicleController aiShip = Instantiate(vehiclePrefabs[vehiclePrefabIndex++ % vehiclePrefabs.Length]);
            aiShip.transform.parent = transform;

            aiShip.mapController = mapInPlay;
            aiShip.driver = aiShip.gameObject.AddComponent<AIDriver>();

            if (checkWaypoints) aiShip.driver.waypointCleared += WaypointCleared;

            mapInPlay.AddContestant(aiShip);
            shipsInPlay[shipIndex++] = aiShip;
        }
    }
    private Rect CalculateCamRect(int screens, int screenIndex)
    {
        int square = Mathf.CeilToInt(Mathf.Sqrt(screens));
        int rows = Mathf.CeilToInt((float)screens / square);
        int columnsLow = screens / rows, columnsHigh = Mathf.CeilToInt((float)screens / rows);
        int remainder = rows * columnsHigh - screens;

        int maxLowColIndex = columnsLow * remainder;
        int col = screenIndex < maxLowColIndex ? screenIndex % columnsLow : (screenIndex - maxLowColIndex) % columnsHigh;
        int row = screenIndex < maxLowColIndex ? screenIndex / columnsLow : remainder + (screenIndex - maxLowColIndex) / columnsHigh;

        float width = screenIndex < maxLowColIndex ? 1f / columnsLow : 1f / columnsHigh, height = 1f / rows;

        return new Rect(col * width, ((rows - 1) * height) - row * height, width, height);
    }
    public void EndGame()
    {
        ingame = false;
        paused = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        DestroyContestants();
        Destroy(mapInPlay.gameObject);

        Time.timeScale = paused ? 0 : 1;
    }
    private void DestroyContestants()
    {
        if (shipsInPlay != null)
        {
            for (int i = 0; i < shipsInPlay.Length; i++)
            {
                //shipsInPlay[i].DestroyAllPowerups();
                shipsInPlay[i].driver.waypointCleared -= WaypointCleared;
                shipsInPlay[i].Dispose();
                //if (shipsInPlay[i].followingCamera) Destroy(shipsInPlay[i].followingCamera.gameObject);
                //Destroy(shipsInPlay[i].gameObject);
            }
        }
        shipsInPlay = null;
    }

    public void TogglePause()
    {
        paused = !paused;
        pauseMenu.SetActive(paused);
        Cursor.visible = paused;
        Cursor.lockState = mouseInPlay ? (paused ? CursorLockMode.None : CursorLockMode.Locked) : CursorLockMode.None;
        Time.timeScale = paused ? 0 : 1;
        //rewiredPlayer.controllers.maps.SetMapsEnabled(!paused, "ShipNavigation");
    }

    public void WaypointCleared(Driver driver, Transform waypoint)
    {
        if (driver.waypointsCleared >= waypointsToWin && !matchResults.Contains(driver.vehicle))
        {
            matchResults.Add(driver.vehicle);
            if(driver.vehicle.followingCamera)
            {
                int place = matchResults.IndexOf(driver.vehicle) + 1;
                driver.vehicle.followingCamera.resultText.text = place + enths[place < 20 ? place : place % 10];
            }
        }
    }
}
public enum GameMode { Race, Team_Race, Battle, Team_Battle, CTF, Team_CTF, KotH, Team_KotH, Tag, Team_Tag, }