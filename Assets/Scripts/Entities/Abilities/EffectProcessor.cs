using Assets.Scripts.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                    target.abilities.ForEach(ability => ability.cooldownTracker -= potency);
                    break;

            }
       }
    }


}
