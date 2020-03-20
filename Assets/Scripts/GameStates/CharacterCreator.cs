using System;
using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.RPA_Client;
using Assets.Scripts.RPA_Player;
using SimpleJSON;
using Assets.Scripts.RPA_Message;
using System.IO;
using Assets.Scripts.Player_Classes;

public class CharacterCreator : MonoBehaviour
{
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

    //Statically loaded data
    private Texture2D[] classIcons;
    private const string CLASS_DATA_FILE = "assets/resources/data/meta_data.json";
    private string[] classDescriptions;
    private string[] classNames;

    /******************INITIALIZERS***************/
    void Awake()
    {
        initClassData();
        initPlayerUI();
        drawPlayerList();
    }

    //Parses json file for class data and populates icon textures
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

    //Sets the client side players data
    private void initPlayerUI()
    {
        readyButton.interactable = false;
        playerNameField.text = Game.players[0].name;
        gameIdText.text = Game.getGameId().ToString();
        playerCountText.text = (Game.connectedPlayers).ToString();
    }

    //Redraw players name and icons on party menu
    private void drawPlayerList()
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
    //Update game state upon server communications
    private void Update()
    {
        int readyCount = 0;
        if (Client.ready())
            processServerInstructions(Client.read());

        foreach (Player p in Game.players)
            if (p.ready)readyCount++;

        startButton.interactable = readyCount == Game.connectedPlayers;
    }

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
                drawPlayerList();
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

    public void mainMenuClicked()
    {
        Client.dissconnect();
    }

    public void readyClicked()
    {
        Game.players[0].ready = !Game.players[0].ready;
        if (Game.players[0].ready) readyText.text = "Cancel Ready";
        else readyText.text = "Ready Up!";
        renderReadyState(0);
        CharacterCreationMessage readyChange = new CharacterCreationMessage((int)CreationInstruction.READY_UP);
        Client.send(readyChange.getMessage());
    }

    private void renderReadyState(int player)
    {
        readyChecks[player].enabled = Game.players[player].ready;
    }

    public void classClicked(int classChosen)
    {
        if (classChosen == Game.players[0].adventuringClass) { return; }
        if (!selectClass(classChosen, 0)) { return; }
        if (!readyButton.interactable) readyButton.interactable = true;

        classDescription.text = classDescriptions[classChosen];
        classChoice.text = classNames[classChosen];
        CharacterCreationMessage classChange = new CharacterCreationMessage((int)CreationInstruction.CLASS_CHANGE);
        Client.send(classChange.getMessage());
    }

    private bool selectClass(int classChosen, int playerIndex)
    {
        if (classChosen < 0 || classChosen > 3) { return false; }
        partyClasses[playerIndex].texture = classIcons[classChosen];
        partyClasses[playerIndex].enabled = true;
        Game.players[playerIndex].adventuringClass = classChosen;
        return true;
    }
    
    public void startClicked()
    {
        //Remove player objects that aren't being utilised
        int connectedPlayers = Game.connectedPlayers;

        if (connectedPlayers != Game.PARTY_LIMIT)
            Game.players.RemoveRange(connectedPlayers, Game.PARTY_LIMIT - connectedPlayers);

        AdventuringClass.setClassSprites(Game.players);
        foreach (Player player in Game.players)
        {
            player.assetData.icon = classIcons[player.adventuringClass];
            player.applyClass();
        }
    }
}
