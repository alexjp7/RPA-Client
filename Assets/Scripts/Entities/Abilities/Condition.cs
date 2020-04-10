﻿/*---------------------------------------------------------------
                        CONDITION
 ---------------------------------------------------------------*/
/***************************************************************
* Container for storing applied effects on a Combatable, including
  how long it lasts for (in turns) and how strong is the effect is.
***************************************************************/

namespace Assets.Scripts.Entities.Abilities
{

    public class Condition
    {
        public int effectId { get; private set; }
        public int turnsRemaining { get; private set; }
        public int potency { get; private set; }

        public Condition(int _effectId, int _potency, int _turnsRemaining)
        {
            effectId = _effectId;
            potency = _potency;
            turnsRemaining = _turnsRemaining;
        }

        public int reduceEffect(int amount = 1)
        {
            return turnsRemaining -= amount;
        }

        public int extendEffect(int amount)
        {
            return turnsRemaining += amount;
        }
    }
}
