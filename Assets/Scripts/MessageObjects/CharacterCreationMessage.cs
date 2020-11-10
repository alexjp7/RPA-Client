/*---------------------------------------------------------------
                    CHARACTER-CREATION-MESSAGE
 ---------------------------------------------------------------*/
/***************************************************************
*  This message object is used as a serializable model in client
   to server communication of the Character/party creation state.

*   CharacterCreationMessage inherits from the Message class which provides
    the base functionality for serialisation and deserialisation,
    with any additional fields provided by the sub-class.
**************************************************************/

using System;
using Assets.Scripts.RPA_Game;
using SimpleJSON;

namespace Assets.Scripts.RPA_Messages
{
    public enum CreationInstruction: int
    {   //Connection and disconenction only on deserialization
        CONNECTION = 0,
        DISCONNECTION = 1,
        CLASS_CHANGE = 2,
        READY_UP = 3,
        GAME_START = 4
    }

    class CharacterCreationMessage : Message
    {
        public int instructionType;
        public int playerIndex;
        public string playerName;
        public int adventuringClass;
        public bool playerReadyStatus;

        /***************************************************************
        * On Message consturction, based on the paramater/s used in construciton
          the object will be constructed to be used for seriazation or
          deserialization.
        **************************************************************/
        public CharacterCreationMessage(CreationInstruction instructionType) : base()
        {
            this.stateId = (int)State.CHARACTER_CREATION;
            this.instructionType = (int)instructionType;
            serialize();
        }

        public CharacterCreationMessage(string jsonString) : base()
        {
            this.deserialize(jsonString);
        }
        
        /***************************************************************
        @return - The JSON string of the event of a client side
        Adventering-class change.
        **************************************************************/
        private string serializeClassChange()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.stateId);
            json.Add("game_id", this.gameId);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);
            json.Add("adventuringClass", this.actor.adventuringClass);
            return json.ToString();
        }

        /***************************************************************
        @return - the JSON string of the event of a client side
        event of clicking the 'ready up' button.
        **************************************************************/
        private string serializeReadyUp()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.stateId);
            json.Add("game_id", this.gameId);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);
            json.Add("ready", this.actor.ready);
            return json.ToString();

        }

        private string serializeStart()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.stateId);
            json.Add("game_id", this.gameId);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);
            return json.ToString();
        }



        /***************************************************************
        * Transform game data into a JSON string which is sent by the 
          NetworkClient's TCPClient instance which is then communicated to 
          the server
        **************************************************************/
        protected override void serialize()
        {
            string message = "";

            switch((CreationInstruction)this.instructionType)
            { 
                case CreationInstruction.CLASS_CHANGE:
                    message = serializeClassChange();    
                    break;
                case CreationInstruction.READY_UP:
                    message = serializeReadyUp();
                    break;
                case CreationInstruction.GAME_START:
                    message = serializeStart();
                    break;

                default:
                    //Do some error checking
                    break;
            }
            //Should error check empty message
            this.message = message;
        }



        /***************************************************************
        * Transforms an incoming JSON string from the server into
          useable game data.
        **************************************************************/
        protected override void deserialize(string jsonString)
        {
            base.deserialize(jsonString);

            JSONNode json = JSON.Parse(jsonString);
            this.clientId = json["client_id"].AsInt;
            this.instructionType = json["instructionType"].AsInt;
            this.playerIndex = Game.getPlayerIndex(clientId);
            this.adventuringClass  = json["adventuringClass"].AsInt;
            this.playerName = json["name"].Value;
            this.playerReadyStatus= json["ready"].AsBool;
        }

    }
}
