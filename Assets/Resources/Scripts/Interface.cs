using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Rewired;
using System.Linq;
using System;

public class Interface : MonoBehaviour
{
    public GameObject[] defaultSelections;
    public GameMaster gameMaster;

    #region Player Management
    public int maxPlayers = 16;
    private Controller[] controllersInPrompts;
    public Sprite joystickIcon, keyboardIcon, mouseIcon;
    public Button addPlayerButtonPrefab, playerManagementContinueButton;
    private Button currentAddPlayerButton;
    public PlayerPrompt playerPromptPrefab;
    private List<PlayerPrompt> playerPrompts;
    public GameObject playerBowlContent;
    #endregion

    #region Options
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;
    #endregion

    void Start ()
    {
        ReInput.ControllerConnectedEvent += ReInput_ControllerConnectedEvent;
        ReInput.ControllerDisconnectedEvent += ReInput_ControllerDisconnectedEvent;
        LoadSettings();
	}
    void Update()
    {
        CheckSelection();
    }

    private void ReInput_ControllerDisconnectedEvent(ControllerStatusChangedEventArgs obj)
    {
        RefreshPlayerPrompts();
    }
    private void ReInput_ControllerConnectedEvent(ControllerStatusChangedEventArgs obj)
    {
        ReInput.players.SystemPlayer.controllers.AddController(ReInput.controllers.GetController(obj.controllerType, obj.controllerId), false);
        RefreshPlayerPrompts();
    }
    private void CheckSelection()
    {
        if (!EventSystem.current.currentSelectedGameObject || !EventSystem.current.currentSelectedGameObject.activeInHierarchy)
        {
            foreach (GameObject defaultSelection in defaultSelections)
            {
                if (defaultSelection.activeInHierarchy) { EventSystem.current.SetSelectedGameObject(defaultSelection); break; }
            }
        }
    }

    #region Player Management
    public void AddPlayerPrompt()
    {
        if (playerPrompts == null) playerPrompts = new List<PlayerPrompt>();

        PlayerPrompt playerPrompt = Instantiate(playerPromptPrefab);
        playerPrompt.removePlayerButton.onClick.AddListener(OnRemovePlayerClicked);
        playerPrompt.inputDropdown.onValueChanged.AddListener(OnControllerDropdownValueChanged);
        playerPrompt.transform.SetParent(playerBowlContent.transform, false);

        if (playerPrompts.Count <= 0) playerPrompt.removePlayerButton.interactable = false;
        playerPrompts.Add(playerPrompt);
        RefreshPlayerPrompts();

        playerManagementContinueButton.interactable = AllPlayersPickedControllers();
    }
    public void RemoveAllPlayerPrompts()
    {
        for (int i = playerPrompts.Count - 1; i >= 0; i--)
        {
            Destroy(playerPrompts[i].gameObject);
        }
        playerPrompts.Clear();
        playerPrompts = null;
    }
    private void OnRemovePlayerClicked()
    {
        PlayerPrompt playerPrompt = EventSystem.current.currentSelectedGameObject.GetComponentInParent<PlayerPrompt>();
        if(playerPrompt)
        {
            if (playerPrompts.Contains(playerPrompt)) playerPrompts.Remove(playerPrompt);
            Destroy(playerPrompt.gameObject);
            RefreshPlayerPrompts();
        }

        playerManagementContinueButton.interactable = AllPlayersPickedControllers();
    }
    private void OnControllerDropdownValueChanged(int value)
    {
        CheckInputSelection(EventSystem.current.currentSelectedGameObject.GetComponentInParent<PlayerPrompt>());
    }
    private void CheckInputSelection(PlayerPrompt prompt)
    {
        if (prompt && prompt.inputDropdown)
        {
            int value = prompt.inputDropdown.value;
            if (value - 1 < controllersInPrompts.Length)
            {
                if (prompt)
                {
                    RefreshLayoutDropdown(prompt);

                    prompt.inputTypeImage.color = new Color(0, 0, 0, 1);
                    if (value <= 0)
                    {
                        prompt.inputTypeImage.color = new Color(0, 0, 0, 0);
                    }
                    else if (controllersInPrompts[value - 1].type == ControllerType.Keyboard)
                    {
                        prompt.inputTypeImage.overrideSprite = keyboardIcon;
                    }
                    else if (controllersInPrompts[value - 1].type == ControllerType.Mouse)
                    {
                        prompt.inputTypeImage.overrideSprite = mouseIcon;
                    }
                    else
                    {
                        prompt.inputTypeImage.overrideSprite = joystickIcon;
                    }
                }
            }
        }

        playerManagementContinueButton.interactable = AllPlayersPickedControllers();
    }
    public void RefreshPlayerPrompts()
    {
        if (playerPrompts != null)
        {
            #region Add AddPlayerButton?
            if (currentAddPlayerButton) Destroy(currentAddPlayerButton.gameObject);
            if (playerPrompts.Count < maxPlayers)
            {
                currentAddPlayerButton = Instantiate(addPlayerButtonPrefab);
                currentAddPlayerButton.onClick.AddListener(AddPlayerPrompt);
                currentAddPlayerButton.transform.SetParent(playerBowlContent.transform, false);
            }
            #endregion

            Controller[] previousControllers = controllersInPrompts;
            controllersInPrompts = ReInput.controllers.Controllers.ToArray();
            List<string> controllerNames = new List<string>();
            controllerNames.Add("None");
            foreach (Controller controller in controllersInPrompts)
            {
                controllerNames.Add(controller.name);
            }

            for (int i = 0; i < playerPrompts.Count; i++)
            {
                PlayerPrompt playerPrompt = playerPrompts[i];
                playerPrompt.playerText.text = "Player " + (i > 8 ? "" : "0") + (i + 1);

                int selectionIndex = -1;
                #region Check Controller Selection
                if (playerPrompt.inputDropdown.value > 0)
                {
                    Controller inputSelection = previousControllers[playerPrompt.inputDropdown.value - 1];
                    
                    if (inputSelection.isConnected)
                    {
                        for(int j = 0; j < controllersInPrompts.Length; j++)
                        {
                            if(controllersInPrompts[j] == inputSelection) { selectionIndex = j; break; }
                        }
                    }
                    //Debug.Log("Index Found: " + selectionIndex);
                }
                #endregion
                playerPrompt.inputDropdown.ClearOptions();
                playerPrompt.inputDropdown.AddOptions(controllerNames);

                playerPrompt.inputDropdown.value = selectionIndex + 1;
                CheckInputSelection(playerPrompt);
            }
        }
    }
    public void RefreshLayoutDropdown(PlayerPrompt playerPrompt)
    {
        if(playerPrompt.inputDropdown && playerPrompt.layoutDropdown)
        {
            playerPrompt.layoutDropdown.value = 0;
            playerPrompt.layoutDropdown.ClearOptions();
            if(playerPrompt.inputDropdown.value > 0 && playerPrompt.inputDropdown.value - 1 < controllersInPrompts.Length)
            {
                Controller controller = controllersInPrompts[playerPrompt.inputDropdown.value - 1];
                IList<InputLayout> layouts;
                if (controller.type == ControllerType.Joystick) layouts = ReInput.mapping.JoystickLayouts;
                else if (controller.type == ControllerType.Keyboard) layouts = ReInput.mapping.KeyboardLayouts;
                else if (controller.type == ControllerType.Mouse) layouts = ReInput.mapping.MouseLayouts;
                else layouts = ReInput.mapping.CustomControllerLayouts;

                List<string> layoutNames = new List<string>();
                foreach(InputLayout layout in layouts)
                {
                    if (ReInput.mapping.GetControllerMapInstance(controller, "VehicleActions", layout.name).elementMapCount > 0)
                        layoutNames.Add(layout.name);
                }
                playerPrompt.layoutDropdown.AddOptions(layoutNames);
            }

            playerPrompt.layoutDropdown.interactable = playerPrompt.layoutDropdown.options.Count > 0;
        }
    }
    public void RefreshPlayerBowlCellSize()
    {
        #region Scale Bowl Cells
        Bounds bowlBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(playerBowlContent.transform);
        float bowlCellSize = 120;
        if (bowlBounds.extents.x * 2 < (120 * 8 + 5 * 7) || bowlBounds.extents.y * 2 < (120 * 2 + 5))
        {
            float maxWidth = (bowlBounds.extents.x - 5 * 7) * 2 / 8, maxHeight = (bowlBounds.extents.y - 5) * 2 / 2;
            bowlCellSize = Mathf.Min(maxWidth, maxHeight);
        }
        playerBowlContent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(bowlCellSize, bowlCellSize);
        #endregion
    }
    public bool AllPlayersPickedControllers()
    {
        bool allPicked = true;
        foreach(PlayerPrompt playerPrompt in playerPrompts)
        {
            if (playerPrompt.inputDropdown.value == 0) { allPicked = false; break; }
        }
        return allPicked;
    }
    public void DistributeControllers()
    {
        //Debug.Log("Distributing Controllers to Players");
        for (int i = 0; i < playerPrompts.Count; i++)
        {
            int controllerIndex = playerPrompts[i].inputDropdown.value - 1;
            if (controllerIndex >= 0 && controllerIndex < controllersInPrompts.Length)
            {
                Controller controller = controllersInPrompts[controllerIndex];
                if (controller.type == ControllerType.Mouse) gameMaster.mouseInPlay = true;
                Player player = ReInput.players.GetPlayer(i);
                player.controllers.AddController(controller, false);
                player.controllers.maps.LoadMap(controller.type, controller.id, "VehicleActions", playerPrompts[i].layoutDropdown.options[playerPrompts[i].layoutDropdown.value].text);
                player.controllers.maps.SetAllMapsEnabled(true);

                //DebugPlayerControllers(player);
            }
        }
        gameMaster.humans = playerPrompts.Count;

        //DebugPlayerControllers(ReInput.players.SystemPlayer);
    }
    public void ReturnControllersToSystem()
    {
        gameMaster.mouseInPlay = false;
        //Debug.Log("Returning Controllers to System");
        foreach(Player player in ReInput.players.Players)
        {
            player.controllers.maps.ClearAllMaps(false);
            player.controllers.ClearAllControllers();
            //DebugPlayerControllers(player);
        }
        //DebugPlayerControllers(ReInput.players.SystemPlayer);
    }
    public void DebugPlayerControllers(Player player)
    {
        string systemControllerDebug = player.name + " Controllers (";
        foreach (Controller controller in player.controllers.Controllers)
        {
            systemControllerDebug += controller.name + ", ";
        }
        Debug.Log(systemControllerDebug + ")");
    }
    #endregion

    #region Options
    public void LoadSettings()
    {
        fullscreenToggle.isOn = Screen.fullScreen;
        int resolutionIndex = -1;
        List<string> resolutions = new List<string>();
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            Resolution currentResolution = Screen.resolutions[i];
            resolutions.Add(currentResolution.ToString());
            if (Screen.width == currentResolution.width && Screen.height == currentResolution.height) resolutionIndex = i;
        }
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutions);
        resolutionDropdown.value = resolutionIndex;
    }
    public void SaveSettings()
    {
        Resolution selectedResolution = Screen.resolutions[resolutionDropdown.value];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreenToggle.isOn, selectedResolution.refreshRate);
    }
    #endregion

    public void Exit()
    {
        Application.Quit();
    }
}
