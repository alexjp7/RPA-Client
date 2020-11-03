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
using System;


namespace Assets.Scripts.Entities.Abilities
{
    /// <summary>
    /// Status effect types
    /// </summary>
    public enum StatusEffect
    {
        //One-Time Use
        COOLDOWN_CHANGE = 0,
        //Application Effects
        REFLECT_DAMAGE = 1,
        DAMAGE_TAKEN_UP = 2, DAMAGE_TAKEN_DOWN = 3,
        BLEED = 5,
        POISON = 6, POISON_WEAPON = 7,
        SLEEP = 8
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

        public static StatusEffect[] PreConditionEffects =
        {
            StatusEffect.BLEED,
            StatusEffect.POISON,
        };

        public static StatusEffect[] ImpairingEffects =
        {
            StatusEffect.SLEEP
        };


        /// <summary>
        /// Primary implementation for status effects. Either adds the effect to the conditions list of a Combatant or directly applies the effect to a target/state of the combat.     
        /// 
        /// </summary>
        /// <param name="target"> The combatant whom will recieve the effect.</param>
        /// <param name="effectId"> The status effect ID that will be applied to the target </param>
        /// <param name="potency">  strength of the effect (optional)</param>
        /// <param name="turnsApplied"> The duration of the effect.</param>
        public void applyEffect(in Combatant target, int effectId, int potency, int turnsApplied)
        {
            switch ((StatusEffect)effectId)
            {
                /******IMEDIATE EFFECT*******/
                case StatusEffect.COOLDOWN_CHANGE:
                    target.abilities.ForEach(ability => ability.cooldownTracker += potency);
                    break;

                case StatusEffect.REFLECT_DAMAGE: //Add new effect, or extend effect if exists
                    if (target.conditions.ContainsKey(effectId)) target.conditions[effectId].extendEffect(turnsApplied);
                    else target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied));
                    break;

                case StatusEffect.DAMAGE_TAKEN_UP:
                    if (target.conditions.ContainsKey(effectId)) target.conditions[effectId].extendEffect(turnsApplied);
                    else target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied));
                    break;

                case StatusEffect.DAMAGE_TAKEN_DOWN:
                    if (target.conditions.ContainsKey(effectId)) target.conditions[effectId].extendEffect(turnsApplied);
                    else target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied));
                    break;

                /******IMPAIRING-EFFECTS*******/
                case StatusEffect.SLEEP:
                    applyDamageModifer(target, effectId, potency, turnsApplied);
                    break;

                /******PRE-TURN EFFECT*******/
                //Stacking effects
                case StatusEffect.BLEED:
                    if (target.conditions.ContainsKey(effectId))
                    {
                        //Only apply the longer duration
                        if (turnsApplied > target.conditions[effectId].turnsRemaining)
                        {
                            target.conditions[effectId].turnsRemaining = turnsApplied;
                        }

                        target.conditions[effectId].stacks++;
                    }
                    else target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied, 1));
                    break;

                case StatusEffect.POISON_WEAPON:
                    if (target.conditions.ContainsKey(effectId)) target.conditions[effectId].extendEffect(turnsApplied);
                    else target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied));
                    break;

                case StatusEffect.POISON:
                    if (target.conditions.ContainsKey(effectId))
                    {
                        //Only apply the longer duration
                        if (turnsApplied > target.conditions[effectId].turnsRemaining)
                        {
                            target.conditions[effectId].turnsRemaining = turnsApplied;
                        }

                        target.conditions[effectId].stacks++;
                    }
                    else target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied, 1));
                    break;

                default:
                    throw new ArgumentException($"Unexpected status effect id = {effectId} being applied to {target.name} - Check the caster's <caster_entity>.json ");
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
        private void applyDamageModifer(in Combatant target, int effectId, int potency, int turnsApplied)
        {
            bool isExtending = false;

            if (effectId == (int)StatusEffect.DAMAGE_TAKEN_UP)
            {
                if (isExtending = target.conditions.ContainsKey(effectId))
                {
                    target.conditions[effectId].extendEffect(turnsApplied);
                    if (potency > target.conditions[effectId].potency)
                    {
                        target.conditions[effectId].potency = potency;
                    }
                }
            }
            else if (effectId == (int)StatusEffect.DAMAGE_TAKEN_DOWN)
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

            if (!isExtending)
            {
                target.conditions.Add(effectId, new Condition(effectId, potency, turnsApplied));
            }
        }

        /// <summary>
        /// Called when applying a status effect and the UI is required to  display the labal/description of what an effect does.
        /// </summary>
        /// <param name="effectId"> Numeric identifier for the status effect that an abilility applies.</param>
        /// <param name="potency"> The strength of the effect </param>
        /// <returns></returns>
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

                case StatusEffect.BLEED:
                    value = $"bleed";
                    break;

                case StatusEffect.POISON_WEAPON:
                    value = $"poison weapon";
                    break;

                case StatusEffect.POISON:
                    value = $"poison";
                    break;

                case StatusEffect.SLEEP:
                    value = $"sleep";
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
