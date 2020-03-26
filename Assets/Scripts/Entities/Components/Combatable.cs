/*---------------------------------------------------------------
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

namespace Assets.Scripts.Entities.Components
{
    //Generic implementation of ability effect applications
    public abstract class Combatable 
    {
        public int combatOrder { get; set; }
        public Renderable assetData { get; set; }
        public Damageable healthProperties { get; protected set; }
        public List<Ability> abilities { get; protected set; }
        public  string name  {get; protected set;}

        public Combatable()
        {
            assetData = new Renderable();
        }

        /***************************************************************
         * Provides the base logic for dealing damage.
         
         * Applies damage through a min/max bounded randomly generated
          value.

        @param - minDamage: The lower bound of the damage being applied
        that is used to calculate the actual amount dealt.
        @param - maxDamage: The upper bound of the damage being applied
        that is used to calculate the actual amount dealt.

        @return  - whether the damage applied resulted in the 
        combatable's HP dropping below 0 (i.e. is defeated).
        **************************************************************/
        public virtual bool applyDamage(int minDamage, int maxDamage)
        {
            float damageDealt = Util.Random.getInt(minDamage, maxDamage);
            healthProperties.currentHealth -= damageDealt;
            return damageDealt >= (healthProperties.currentHealth + damageDealt);
        }

        /***************************************************************
         * Provides the base logic for applying a hp recovery/healing
           effect to a sprite.

          @param - minHealing: The lower bound of the healing being applied
          that is used to calculate the actual amount healed.
          @param - maxHealing: The upper bound of the healing being applied
          that is used to calculate the actual amount healed.
        **************************************************************/
        public virtual void applyHealing(int minDamage, int maxDamage)
        {
            float damageHealed = Util.Random.getInt(minDamage, maxDamage);
            healthProperties.currentHealth += damageHealed;
            healthProperties.currentHealth = System.Math.Min(healthProperties.currentHealth, healthProperties.maxHealth);
        }

        /***************************************************************
         * Not Implemented Yet
        **************************************************************/
        public virtual void applyDebuff(in Ability ability)
        {
            throw new NotImplementedException("Function: applyDebuf() in object: Combatable");
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
