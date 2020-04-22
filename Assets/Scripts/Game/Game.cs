/*---------------------------------------------------------------
                        GAME
 ---------------------------------------------------------------*/
/***************************************************************
* The Game class provides static access to the overarching
  meta-properties of the current game session started/joined
  by a player. 

* The Game class collates any of the data that needs to be maintained
  throughout the life-time of the application, including
  the; Game ID provided from the server, the list of players
  and the TCP Client instance.
**************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Messages;

namespace Assets.Scripts.RPA_Game
{
    public class Game
    {
        //Game Constants
        public static bool isSinglePlayer;
        public static readonly bool NEW_GAME = true;
        public static readonly bool JOINED_GAME = false;
        public static readonly int PARTY_LIMIT = 4;

        //Game Properties
        public static int partyLeaderId { get; set; }
        public static int gameId { get; set;}
        public static List<Player> players { get; set;}
        public static Player clientSidePlayer { get => players.Find(player => player.isClientPlayer); }
        public static int connectedPlayers { get; set;} //SET PUBLIC ONLY FOR TESTING PURPOSES
        public static string gameMessage { set; get;}

        //error/state flags
        private static bool isNewGame;
        private static bool isStarted;

        public static Client gameClient = Client.getInstance();


        /*---------------------------------------------------------------
                            GAME/CLIENT INITIALIZERS
         ---------------------------------------------------------------*/
        /***************************************************************
        * Calls the initialisation functions in order to being a game 
          session.

        @param - playerName: Name of the Client side player as chosen when 
        prompted by main-menu dialogue.
        @param - _isNewGame: flag for determining whether the player has
        selected a New game, or is joining a current game.
        @param - _gameId: If starting a new game, this value will be -1,
        otherwise upon joining a game _gameId will be a user inputed
        value of the game they wish to join.

        @return - true: The game join/start is succesful, prompting
                  the creation of the CharacterCreation game state.

                - false: Server communication failed, or an invalid
                  gameId was provided, resulting in error message 
                  to the user.
        **************************************************************/
        public static bool start(string playerName, bool _isNewGame, int _gameId)
        {
            partyLeaderId = -1;
            isStarted = false;
            isNewGame = _isNewGame;
            gameId = _gameId;
            init(playerName);

            return isStarted;
        }

        /***************************************************************
        * Sets game/player data to accomidate offline play
        **************************************************************/
        public static void startOffline(string playerName)
        {
            players = new List<Player>();
            Player player = new Player();
            player.name = playerName;
            players.Add(player);
            partyLeaderId = player.id; 
            connectedPlayers = 1;
            players[0].isClientPlayer = true; 
        }


        /***************************************************************
        * initialises client connection to server, and empty player
          objects.

        * init() also sets the isStarted flag, which is returned back
          to the MainMenu state to communicate any errors in connection.

        @param - playerName: Name of the Client side player as chosen 
        when prompted by main-menu dialogue.
        **************************************************************/

        private async static void init(string playerName)
        {
            connectedPlayers = 0;
            players = new List<Player>();

            //Instantiate default player objects during game init
            for (int i = 0; i < PARTY_LIMIT; i++) 
                players.Add(new Player());

            players[0].isClientPlayer = true; // flag  the player at start of player list as the client side player
            ConnectionMessage connectionMessage = new ConnectionMessage(playerName, (int) ConnectionMessage.ConnectionMessageType.OUTBOUND);
            gameClient.connect();

            //Wait for return message of game creation/joining
            if(gameClient.isConnected())
            {
                gameClient.send(connectionMessage.getMessage());
                Task<int> recieveGameId = waitForGameData(playerName);
                gameId = await recieveGameId;

                if (gameId == -1)
                {
                    gameMessage = "Game is either full OR doesn't exist yet";
                }
                else
                {
                    isStarted = true;
                }
            }
            else gameMessage = "Server Connection failed! Check server status";
        }

        /***************************************************************
        * Asynchronously waits for the server reply with a gameId
          or a dissconection packet.

        @param - playerName: Name of the Client side player as chosen 
        when prompted by main-menu dialogue.

        @return - The asyncronous Task containing the Game ID from the 
        server.
        **************************************************************/
        private static Task<int> waitForGameData(string playerName)
        {
            bool hasGameId = false;
            int result = -1;
            string serverMessage;

            while(!hasGameId)
            {
                if (gameClient.ready())
                {
                    serverMessage = gameClient.read();
                    //Continually Check if client connection has been lost while attempting to join game 
                    //A dissconect here indicates an invalid Game ID was provided
                    if (serverMessage == "d") break;

                    else if (serverMessage != "a")
                    {  
                        ConnectionMessage message = new ConnectionMessage(serverMessage, (int)ConnectionMessage.ConnectionMessageType.INBOUND);
                        result = message.game_id;
                        hasGameId = true;
                    }
                } 
            }
            return Task.FromResult(result);
        }

        /*---------------------------------------------------------------
                            PLAYER-MANAGERS
         ---------------------------------------------------------------*/
        /***************************************************************
        * Adds a new player to the players list.
       
        * Called from CharacterCreationState when an inbound connection
          message is recieved by client.

        @param - name: Name of the Client side player as chosen 
        when prompted by main-menu dialogue.
        @param - id: player ID that is set by the server.
        **************************************************************/
        public static void addPlayer(string name, int id)
        {
            players[connectedPlayers].name = name;
            players[connectedPlayers].id = id;
            connectedPlayers++;
        }

        /***************************************************************
        * Adds a new player to the players list.
       
        * Called from CharacterCreationMessage after joining a new game
          and the existing player data is sent to the client

        @param - name: Name of the Client side player as chosen 
        when prompted by main-menu dialogue.
        @param - id: Unique ID for a player that is set by the server.
        @param - adventuringClass: The current class selection of 
        an existing player.
        @param - ready: The current ready state of the existing player.
        **************************************************************/
        public static void addPlayer(int id, string name, int adventuringClass, bool ready)
        {
            players[connectedPlayers].name = name;
            players[connectedPlayers].id = id;
            players[connectedPlayers].adventuringClass = adventuringClass;
            players[connectedPlayers].ready = ready;
            connectedPlayers++;
        }

        /***************************************************************
        @param - playerId:  Unique ID for a player that is set by the server..
        
        @return - The position in the player list of a player based
        on their ID.
        **************************************************************/
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

        /***************************************************************
        * Removes the player from the player list based on the player
          id passed in. This function is called when the server notifies
          when a player has dissconnected from the game session.
        * 
        @param - playerId: Unique ID for a player that is set by the
        server.       
        **************************************************************/
        public static void removePlayer(int playerId)
        {
            int playerRemoving = getPlayerIndex(playerId);
            resize(playerRemoving);
            connectedPlayers--;
        }

        /***************************************************************
        * Maintains a contigous array of Player elements, and
          resets the removed player's fields to default values.

        @param - index: index of the player's list to be removed. 
        **************************************************************/
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

        /***************************************************************
        @param - playerId: Unique ID for a player that is set by the
        server. 
        
        @return - The player who's ID matches the passed in value.
        **************************************************************/
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

}
