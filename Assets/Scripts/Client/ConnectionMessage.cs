using UnityEngine;
using Assets.Scripts.RPA_Player;
using System;
using SimpleJSON;
using System.Collections.Generic;

    [Serializable]
 class ConnectionMessage: Assets.Scripts.RPA_Message.Message
{
        public string name;

        public enum ConnectionMessageType
        {
            OUTBOUND = 0,
            INBOUND = 1
        }

        public ConnectionMessage(string token, int instructionType) : base()
        {
            this.state_id = (int) States.CONNECTION;
            if(instructionType == (int)ConnectionMessageType.INBOUND)
            {
                this.deserialize(token);
            }
            else if(instructionType == (int)ConnectionMessageType.OUTBOUND)
            {
                this.client_id = -1;
                this.name = token;
                serialize();
            }
        }

        protected override void serialize()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.state_id);
            json.Add("game_id", this.game_id);
            json.Add("name", this.name);
            json.Add("client_id", this.client_id);
            this.message = json.ToString();
        }

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
