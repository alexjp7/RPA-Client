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
using System;

namespace Assets.Scripts.RPA_Messages
{
    public class BattleStateMessage : Message
    {
        //Battle State Messages
        public int targetId;
        public bool isTargetEnemy;

        //Ability Fields
        public int[] typeIds;
        public string abilityName;
        //Damage Ability Fields
        public float damage;

        //Non-Damage Fields
        public float potency;
        //Debuff Fields
        public int[] conditionIds;
        //Buff Fields
        public int[] buffIds;

        //Multi-target Fields
        public int numberOfTargets;

        protected override void deserialize(string jsonString)
        {
            base.deserialize(jsonString);
        }

        protected override void serialize()
        {
            throw new NotImplementedException();
        }
    }

}

