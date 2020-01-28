using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

//Project Imports
using Assets.Scripts.RPA_Player;
using Assets.Scripts.RPA_Client;
using Assets.Scripts.RPA_Message;
using Assets.Scripts.Util;


public class Game : MonoBehaviour
{
    public static readonly bool NEW_GAME = true;
    public static readonly bool JOINED_GAME = false;

    private static int gameId;
    public static List<Player> players;
    private static int connectedPlayers;
    public static bool isNewGame;
    public static readonly int PARTY_LIMIT = 4;
    public static int readyCount = 0;

    /*******************Game/Client Initialization*******************/
    public static void start(string playerName, bool _isNewGame, int _gameId)
    {
        isNewGame = _isNewGame;
        gameId = _gameId;
        init(playerName);
    }

    //initialises client connection to server, and  player array.
    private async static void init(string playerName)
    {
        connectedPlayers = 0;
        players = new List<Player>();
        //Consturct 4 empty players on game start 
        for (int i = 0; i < PARTY_LIMIT; i++)
            players.Add(new Player());
        //Send connection message to server with player name/gameId
        ConnectionMessage connectionMessage = new ConnectionMessage(playerName, (int) ConnectionMessage.ConnectionMessageType.OUTBOUND);
        Client.connect();
        Client.send(connectionMessage.getMessage());

        //Wait for return message of game creation/joining
        Task<int> recieveGameId = waitForGameData(playerName);
        gameId = await recieveGameId;
    }

    private static Task<int> waitForGameData(string playerName)
    {
        bool hasGameId = false;
        int result = -1;
        string serverMessage;
        //Wait for server to respond with game Id
        while(!hasGameId)
        {
            if(Client.ready())
            {
                serverMessage = Client.read();
                if (serverMessage != "a")
                {   //Deserializes server message and updates player list if existing players in game
                    ConnectionMessage message = new ConnectionMessage(serverMessage, (int)ConnectionMessage.ConnectionMessageType.INBOUND);
                    result = message.game_id;
                    hasGameId = true;
                }
            } 
        }
        return Task.FromResult(result);
    }

    public static int getGameId()
    {
        return gameId;
    }

    /*******************Player managment*******************/
    //Typically called when new players join
    public static void addPlayer(string name, int id)
    {
        players[connectedPlayers].name = name;
        players[connectedPlayers].id = id;
        connectedPlayers++;
    }

    //Typically called when joining a current game
    public static void addPlayer(int id, string name, int adventuringClass, bool ready)
    {
        players[connectedPlayers].name = name;
        players[connectedPlayers].id = id;
        players[connectedPlayers].adventuringClass = adventuringClass;
        players[connectedPlayers].ready = ready;

        connectedPlayers++;
    }


    public static int getPlayerIndex(int playerId)
    {
        int playetIndex = -1; ;
        for (int i = 0; i <= connectedPlayers - 1; i++)
        {
            if (players[i].id == playerId)
            {
                playetIndex = i;
            }
        }
        return playetIndex;
    }

    public static void removePlayer(int playerId)
    {
        int playerRemoving = getPlayerIndex(playerId);
        resize(playerRemoving);
        connectedPlayers--;
    }

    //Maintains contiguous array 
    private static void resize(int index)
    {
        if(connectedPlayers > 2 && index != connectedPlayers -1)
        {
            Player temp = players[connectedPlayers -1]; 
            players[connectedPlayers - 1] = players[index]; 
            players[index] = temp; 
        }
        players[currentConnections()].name = "";
        players[currentConnections()].id = -1;
        players[currentConnections()].adventuringClass =-1;
        players[currentConnections()].ready = false;
    }

    public static Player getPlayerById(int playerId)
    {
        foreach(Player p in players)
        {
            if(playerId == p.id)
                return p;
        }
        return null;
    }

    public static int currentConnections()
    {
        return connectedPlayers-1;
    }

    private void Update()
    {

    }
}
