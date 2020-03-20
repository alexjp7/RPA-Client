/*---------------------------------------------------------------
                    MESSAGE(Abstract Model)
 ---------------------------------------------------------------*/
/***************************************************************
*  The  Message class provides the base functionality/fields for
    the TCP communication on the client side.
*   Objects that inherit the class will be provided with 
    overite methods for serialization and deserialziation.
*   The message string field of this base class allows for
    protected set/public get operations to retrieve a 
    serialized object.
**************************************************************/
using System;
using SimpleJSON;
using Assets.Scripts.RPA_Player;


namespace Assets.Scripts.RPA_Message
{
    public abstract class Message
    {
        //Comment Message fields
        public int state_id;
        public int game_id;
        public int client_id;
        public string message;
        protected RPA_Player.Player actor;

        /***************************************************************
        * The States enum is used to indicate what game state is currently
          being operated on and is shared between client and server.
        **************************************************************/
        public enum States: int
        {
            CONNECTION = 0,
            CHARACTER_CREATION = 1
        }

        /***************************************************************
         * The base Message construction sets all the common fields
           for all sub-class message types.
        **************************************************************/
        public Message()
        {
            actor = Game.players[0] == null ? null : Game.players[0] ;
            this.game_id = Game.getGameId();
            this.state_id = -1;
            this.message = "";
            this.client_id = actor == null ? -1 : actor.id;
        }

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
            this.state_id = json["state_id"].AsInt;
            this.game_id = json["game_id"].AsInt;

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