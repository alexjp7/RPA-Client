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
        //One-Time Use
        COOLDOWN_CHANGE = 0,
        //Application Buffs
        REFLECT_DAMAGE = 1,
        DAMAGE_TAKEN_UP = 2,
        DAMAGE_TAKEN_DOWN = 3,
        STUN = 5,
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

                case StatusEffect.REFLECT_DAMAGE: //Add new effect, or extend effect if exists
                    if (target.conditions.ContainsKey(effectId)) target.conditions[effectId].extendEffect(turnsApplied);
                    else target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied));
                    break;

                case StatusEffect.DAMAGE_TAKEN_UP:
                    applyDamageModifer(target, effectId, potency, turnsApplied);
                    break;

                case StatusEffect.DAMAGE_TAKEN_DOWN:
                    applyDamageModifer(target, effectId, potency, turnsApplied);
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
        private void applyDamageModifer(in Combatable target, int effectId ,int potency, int turnsApplied)
        {
            bool isExtending = false;

            if(effectId == (int) StatusEffect.DAMAGE_TAKEN_UP)
            {
                if (isExtending = target.conditions.ContainsKey(effectId) )
                {
                    target.conditions[effectId].extendEffect(turnsApplied);
                    if (potency > target.conditions[effectId].potency)
                    {
                        target.conditions[effectId].potency = potency; 
                    }
                }
            }
            else if (effectId == (int) StatusEffect.DAMAGE_TAKEN_DOWN)
            {
                if (isExtending = target.conditions.ContainsKey(effectId))
                {
                    target.conditions[effectId].extendEffect(turnsApplied);
                    if (potency < target.conditions[effectId].potency)
                    {
                        target.conditions[effectId].potency = potency;
                    }
                }
            }

            if(!isExtending)
            {
                target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied));
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
                    value = $"dmg {potency}% Damage";
                    break;

                case StatusEffect.DAMAGE_TAKEN_UP:
                    value = $"+{potency}% incoming dmg ";
                    break;

                case StatusEffect.DAMAGE_TAKEN_DOWN:
                    value = $"{potency}% incoming dmg ";
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
                    value = "reflect DMG";
                    break;
                case StatusEffect.DAMAGE_TAKEN_UP:
                    value = "+ incoming DMG";
                    break;

                case StatusEffect.DAMAGE_TAKEN_DOWN:
                    value = "- incoming DMG";
                    break;
            }

            return value + " Expired";
        }
    }


}
