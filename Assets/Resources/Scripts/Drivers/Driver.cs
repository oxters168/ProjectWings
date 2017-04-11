using UnityEngine;

public abstract class Driver : MonoBehaviour
{
    public VehicleController vehicle { get; private set; }
    public Transform target;
    public int waypointIndex { get; protected set; }
    public int waypointsCleared { get; protected set; }

    public delegate void WaypointCleared(Driver driver, Transform waypoint);
    public event WaypointCleared waypointCleared;

    protected virtual void Start ()
    {
        vehicle = GetComponent<VehicleController>();
        target = vehicle.mapController.GetWaypoint(waypointIndex);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == target)
        {
            waypointIndex++;
            if (waypointIndex >= vehicle.mapController.waypointCount) waypointIndex = 0;
            target = vehicle.mapController.GetWaypoint(waypointIndex);
            waypointsCleared++;

            //Debug.Log(name + " entered " + other.name + " total: " + waypointsCleared);

            if (waypointCleared != null) waypointCleared(this, other.transform);
        }
    }
}
