using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entities.Abilities
{
    /***************************************************************
    * Container for storing applied effects on a Combatable, including
      how long it lasts for (in turns) and how strong is the effect is.
    ***************************************************************/
    public class Condition
    {
        public int effectId { get;}
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
