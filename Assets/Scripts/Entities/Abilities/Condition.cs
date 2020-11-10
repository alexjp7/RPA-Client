/*---------------------------------------------------------------
                        CONDITION
 ---------------------------------------------------------------*/
/***************************************************************
* Container for storing applied effects on a Combatant, including
  how long it lasts for (in turns) and how strong is the effect is.
***************************************************************/

namespace Assets.Scripts.Entities.Abilities
{

    public class Condition
    {
        public int effectId { get; private set; }
        public int turnsRemaining { get; set; }
        public int stacks { get; set; }
        public int potency { get; set; }

        public Condition(int _effectId, int _potency, int _turnsRemaining, int _stacks = 0)
        {
            effectId = _effectId;
            potency = _potency;
            turnsRemaining = _turnsRemaining;
            stacks = _stacks;
        }

        /// <summary>
        /// Reduces a status effects turn count. When a turn count hits 0, the caller logic should remove the effect from the combatant.
        /// </summary>
        /// <param name="amount"> The number of turns to reduce the effects turn duration by. </param>
        /// <returns> The remaining turns that the effect will remain on the combatant</returns>
        public int reduceEffect(int amount = 1)
        {
            return turnsRemaining -= amount;
        }

        /// <summary>
        /// Increases the effects turn count.
        /// </summary>
        /// <param name="amount">The number of turns to increase the effects turn duration by. </param>
        /// <returns>The remaining turns that the effect will remain on the combatant</returns>
        public int extendEffect(int amount)
        {
            return turnsRemaining += amount;
        }

        public override string ToString()
        {
            return "Effect Id " + effectId + " Turns Remaining " + turnsRemaining + " potency" + potency;
        }
    }
}
