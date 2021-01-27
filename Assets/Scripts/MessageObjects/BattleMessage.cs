/*---------------------------------------------------------------
                            BATTLE-STATE-MESSAGE
 ---------------------------------------------------------------*/
/***************************************************************
*  This message object is used as a serializable model in client
   to server communication of any battle state updates and events.

*   BattleStateMessage inherits from the Message class which provides
    the base functionality for serialisation and deserialisation,
    with any additional fields provided by the sub-class.
**************************************************************/
namespace Assets.Scripts.RPA_Messages
{
    using SimpleJSON;
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.GameStates;
    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.Util;
    public enum BattleInstruction
    {
        COMBAT_INIT = 0,
        TURN_PROGRESSED = 1,
        TURN_ACTION = 2
    }
    public class BattleMessage : Message
    {
        public int instructionType { get; private set; }
        public List<int> turnOrder { get; private set; }
        public List<KeyValuePair<string, string>> monsters { get; private set; }

        public Dictionary<int,float> targets { get; private set; }
        public TargetingType abilityTargeting { get; private set; }
        public ConditionStrength conditionStrength { get; private set;}
        public List<int> abilityTypes { get; private  set; }
        public string abilityName { get; private set;}

        private static CombatController turnController => StateManager.battleState.combatController;

        /*---------------------------------------------------------------
                            SERIALIZATION
         ---------------------------------------------------------------*/
        public BattleMessage(BattleInstruction instructionType)
        {
            this.stateId = (int)State.BATTLE;
            this.instructionType = (int)instructionType;
            serialize();
        }

        protected override void serialize()
        {
            string message = "";

            switch ((BattleInstruction)this.instructionType)
            {
                case BattleInstruction.COMBAT_INIT:
                    message = serializeInitCombat();
                    break;

                case BattleInstruction.TURN_PROGRESSED:
                    message = serializeTurnProgressed();
                    break;

                case BattleInstruction.TURN_ACTION:
                    message = serializeTurnAction();
                    break;

                default:
                    //Do some error checking
                    break;
            }
            //Should error check empty message
            this.message = message;
        }

        private string serializeTurnAction()
        {
            JSONObject json = new JSONObject();
            Ability abilityUsed = turnController.lastAbilityUsed;

            JSONArray targets = new JSONArray();

            foreach (var target in turnController.targets)
            {
                JSONObject jsonTarget = new JSONObject();
                jsonTarget.Add("id",target.id);
                jsonTarget.Add("remaining_health",target.healthProperties.currentHealth);
                targets.Add(jsonTarget);
            }

            json.Add("state_id", this.stateId);
            json.Add("game_id", this.gameId);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);
            json.Add("is_player_turn", turnController.isPlayerTurn);
            json.Add("targets", targets);
            json.Add("ability_used", abilityUsed.toJson());

            return json.ToString();
        }

        private string serializeTurnProgressed()
        {
            JSONObject json = new JSONObject();
            json.Add("state_id", this.stateId);
            json.Add("game_id", this.gameId);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);

            return json.ToString();
        }

        private string serializeInitCombat()
        {
            //Collate combat order/monster party 
            JSONArray turnOrder = new JSONArray();
            JSONArray monsterJson = new JSONArray();

            foreach (var player in Game.players.Select(player => player.id).ToArray())
            {
                turnOrder.Add(JSON.Parse(player.ToString()));
            }

            foreach (var monster in turnController.monsterParty.asList())
            {
                JSONObject monsterObject = new JSONObject();
                monsterObject.Add("type", monster.assetData.name);
                monsterObject.Add("name", monster.name);
                monsterJson.Add(monsterObject);
            }

            JSONObject json = new JSONObject();
            json.Add("state_id", this.stateId);
            json.Add("game_id", this.gameId);
            json.Add("client_id", this.actor.id);
            json.Add("instructionType", this.instructionType);
            json.Add("player_turn_order", turnOrder);
            json.Add("monsters", monsterJson);


            return json.ToString();
        }

        /*---------------------------------------------------------------
                            DESERIALIZATION
         ---------------------------------------------------------------*/
        public BattleMessage(string jsonString)
        {
            this.stateId = (int)State.BATTLE;
            deserialize(jsonString);
        }
        protected override void deserialize(string jsonString)
        {
            base.deserialize(jsonString);

            JSONNode json = JSON.Parse(jsonString);
            this.instructionType = json["instructionType"].AsInt;

            switch ((BattleInstruction)this.instructionType)
            {
                case BattleInstruction.COMBAT_INIT:
                    deserializeCombatInit(json);
                    break;

                case BattleInstruction.TURN_PROGRESSED:
                    //Do Nothing - BattleState can call turnController.takeTurn()
                    break;

                case BattleInstruction.TURN_ACTION:
                    deserializeTurnAction(json);
                    break;

                default:
                    //Do some error checking
                    break;
            }
        }
        private void deserializeTurnAction(JSONNode json)
        {
            abilityTypes = new List<int>();
            targets = new Dictionary<int, float>();

            JSONArray targetsJson = json["targets"].AsArray;

            foreach (JSONNode target in targetsJson)
            {
                targets.Add(target["id"].AsInt, target["remaining_health"].AsFloat);
            }

            abilityTargeting = (TargetingType) json["ability_used"]["targeting_type"].AsInt;

            foreach(JSONNode abilityType in json["ability_used"]["type_ids"])
            {
                abilityTypes.Add(abilityType.AsInt);
            }

            abilityName = json["ability_used"]["name"];
            conditionStrength = new ConditionStrength();

            //conditionStrength.potency = json["ability_used"]
        }

        private void deserializeCombatInit(in JSONNode json)
        {
            //Populate Turn order
            JSONArray turnJson = json["player_turn_order"].AsArray;
            turnOrder = new List<int>();

            foreach (JSONNode turnId in turnJson)
            {
                turnOrder.Add(turnId.AsInt);
            }

            JSONArray monsterJson = json["monsters"].AsArray;
            monsters = new List<KeyValuePair<string, string>>();
            foreach (JSONNode monster in monsterJson)
            {
                monsters.Add(new KeyValuePair<string, string>(monster["type"], monster["name"]));
            }
        }
    }
}

