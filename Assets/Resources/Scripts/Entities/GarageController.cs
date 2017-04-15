using UnityEngine;
using Rewired;

public class GarageController : MonoBehaviour
{
    public GameMaster gameMaster;
    public Transform vehiclePlaceholder;

    private VehicleController currentViewedVehicle;
    private int vehicleIndex;

    void Start()
    {
        SetViewedVehicle(0);
    }
    void Update()
    {
        Input();
    }

    private void Input()
    {
        if (ReInput.players.SystemPlayer.GetButtonDown("UIHorizontal") || ReInput.players.SystemPlayer.GetButtonShortPressDown("UIHorizontal")) NextVehicle();
        if (ReInput.players.SystemPlayer.GetNegativeButtonDown("UIHorizontal") || ReInput.players.SystemPlayer.GetNegativeButtonShortPressDown("UIHorizontal")) PreviousVehicle();
    }

    public void NextVehicle()
    {
        vehicleIndex++;
        if (vehicleIndex >= gameMaster.vehiclePrefabs.Length) vehicleIndex = 0;
        SetViewedVehicle(vehicleIndex);
    }
    public void PreviousVehicle()
    {
        vehicleIndex--;
        if (vehicleIndex < 0) vehicleIndex = gameMaster.vehiclePrefabs.Length - 1;
        SetViewedVehicle(vehicleIndex);
    }
    private void SetViewedVehicle(int index)
    {
        if (currentViewedVehicle) { Destroy(currentViewedVehicle.gameObject); }

        currentViewedVehicle = Instantiate(gameMaster.vehiclePrefabs[index]);
        currentViewedVehicle.GetComponent<Rigidbody2D>().isKinematic = true;
        currentViewedVehicle.transform.position = vehiclePlaceholder.position;
        currentViewedVehicle.transform.rotation = vehiclePlaceholder.rotation;
        currentViewedVehicle.transform.parent = vehiclePlaceholder;
    }
}
