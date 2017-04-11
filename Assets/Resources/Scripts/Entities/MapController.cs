using UnityEngine;

public class MapController : MonoBehaviour
{
    public GameMaster gameMaster;

    public Transform startingPointPositions, powerupPositions, waypointPositions, flagPositions, hillPositions;
    public int contestantCapacity { get { if (startingPoints == null) PrepareStartingPoints();  return startingPoints.Length; } }
    public int startPointsTaken { get; private set; }

    public int waypointCount { get { if (waypoints == null) PrepareWaypoints(); return waypoints.Length; } }
    private Transform[] startingPoints, waypoints, flags, hills;
    private PowerupBox[] powerupBoxes;
    private Transform powerupBoxesParent;

    private int startingPointIndex = 0;
	
    void Start()
    {
        Prepare();
    }
    private void Prepare()
    {
        PreparePowerupBoxes();
        PrepareFlags();
        PrepareHills();
    }
    private void PrepareStartingPoints()
    {
        startingPoints = new Transform[startingPointPositions.childCount];
        for (int i = 0; i < startingPoints.Length; i++)
        {
            startingPoints[i] = startingPointPositions.GetChild(i);
            if (gameMaster && gameMaster.vehicleHolsterPrefab)
            {
                GameObject vehicleHolster = Instantiate(gameMaster.vehicleHolsterPrefab);
                vehicleHolster.transform.parent = startingPoints[i];
                vehicleHolster.transform.localPosition = Vector3.zero;
            }
        }
    }
    private void PreparePowerupBoxes()
    {
        powerupBoxesParent = new GameObject("PowerupBoxes").transform;
        powerupBoxesParent.parent = transform;

        if (gameMaster && gameMaster.powerupBoxPrefab)
        {
            powerupBoxes = new PowerupBox[powerupPositions.childCount];
            for (int i = 0; i < powerupBoxes.Length; i++)
            {
                powerupBoxes[i] = Instantiate(gameMaster.powerupBoxPrefab);
                powerupBoxes[i].transform.position = powerupPositions.GetChild(i).position;
                powerupBoxes[i].transform.parent = powerupBoxesParent;
            }
        }
    }
    private void PrepareWaypoints()
    {
        waypoints = new Transform[waypointPositions.childCount];
        for (int i = 0; i < waypoints.Length; i++) waypoints[i] = waypointPositions.GetChild(i);
    }
    private void PrepareFlags()
    {

    }
    private void PrepareHills()
    {

    }
    public Transform GetWaypoint(int index)
    {
        if (waypoints == null) PrepareWaypoints();
        if (index >= 0 && waypoints != null && index < waypoints.Length)
        {
            return waypoints[index];
        }
        return null;
    }

    public void AddContestant(params VehicleController[] addedContestants)
    {
        if (startingPoints == null) PrepareStartingPoints();

        foreach(VehicleController ship in addedContestants)
        {
            if (startingPointIndex >= startingPoints.Length) break;
            ship.transform.position = startingPoints[startingPointIndex++].position;
        }
    }
}
