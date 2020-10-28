/*---------------------------------------------------------------
                    CHARACTER-CREATOR
 ---------------------------------------------------------------*/
/***************************************************************
* The character-creation game state involves formation of the
  player-party through providing class selection and 
  name selection. While also showing an interactive/dynamic
  display for party member's actions.

* The included functions of this game state are mainly
  event handlers for server driven events.
**************************************************************/
#define REQUIRE_TEST_DATA
#region IMPORTS
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Assets.Scripts.RPA_Messages;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.UI.CharacterCreation;
using SimpleJSON;
using Assets.Scripts.Util;
using System.Collections.Generic;
using System;
#endregion IMPORTS


public class CharacterCreator : MonoBehaviour
{
    #region UI COMPONENTS
    //Player/UI Text
    public Text classChoice;
    public Text classDescription;
    public Text playerNameField;
    public Text gameIdText;
    public Text playerCountText;
    public Text readyText;
    public Button readyButton;
    public Button startButton;
    //Party textures/text
    List<PartyPanel> partyPanels;
    [SerializeField] private GameObject party_vertical_layout;
    #endregion UI COMPONENTS

    //Statically loaded data
    private Texture2D[] classIcons;
    private const string CLASS_DATA_FILE = "Assets/Resources/data/meta_data.json";
    private string[] classDescriptions;
    private string[] classNames;

    /*---------------------------------------------------------------
                       GAME STATE INIATIALISATIONS
     ---------------------------------------------------------------*/
    /***************************************************************
    * Called on Scene initialization
    **************************************************************/
    void Awake()
    {
        if(TestSimulator.isDeveloping)
        {
            TestSimulator.initTestEnvironment(GameState.CHARACTER_CREATION);
        }

        AssetLoader.loadStaticAssets(GameState.CHARACTER_CREATION);
        partyPanels = new List<PartyPanel>();
        initClassData();
    }

    void Start()
    {
        initPlayerUI();
        generatePlayerPanels();
    }

    /***************************************************************
    *  Parses json file for class data and populates icon textures
    **************************************************************/
    private void initClassData()
    {
        string classJSON = File.ReadAllText(CLASS_DATA_FILE);

        JSONNode json = JSON.Parse(classJSON);
        JSONArray classes = json["classes"].AsArray;

        int classCount = classes.Count;
        classIcons = new Texture2D[classCount];
        classDescriptions = new string[classCount];
        classNames = new string[classCount];

        for (int i = 0; i < classCount; i++)
        {
            classNames[i] = classes[i]["name"].Value;
            classDescriptions[i] = classes[i]["description"].Value;
            classIcons[i] = Resources.Load(classes[i]["icon_path"].Value) as Texture2D;
            Adventurer.setDataPaths(classes[i]["id"].AsInt, classes[i]["ability_data"].Value);
        }
    }

    /***************************************************************
    * Sets the client side player's UI
    **************************************************************/
    private void initPlayerUI()
    {
        readyButton.interactable = false;
        playerNameField.text = Game.players[0].name;
        gameIdText.text = Game.gameId.ToString();
        playerCountText.text = (Game.connectedPlayers).ToString();

        if (Game.players[0].ready)
        {
            readyText.text = "Cancel Ready";
            readyButton.interactable = true;
        }
        else
        {
            readyText.text = " Ready Up!";
        }
    }

    /***************************************************************
    * Populates the initial display with the existing players
    **************************************************************/
    private void generatePlayerPanels()
    {
        foreach(Player player in Game.players)
        {
            if(player.id > -1)
            {
                PartyPanel currentPlayer = PartyPanel.create(player);
                currentPlayer.transform.SetParent(party_vertical_layout.transform);
                partyPanels.Add(currentPlayer);
            }
        }
    }

    /*---------------------------------------------------------------
                        CLIENT EVENT-HANDLERS
    ---------------------------------------------------------------*/
    /***************************************************************
    * Returns the player the the main menu, closing the TCP connection
      to th server, inturn notifying the other players
      (handled by server).
    **************************************************************/
    public void mainMenuClicked()
    {
        Game.gameClient.dissconnect();
        StateManager.changeScene(GameState.MAIN_MENU);
    }

    /***************************************************************
    * Toggles the Ready button action and sends the game update
      to the server
    **************************************************************/
    public void readyClicked()
    {
        Game.players[0].ready = !Game.players[0].ready;
        if(Game.players[0].ready) readyText.text = "Cancel Ready";
        else readyText.text = " Ready Up!";

        renderReadyState(0);
        //Send server ready change
        Message.send(new CharacterCreationMessage(CreationInstruction.READY_UP));
    }



    /***************************************************************
    * Event handler for selecting a class through clicking the
      Warrior, Wizard, Rogue or Cleric icons. Sends the selected
      class information to the server.

    @parm - classChosen: int between 0-3 representing the playable
    classes.
    **************************************************************/
    public void classClicked(int classChosen)
    {
        if (classChosen == Game.players[0].adventuringClass) { return; }
        if (!selectClass(classChosen, 0)) { return; }
        if (!readyButton.interactable) readyButton.interactable = true;

        classDescription.text = classDescriptions[classChosen];
        classChoice.text = classNames[classChosen];

        Message.send(new CharacterCreationMessage(CreationInstruction.CLASS_CHANGE));
    }

    private void renderReadyState(int playerIndex)
    {
        partyPanels[playerIndex].setReadyStatus(Game.players[playerIndex].ready);
    }


    /***************************************************************
    * Helper function for applying the class changes to the player
      who triggered the event.

    @parm - classChosen: int between 0-3 representing the playable
    classes.
    @param - playerIndex: int between 0-players.Count() that
    is responsible for the class change.

    @return - true: The class selected was a valid and implemented
                    selection.
              false: The class selected is unknown and cannot be 
                     applied.
    **************************************************************/
    private bool selectClass(int classChosen, int playerIndex)
    {
        if (classChosen < 0 || classChosen > 3) { return false; }
        Game.players[playerIndex].adventuringClass = classChosen;
        partyPanels[playerIndex].setClass(classChosen);
        return true;
    }

    /***************************************************************
    * Event handler for start button; removes unused player objects
      and applies the class chosen (int) to the player's
      Adventurer component.
    **************************************************************/
    public void startClicked()
    {

        if(!Game.isSinglePlayer)
        {
            if (Game.clientSidePlayer.isPartyLeader)
            {
                Message.send(new CharacterCreationMessage(CreationInstruction.GAME_START));
            }

            int connectedPlayers = Game.connectedPlayers;

            if (connectedPlayers != Game.PARTY_LIMIT)
                Game.players.RemoveRange(connectedPlayers, Game.PARTY_LIMIT - connectedPlayers);
        }

        foreach (Player player in Game.players)
        {
            player.applyClass();
        }

        StateManager.changeScene(GameState.BATTLE_STATE);
    }

    /***************************************************************
    * Adds new player UI to the game
    **************************************************************/
    private void addPlayerToUI(in Player player)
    {
        PartyPanel currentPlayer = PartyPanel.create(player);
        currentPlayer.transform.SetParent(party_vertical_layout.transform);
        partyPanels.Add(currentPlayer);
    }

    /***************************************************************
    * Adds new player UI to the game
    **************************************************************/
    private void removePlayerFromUi(int playerIndex)
    {
        Destroy(partyPanels[playerIndex].gameObject);
        partyPanels.RemoveAt(playerIndex);
    }

    /*---------------------------------------------------------------
                        SERVER-HANDLERS
    ---------------------------------------------------------------*/
    /***************************************************************
    * Continually polls the TCP NetworkClient instance to check for 
      server communication, and passes any valid communication
      to processor functions.
    **************************************************************/
    private void Update()
    {
        if (!Game.isSinglePlayer)
        {
            if (Game.gameClient.ready())
            {
                processServerInstructions(Game.gameClient.read());
            }
        }

        startButton.interactable = Player.hasAllReady && Game.clientSidePlayer.isPartyLeader;
    }

    /***************************************************************
    * Processes the packet sent from server to determine what
      client side event to trigger.
    
    @param: instructions: The JSON string of any player-party
    driven changes, or 'a' as an alive check.
    **************************************************************/
    private void processServerInstructions(string instructions)
    {
        if (instructions == "" || instructions[0] == 'a') { return; }
        CharacterCreationMessage message = new CharacterCreationMessage(instructions);
        Player player = message.instructionType == 2 || message.instructionType == 3 ? Game.players[Game.connectedPlayers - 1] : null;
        switch ((CreationInstruction)message.instructionType)
        {
            case CreationInstruction.CONNECTION:
                Game.addPlayer(message.playerName, message.clientId);
                playerCountText.text = (Game.connectedPlayers).ToString();
                addPlayerToUI(Game.players[Game.connectedPlayers - 1]);
                break;

            case CreationInstruction.DISCONNECTION:
                removePlayerFromUi(Game.getPlayerIndex(message.clientId));
                Game.removePlayer(message.clientId);
                playerCountText.text = (Game.connectedPlayers).ToString();
                break;

            case CreationInstruction.CLASS_CHANGE:
                player.adventuringClass = message.adventuringClass;
                selectClass(player.adventuringClass, message.playerIndex);
                break;

            case CreationInstruction.READY_UP:
                player.ready = message.playerReadyStatus;
                renderReadyState(message.playerIndex);
                break;

            case CreationInstruction.GAME_START:
                startClicked();
                break;

            default:
                break;
        }
    }


}
