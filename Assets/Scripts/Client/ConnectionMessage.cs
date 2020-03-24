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
using Assets.Scripts.RPA_Player;
using SimpleJSON;


namespace Assets.Scripts.RPA_Message
{
    class ConnectionMessage : Message
    {
        public string name;

        public enum ConnectionMessageType
        {
            OUTBOUND = 0,
            INBOUND = 1
        }

        /***************************************************************
        * On Message consturction, based on the paramater/s used in construciton
          the object will be constructed to be used for seriazation or
          deserialization.
        **************************************************************/
        public ConnectionMessage(string token, int instructionType) : base()
        {
            this.state_id = (int)States.CONNECTION;
            if (instructionType == (int)ConnectionMessageType.INBOUND)
            {
                this.deserialize(token);
            }
            else if (instructionType == (int)ConnectionMessageType.OUTBOUND)
            {
                this.client_id = -1;
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
            json.Add("state_id", this.state_id);
            json.Add("game_id", this.game_id);
            json.Add("name", this.name);
            json.Add("client_id", this.client_id);
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
                Game.addPlayer(playerJson[i]["id"].AsInt,
                                       playerJson[i]["name"].Value,
                                       playerJson[i]["adventuringClass"].AsInt,
                                       playerJson[i]["ready"].AsBool);
            }
        }
    }

}

