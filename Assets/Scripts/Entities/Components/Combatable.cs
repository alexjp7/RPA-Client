using Assets.Scripts.Player.Abilities;
using System.Collections.Generic;
using Assets.Scripts.RPA_Message;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.Entities.Components
{
    //Generic implementation of ability effect applications
    public abstract class Combatable 
    {
        public int combatOrder { get; set; }
        public Damageable healthProperties { get; protected set; }
        public List<Ability> abilities { get; protected set; }
        public  string name  {get; protected set;}

        public virtual bool applyDamage(int minDamage, int maxDamage)
        {
            float damageDealt = Util.Random.getInt(minDamage, maxDamage);
            healthProperties.currentHealth -= damageDealt;
            return damageDealt >= (healthProperties.currentHealth + damageDealt);
        }

        public virtual void applyHealing(int minDamage, int maxDamage)
        {
            float damageHealed = Util.Random.getInt(minDamage, maxDamage);
            healthProperties.currentHealth += damageHealed;
            healthProperties.currentHealth = System.Math.Min(healthProperties.currentHealth, healthProperties.maxHealth);
        }

        public virtual void applyDebuff(in Ability ability)
        {

        }



        public float getMaxHp()
        {
            return healthProperties.maxHealth;
        }

        public float getCurrentHp()
        {
            return healthProperties.currentHealth;
        }

        public bool isAlive()
        {
            return healthProperties.currentHealth > 0;
        }


        public float getHealthPercent() { return healthProperties.getHealthPercentage(); }

    }
}
