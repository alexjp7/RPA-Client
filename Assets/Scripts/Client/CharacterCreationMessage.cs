using System;
using SimpleJSON;
using Assets.Scripts.RPA_Player;

namespace Assets.Scripts.Client
{
    public enum CreationInstruction: int
    {   //Connection and disconenction only on deserialization
        CONNECTION = 0,
        DISCONNECTION = 1,
        CLASS_CHANGE = 2,
        READY_UP = 3
    }

    class CharacterCreationMessage : RPA_Message.Message
    {

        public int instructionType;
        public int playerIndex;

        public CharacterCreationMessage(int instructionType) : base()
        {
            this.state_id = (int)States.CHARACTER_CREATION;
            this.instructionType = instructionType;
            serialize();
        }

        public CharacterCreationMessage(string jsonString) : base()
        {
            this.deserialize(jsonString);
        }

        protected override void serialize()
        {
            string message = "";

            switch(this.instructionType)
            { 
                case (int) CreationInstruction.CLASS_CHANGE:
                    message = serializeClassChange();    
                    break;
                case (int) CreationInstruction.READY_UP:
                    message = serializeReadyUp();
                    break;
                default:
                    //Do some error checking
                    break;
            }
            //Should error check empty message
            this.message = message;
        }

        private string serializeClassChange()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.state_id);
            json.Add("game_id", this.game_id);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);
            json.Add("adventuring_class", this.actor.adventuringClass);
            return json.ToString();
        }

        private string serializeReadyUp()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.state_id);
            json.Add("game_id", this.game_id);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);
            json.Add("ready", this.actor.ready);
            return json.ToString();

        }

        protected override void deserialize(string jsonString)
        {
            base.deserialize(jsonString);

            JSONNode json = JSON.Parse(jsonString);
            this.client_id = json["client_id"].AsInt;

            Player player = Game.getPlayerById(this.client_id);
            this.playerIndex = Game.getPlayerIndex(client_id);

            switch (this.state_id)
            {
                case (int)CreationInstruction.CLASS_CHANGE:
                    player.adventuringClass = json["adventuring_class"].AsInt;
                    break;

                case (int)CreationInstruction.READY_UP:
                    player.ready = json["ready"].AsBool;
                    break;

                case (int)CreationInstruction.CONNECTION:
                    Game.addPlayer(json["name"].Value, json["id"].AsInt);
                    break;

                case (int)CreationInstruction.DISCONNECTION:
                    Game.removePlayer(json["id"].AsInt);
                    break;

                default:
                    //Do some error checking
                    break;
            }
        }

    }
}
