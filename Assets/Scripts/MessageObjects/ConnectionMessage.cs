/*---------------------------------------------------------------
                    CONNECTION-MESSAGE
 ---------------------------------------------------------------*/
/***************************************************************
*  This message object is used as a serializable model in client
   to server communication of game connection and data used to relay
   the gameId, connection status and any existing players.

*   ConnectionMessage inherits from the Message class which provides
    the base functionality for serialisation and deserialisation,
    with any additional fields provided by the sub-class.
**************************************************************/
namespace Assets.Scripts.RPA_Messages
{
    using SimpleJSON;
    using Assets.Scripts.RPA_Game;

    public enum ConnectionMessageType
    {
        OUTBOUND = 0,
        INBOUND = 1
    }
    class ConnectionMessage : Message
    {
        public string name;

        /***************************************************************
        * On Message consturction, based on the paramater/s used in construciton
          the object will be constructed to be used for seriazation or
          deserialization.
        **************************************************************/
        public ConnectionMessage(string token, ConnectionMessageType instructionType) : base()
        {
            this.stateId = (int)State.CONNECTION;
            if (instructionType == ConnectionMessageType.INBOUND)
            {
                this.deserialize(token);
            }
            else if (instructionType == ConnectionMessageType.OUTBOUND)
            {
                this.clientId = -1;
                this.name = token;
                serialize();
            }
        }

        /***************************************************************
        * Transform game data into a JSON string which is sent by the 
          Client's TCPClient instance which is then communicated to 
          the server
        **************************************************************/
        protected override void serialize()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.stateId);
            json.Add("game_id", this.gameId);
            json.Add("name", this.name);
            json.Add("client_id", this.clientId);
            this.message = json.ToString();
        }

        /***************************************************************
        * Trabsforms an incoming JSON string from the server into
          useable game data.
        **************************************************************/
        protected override void deserialize(string jsonString)
        {
            base.deserialize(jsonString);
            JSONNode json = JSON.Parse(jsonString);
            JSONArray playerJson = json["players"].AsArray;

            //Add players to game directly
            for (int i = playerJson.Count - 1; i >= 0; i--)
            {
                //Player data
                int id = playerJson[i]["id"].AsInt;
                string name = playerJson[i]["name"].Value;
                int adventuringClass = playerJson[i]["adventuringClass"].AsInt;
                bool isReady = playerJson[i]["ready"].AsBool;
                
                //Party leader Id set by game server
                if (playerJson[i]["isPartyLeader"].AsBool)
                {
                    Game.partyLeaderId = id;
                }
                Game.addPlayer(id, name, adventuringClass, isReady);
            }
        }
    }

}

