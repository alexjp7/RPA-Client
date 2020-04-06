using Assets.Scripts.Entities.Components;
using UnityEngine;

namespace Assets.Scripts.Entities.Abilities
{
    public enum StatusEffect
    {
        COOLDOWN_CHANGE = 0,
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

       public void applyEffect(in Combatable target, int effectId, int potency)
       {
            switch((StatusEffect)effectId)
            {
                case StatusEffect.COOLDOWN_CHANGE:
                    target.abilities.ForEach(ability => ability.cooldownTracker += potency);
                    break;

            }
       }

        public static string getEffectLabel(int effectId)
        {
            string value = "";

            switch ((StatusEffect)effectId)
            {
                case StatusEffect.COOLDOWN_CHANGE:
                    value = "Cooldowns";
                    break;

            }

            return value;
        }
    }


}
