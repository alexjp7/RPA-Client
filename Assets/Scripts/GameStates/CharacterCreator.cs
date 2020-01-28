using System;
using UnityEngine;
using UnityEngine.UI;

using Assets.Scripts.RPA_Client;
using Assets.Scripts.RPA_Player;
using Assets.Scripts.RPA_Message;
using SimpleJSON;
using Assets.Scripts.Client;
using System.IO;

public class CharacterCreator : MonoBehaviour
{
    //Player/UI Text
    public Text classChoice;
    public Text classDescription;
    public Text playerNameField;
    public Text gameIdText;
    public Text playerCountText;
    public Text readyText;

    public Button startButton;

    //Party textures/text
    public Text[] partyMembers;
    public RawImage[] partyClasses;
    public RawImage[] readyChecks;

    //Statically loaded data
    private Texture2D[] classIcons;
    private const string CLASS_DATA_FILE = "assets/resources/data/classes.json";
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
        string jsonClassData = File.ReadAllText(CLASS_DATA_FILE);

        JSONNode json = JSON.Parse(jsonClassData);
        JSONArray gameClasses = json["classes"].AsArray;

        int classCount = gameClasses.Count;
        classIcons = new Texture2D[classCount];
        classDescriptions = new string[classCount];
        classNames = new string[classCount];

        for (int i = 0; i < classCount; i++)
        {
            partyClasses[i].enabled = false;
            readyChecks[i].enabled = false;
            classNames[i] = gameClasses[i]["name"].Value;
            classDescriptions[i] = gameClasses[i]["description"].Value;
            classIcons[i] = Resources.Load(gameClasses[i]["path"].Value) as Texture2D;
        }
    }
    //Sets the client side players data
    private void initPlayerUI()
    {
        playerNameField.text = Game.players[0].name;
        gameIdText.text = Game.getGameId().ToString();
        playerCountText.text = (Game.currentConnections() + 1).ToString();
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
            ;
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
            if (p.ready) readyCount++;

        Game.readyCount = readyCount;
        startButton.interactable = Game.readyCount == Game.currentConnections() + 1;
    }

    private void processServerInstructions(string instructions)
    {
        if (instructions[0] == 'a') { return; }

        CharacterCreationMessage message = new CharacterCreationMessage(instructions);
        Player player = Game.players[message.playerIndex];

        switch (message.instructionType)
        {
            case (int)CreationInstruction.CONNECTION:
                playerCountText.text = (Game.currentConnections() + 1).ToString();
                partyMembers[Game.currentConnections()].text = player.name;
                break;

            case (int)CreationInstruction.DISCONNECTION:
                playerCountText.text = (Game.currentConnections() + 1).ToString();
                drawPlayerList();
                break;

            case (int)CreationInstruction.CLASS_CHANGE:
                selectClass(player.adventuringClass, message.playerIndex);
                break;

            case (int)CreationInstruction.READY_UP:
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
    //This is about scruffy, refactor this
    public void readyClicked()
    {
        Game.players[0].ready = !Game.players[0].ready;
        if (!Game.players[0].ready) readyText.text = "Cancel Ready";
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
        //Check for valid class selections, and return to stop uncessary processing
        if (classChosen == Game.players[0].adventuringClass) { return; }
        if (!selectClass(classChosen, 0)) { return; }

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
}
