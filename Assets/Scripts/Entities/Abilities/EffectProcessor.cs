/*---------------------------------------------------------------
                      EFFECT-PROCESSOR
 ---------------------------------------------------------------*/
/***************************************************************
* A singleton for providing functionality for applying any battle
 status effects to a target, and the specific implementation 
 for each effect.
**************************************************************/
using Assets.Scripts.Entities.Components;

namespace Assets.Scripts.Entities.Abilities
{
    public enum StatusEffect
    {
        COOLDOWN_CHANGE = 0,
        REFLECT_DAMAGE = 1,
        DAMAGE_MODIFER = 2,
        STUN = 3,
    }



    public class EffectProcessor
    {
        public static EffectProcessor INSTANCE;

        private EffectProcessor() { }

        public static EffectProcessor getInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new EffectProcessor();
            }
            return INSTANCE;
        }

        /***************************************************************
        * Primary implementation for status effects. Either adds
          the effect to the conditions list of a Combatable or directly
          applies the effect to a target/state of the combat.
         
        @param - target: The target who the effect is being applied to
        @paran - effectId: Numeric identifier for the status effect 
        that an abilility applies.
        @potency - strength of the effect (optional)
        @turnsApplied - The duration of the effect.
        ***************************************************************/
        public void applyEffect(in Combatable target, int effectId, int potency, int turnsApplied)
       {
            switch((StatusEffect)effectId)
            {
                case StatusEffect.COOLDOWN_CHANGE:
                    target.abilities.ForEach(ability => ability.cooldownTracker += potency);
                    break;

                case StatusEffect.REFLECT_DAMAGE:
                    target.conditions.Add(new Condition(effectId, potency, turnsApplied));
                    break;

                case StatusEffect.DAMAGE_MODIFER:
                    applyDamageModifer(target, potency, turnsApplied);

                    break;
            }
        }


        /***************************************************************
        * Checks to only allow for the strongest damage modifier effect 
          to be applied to a target, while allowing for re-application
          of duration to be applied
          to a target.

        @param - target: The target who the effect is being applied to
        @potency - strength of the effect (optional)
        @turnsApplied - The duration of the effect.
        ***************************************************************/
        private void applyDamageModifer(in Combatable target, int potency, int turnsApplied)
        {
            bool isAdding = false;
            bool isUpdating = false;
            int modifiedIndex = -1;
            int positiveEffectIndex = -1;
            int negativeEffectIndex = -1;

            for(int i = 0; i < target.conditions.Count; i++)
            {
                if(negativeEffectIndex > -1 && positiveEffectIndex > -1)
                    break;

                if(target.conditions[i].effectId == (int) StatusEffect.DAMAGE_MODIFER)
                {
                    if(target.conditions[i].potency < 0)  positiveEffectIndex = i;
                    else negativeEffectIndex = i;
                }
            }
      
            if (positiveEffectIndex == -1 && negativeEffectIndex == -1)  //No Damage modifier effect exists - incoming condition can be added
            {
                isAdding = true;
            }
            else if(potency < 0)  // A Buff is being considered, if new value is more powerful update it
            {
                modifiedIndex = positiveEffectIndex;
                isUpdating = potency <= target.conditions[modifiedIndex].potency;
            }
            else if(potency > 0) //A Debuff is being considered, if new value is more powerful update it
            {
                modifiedIndex = positiveEffectIndex;
                isUpdating = potency >= target.conditions[modifiedIndex].potency;
            }

            if (isAdding)
            {
                target.conditions.Add(new Condition((int)StatusEffect.DAMAGE_MODIFER, potency, turnsApplied));
            }
            else if (isUpdating)
            {
                target.conditions[modifiedIndex].extendEffect(turnsApplied);
            }
        }



        /***************************************************************
        * Called when applying a status effect and the UI is required to
         display the labal/description of what an effect does.
         
        @paran - effectId: Numeric identifier for the status effect 
        that an abilility applies.
        ***************************************************************/
        public static string getEffectLabel(int effectId, int potency)
        {
            string value = "";

            switch ((StatusEffect)effectId)
            {
                case StatusEffect.COOLDOWN_CHANGE:
                    value = "Cooldowns " + potency;
                    break;

                case StatusEffect.REFLECT_DAMAGE:
                    value = "Reflect " + potency +  "% ";
                    break;

                case StatusEffect.DAMAGE_MODIFER:
                    value = "Damage " + potency + "% ";
                    break;
            }

            return value;
        }

        /***************************************************************
        * Called when applying a status effect and the UI is required to
         display the labal/description of what an effect does.
         
        @paran - effectId: Numeric identifier for the status effect 
        that an abilility applies.
        ***************************************************************/
        public static string getEffectLabel(int effectId)
        {
            string value = "";

            switch ((StatusEffect)effectId)
            {
                case StatusEffect.REFLECT_DAMAGE:
                    value = "Reflect ";
                    break;
                case StatusEffect.DAMAGE_MODIFER:
                    value = "Damage Modifier ";
                    break;
            }

            return value + " Expired";
        }
    }


}
