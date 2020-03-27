﻿/*---------------------------------------------------------------
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

#region IMPORTS
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Assets.Scripts.RPA_Messages;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Game;
using SimpleJSON;
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
    public Text[] partyMembers;
    public RawImage[] partyClasses;
    public RawImage[] readyChecks;
    #endregion UI COMPONENTS

    //Statically loaded data
    private Texture2D[] classIcons;
    private const string CLASS_DATA_FILE = "assets/resources/data/meta_data.json";
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
        initClassData();
        initPlayerUI();
        updatePlayerList();
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
            partyClasses[i].enabled = false;
            readyChecks[i].enabled = false;
            classNames[i] = classes[i]["name"].Value;
            classDescriptions[i] = classes[i]["description"].Value;
            classIcons[i] = Resources.Load(classes[i]["icon_path"].Value) as Texture2D;

            AdventuringClass.setDataPaths(classes[i]["id"].AsInt, classes[i]["ability_path"].Value);
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
    }

    /***************************************************************
    * Re-populates players name and icons on party menu,
      called when player data changes (Ready Status, Class Selection),
      joining or leaveing the game.
    **************************************************************/
    private void updatePlayerList()
    {
        for (int i = 0; i < Game.players.Count; i++)
        {
            Player p = Game.players[i];
            partyMembers[i].text = p.name;
            if (p.adventuringClass == -1) partyClasses[i].enabled = false;
            else selectClass(p.adventuringClass, i);
            renderReadyState(i);
        }
    }

    /*---------------------------------------------------------------
                        SERVER-HANDLERS
    ---------------------------------------------------------------*/
    /***************************************************************
    * Continually polls the TCP Client instance to check for 
      server communication, and passes any valid communication
      to processor functions.
    **************************************************************/
    private void Update()
    {
        int readyCount = 0;
        if (Game.gameClient.ready())
            processServerInstructions(Game.gameClient.read());

        foreach (Player p in Game.players)
            if (p.ready)readyCount++;

        startButton.interactable = readyCount == Game.connectedPlayers;
    }

    /***************************************************************
    * Processes the packet sent from server to determine what
      client side event to trigger.
    
    @param: instructions: The JSON string of any player-party
    driven changes, or 'a' as an alive check.
    **************************************************************/
    private void processServerInstructions(string instructions)
    {
        if (instructions[0] == 'a') { return; }

        CharacterCreationMessage message = new CharacterCreationMessage(instructions);
        Player player = message.instructionType == 0 || message.instructionType == 1 ? null: Game.players[message.playerIndex];

        switch ((CreationInstruction)message.instructionType)
        {
            case CreationInstruction.CONNECTION:
                Game.addPlayer(message.playerName, message.client_id);
                playerCountText.text = (Game.connectedPlayers).ToString();
                partyMembers[Game.connectedPlayers - 1].text = message.playerName;
                break;

            case CreationInstruction.DISCONNECTION:
                Game.removePlayer(message.client_id);
                playerCountText.text = (Game.connectedPlayers).ToString();
                updatePlayerList();
                break;

            case CreationInstruction.CLASS_CHANGE:
                player.adventuringClass = message.adventuringClass;
                selectClass(player.adventuringClass, message.playerIndex);
                break;

            case CreationInstruction.READY_UP:
                player.ready = message.playerReadyStatus;
                renderReadyState(message.playerIndex);
                break;

            default:
                break;
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
    }

    /***************************************************************
    * Toggles the Ready button action and sends the game update
      to the server
    **************************************************************/
    public void readyClicked()
    {
        Game.players[0].ready = !Game.players[0].ready;
        if (Game.players[0].ready) readyText.text = "Cancel Ready";
        else readyText.text = "Ready Up!";
        renderReadyState(0);
        CharacterCreationMessage readyChange = new CharacterCreationMessage((int)CreationInstruction.READY_UP);
        Game.gameClient.send(readyChange.getMessage());
    }

    /***************************************************************
    * Sets the UI (green tick) indicator for ready status
      to the value of the Player's ready status.

     @param - player: The index of the player who has toggled
     a ready action.
    **************************************************************/
    private void renderReadyState(int player)
    {
        readyChecks[player].enabled = Game.players[player].ready;
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
        CharacterCreationMessage classChange = new CharacterCreationMessage((int)CreationInstruction.CLASS_CHANGE);
        Game.gameClient.send(classChange.getMessage());
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
        partyClasses[playerIndex].texture = classIcons[classChosen];
        partyClasses[playerIndex].enabled = true;
        Game.players[playerIndex].adventuringClass = classChosen;
        return true;
    }

    /***************************************************************
    * Event handler for start button; removes unused player objects
      and applies the class chosen (int) to the player's
      AdventuringClass component.
    **************************************************************/
    public void startClicked()
    {
        int connectedPlayers = Game.connectedPlayers;

        if (connectedPlayers != Game.PARTY_LIMIT)
            Game.players.RemoveRange(connectedPlayers, Game.PARTY_LIMIT - connectedPlayers);

        AdventuringClass.setClassSprites(Game.players);
        foreach (Player player in Game.players)
        {
            player.applyClass();
        }
    }
}
