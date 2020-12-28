/*---------------------------------------------------------------
                    MESSAGE(Abstract Model)
 ---------------------------------------------------------------*/
/***************************************************************
*  The  Message class provides the base functionality/fields for
    the TCP communication on the client side.

*   Objects that inherit the class will be provided with 
    overite methods for serialization and deserialziation.
**************************************************************/
using System;
using SimpleJSON;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Game;


namespace Assets.Scripts.RPA_Messages
{
    public abstract class Message
    {
        //Game ID
        public int gameId;
        //Game state ID
        public int stateId;
        //Client who is sending/recieving a message
        public int clientId;
        //JSON message
        public string message;
        protected Player actor;


        /***************************************************************
        * The States enum is used to indicate what game state is currently
          being operated on and is shared between client and server.
        **************************************************************/
        public enum State : int
        {
            CONNECTION = 0,
            CHARACTER_CREATION = 1,
            BATTLE = 2
        }

        public static void send(Message gameMessage)
        {
            if (!Game.isSinglePlayer)
            {
                Game.gameClient.send(gameMessage.message);
            }
        }

        /***************************************************************
         * The base Message construction sets all the common fields
           for all sub-class message types.
        **************************************************************/
        public Message()
        {
            actor = Game.clientSidePlayer == null ? null : Game.clientSidePlayer;
            this.gameId = Game.gameId;
            this.stateId = -1;
            this.message = "";
            this.clientId = actor == null ? -1 : actor.id;
        }

        /***************************************************************
        * The message string field of this base class allows for
          protected set/public get operations to retrieve a 
          serialized object.

        @return - JSON string of a game-state's message object.
        **************************************************************/
        public string getMessage()
        {
            return this.message;
        }

        /***************************************************************
        * Transform game data into a JSON string which is sent by the 
          Client's TCPClient instance which is then communicated to 
          the server - virtual method is provided to allow for
          statId and gameId operations to remain consistent.
        **************************************************************/
        protected virtual void deserialize(string jsonString)
        {
            JSONNode json = JSON.Parse(jsonString);
            this.stateId = json["state_id"].AsInt;
            this.gameId = json["game_id"].AsInt;

        }

        /***************************************************************
        * Transform game data into a JSON string which is sent by the 
          Client's TCPClient instance which is then communicated to 
          the server - Serialisation doesn't not offer the same level
          of re-useability from a top-level perspective, as each
          game state must be responsible its own instruction types.
        **************************************************************/
        protected abstract void serialize();
    }
}