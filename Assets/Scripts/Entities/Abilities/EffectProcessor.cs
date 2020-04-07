/*---------------------------------------------------------------
                      EFFECT-PROCESSOR
 ---------------------------------------------------------------*/
/***************************************************************
* A singleton for providing functionality for applying any battle
 status effects to a target, and the specific implementation 
 for each effect.
**************************************************************/
using Assets.Scripts.Entities.Components;
using UnityEngine;

namespace Assets.Scripts.Entities.Abilities
{
    public enum StatusEffect
    {
        COOLDOWN_CHANGE = 0,
        REFLECT_DAMAGE = 1,
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
        @turnsApplied - The duration of the effect (optional).
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
            }

            return value + " Expired";
        }
    }


}
