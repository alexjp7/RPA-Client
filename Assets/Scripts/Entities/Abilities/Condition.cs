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

        public int reduceEffect(int amount = 1)
        {
            return turnsRemaining -= amount;
        }

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
