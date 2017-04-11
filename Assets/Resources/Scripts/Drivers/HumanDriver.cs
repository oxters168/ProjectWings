using Rewired;

public class HumanDriver : Driver
{
    private int playerID = -1;
    public Player rewiredPlayer { get; private set; }

    void Update()
    {
        GetInput();
    }

    public void SetPlayer(int id)
    {
        if (id >= 0 && id < ReInput.players.playerCount)
        {
            playerID = id;
            rewiredPlayer = ReInput.players.GetPlayer(playerID);
        }
    }
    private void GetInput()
    {
        if (rewiredPlayer != null)
        {
            vehicle.inputRightStick = new UnityEngine.Vector2(rewiredPlayer.GetAxis("RightStickX"), rewiredPlayer.GetAxis("RightStickY"));
            vehicle.inputLeftStick = new UnityEngine.Vector2(rewiredPlayer.GetAxis("LeftStickX"), rewiredPlayer.GetAxis("LeftStickY"));
            //vehicle.inputRotation = rewiredPlayer.GetAxis("Horizontal");
            vehicle.inputThruster = rewiredPlayer.GetAxis("RightTrigger");
            vehicle.selection1 = rewiredPlayer.GetButtonDown("Square");
            vehicle.selection2 = rewiredPlayer.GetButtonDown("Triangle");
            vehicle.selection3 = rewiredPlayer.GetButtonDown("Circle");
            vehicle.inputFire = rewiredPlayer.GetButton("Cross");

            //if (rewiredPlayer.GetButtonDown("UICancel")) vehicle.mapController.gameMaster.TogglePause();
        }
    }
}
