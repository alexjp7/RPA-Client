using System;
using SimpleJSON;
using Assets.Scripts.RPA_Player;


namespace Assets.Scripts.RPA_Message
{
    [Serializable]
    abstract class Message
    {
        public int state_id;
        public int game_id;
        public int client_id;
        public string message;

        protected Player actor;

        public enum States: int
        {
            CONNECTION = 0,
            CHARACTER_CREATION = 1
        }

        //Used When starting game
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

        protected virtual void deserialize(string jsonString)
        {
            JSONNode json = JSON.Parse(jsonString);
            this.state_id = json["state_id"].AsInt;
            this.game_id = json["game_id"].AsInt;

        }

        protected abstract void serialize();
    }
}
