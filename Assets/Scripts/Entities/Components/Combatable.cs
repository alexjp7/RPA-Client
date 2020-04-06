﻿/*---------------------------------------------------------------
                       COMBATABLE
 ---------------------------------------------------------------*/
/***************************************************************
*  The Combatable type contains fields relating to combat
   including; turn order, health properties (Damageable), abilities
   and the base logic for applying damage, healing and status 
   effects to these types of objects. The combatable base
   provides a abstract type to allow for generic handling
   of both player and monster types.
**************************************************************/

using System;
using System.Collections.Generic;
using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.UI;

namespace Assets.Scripts.Entities.Components
{
    public enum CombatantType
    {
        PLAYER,
        MONSTER
    }

    public abstract class Combatable
    {
        private readonly string BASE_SPRITE_PATH = "textures/sprite_textures/";

        public string name { get; protected set;}
        public CombatantType type { get; protected set;}
        protected string assetPath { get; set; }

        //Components
        private CombatSprite _combatSprite; //Instance holder
        public Renderable assetData { get; set; }
        public Damageable healthProperties { get; protected set; }
        public List<Ability> abilities { get; protected set; }

        public CombatSprite combatSprite
        {
            get
            {
                if (_combatSprite == null)
                {
                    _combatSprite = CombatSprite.create(this);
                }
                return _combatSprite;
            }
        }

        //Default Constructor
        public Combatable()
        {
            assetPath = BASE_SPRITE_PATH;
            assetData = new Renderable();
        }

        /***************************************************************
        * sets the path to load/retrieve the asset of this combatable.
         
        @param - spriteName: The toString() value of the player/monster
        enum.
        **************************************************************/
        protected void setSpritePath(string spriteName)
        {
            assetData.spritePath = assetPath + spriteName;
        }

        /***************************************************************
        * Provides the base logic for dealing damage.
         
        @param - minDamage: The lower bound of the damage being applied
        that is used to calculate the actual amount dealt.
        @param - maxDamage: The upper bound of the damage being applied
        that is used to calculate the actual amount dealt.

        @return  - The damage result.
        **************************************************************/
        public virtual int applyDamage(int minDamage, int maxDamage)
        {
            float damageDealt = Util.Random.getInt(minDamage, maxDamage);
            healthProperties.currentHealth -= damageDealt;
            
            return (int) damageDealt;
        }

        /***************************************************************
        * Provides the base logic for applying a hp recovery/healing
          effect to a sprite.

        @param - minHealing: The lower bound of the healing being applied
        that is used to calculate the actual amount healed.
        @param - maxHealing: The upper bound of the healing being applied
        that is used to calculate the actual amount healed.

        @return  - The healing result.
        **************************************************************/
        public virtual int  applyHealing(int minValue, int maxValue)
        {
            float damageHealed = Util.Random.getInt(minValue, maxValue);
            healthProperties.currentHealth += damageHealed;
            healthProperties.currentHealth = Math.Min(healthProperties.currentHealth, healthProperties.maxHealth);
            return (int) damageHealed;
        }

        /***************************************************************
        * Applies the status effect to this combatable.
        @param - statusEffect: numeric ID for an ability status effect.
        see EffectProcess.cs.
        @param - potency: The strength or duration if applicable of the 
        status effect.
        **************************************************************/
        public virtual void applyEffect(int statusEffect, int potency)
        {
            EffectProcessor.getInstance().applyEffect(this, statusEffect, potency);
        }   

        /***************************************************************
         * Updates cooldown counters for each ability.
       
        @param - effectiveTurnCount: the amount turns taken
        by a combatable.
        **************************************************************/
        public virtual void updateAbilityCooldowns()
        {
            foreach(Ability ability in abilities)
            {
                ability.updateCooldown();
            }
        }

        /***************************************************************
        @return  - The value of the Combatables MAX health.
        **************************************************************/
        public float getMaxHp() { return healthProperties.maxHealth;}

        /***************************************************************
        @return  - The value of the Combatables CURRENT health.
        **************************************************************/
        public float getCurrentHp() { return healthProperties.currentHealth;}

        /***************************************************************
        @return  - true: this combatable is alive
                   false: this comtable is dead, and is now ready to be
                   removed from combat.
        **************************************************************/
        public bool isAlive() { return healthProperties.currentHealth > 0;}

        /***************************************************************
        * Helper function used to determine the fill amount of a player
           or monsters HP bar.
        
        * This function call delegates the processing of the current entities 
          health percentage to a Damageable component.
       
        @return  - The health value as a decimal value (0-1).
        **************************************************************/
        public float getHealthPercent() { return healthProperties.getHealthPercentage(); }
    }
}
