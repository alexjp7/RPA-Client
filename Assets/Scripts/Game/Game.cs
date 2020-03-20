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
    //Game Constants
    public static readonly bool NEW_GAME = true;
    public static readonly bool JOINED_GAME = false;
    public static readonly int PARTY_LIMIT = 4;
    //Game Properties
    public static int gameId { get; set;}
    public static List<Player> players { get; set;}
    public static int connectedPlayers { get; set;} //SET PUBLIC ONLY FOR TESTING PURPOSES
    public static string gameMessage { set; get;}
    //error/state flags
    private static bool isNewGame;
    private static bool isStarted;

    /*******************Game/Client Initialization*******************/
    public static bool start(string playerName, bool _isNewGame, int _gameId)
    {
        isStarted = false;
        isNewGame = _isNewGame;
        gameId = _gameId;
        init(playerName);

        return isStarted;
    }

    //initialises client connection to server, and  player array.
    private async static void init(string playerName)
    {
        connectedPlayers = 0;
        players = new List<Player>();
        //Consturct 4 empty players on game start 
        for (int i = 0; i < PARTY_LIMIT; i++)
            players.Add(new Player());

        Game.players[0].isClientPlayer = true; // flag  the player at start of player list as the client side player
        //Send connection message to server with player name/gameId
        ConnectionMessage connectionMessage = new ConnectionMessage(playerName, (int) ConnectionMessage.ConnectionMessageType.OUTBOUND);
        Client.connect();

        //Wait for return message of game creation/joining
        if(Client.isConnected())
        {
            Client.send(connectionMessage.getMessage());
            Task<int> recieveGameId = waitForGameData(playerName);
            gameId = await recieveGameId;
            //An invalid Game Id was provided
            if (gameId == -1) gameMessage = "Game is either full OR doesn't exist yet";  
            else isStarted = true;
        }
        else gameMessage = "Server Connection failed! Check server status";
    }

    private static Task<int> waitForGameData(string playerName)
    {
        bool hasGameId = false;
        int result = -1;
        string serverMessage;
        //Wait for server to respond with game Id
        while(!hasGameId)
        {
            if (Client.ready())
            {
                serverMessage = Client.read();
                //Continually Check if client connection has been lost while attempting to join game 
                //A dissconect here indicates an invalid Game ID was provided
                if (serverMessage == "d")
                {
                    Debug.Log("Server message = " + serverMessage);
                    break;
                }

                else if (serverMessage != "a")
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
        if(connectedPlayers > 2 && index != connectedPlayers -  1)
        {
            Player temp = players[connectedPlayers - 1]; 
            players[connectedPlayers -1] = players[index]; 
            players[index] = temp; 
        }

        players[connectedPlayers - 1].name = "";
        players[connectedPlayers - 1].id = -1;
        players[connectedPlayers - 1].adventuringClass =-1;
        players[connectedPlayers - 1].ready = false;
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
}
