﻿/*---------------------------------------------------------------
                       DAMAGEABLE
 ---------------------------------------------------------------*/
/***************************************************************
 * The Damageable object represents a component that is used
   to contain both meta and instanced based health properties.

*  A class that is composed of a Damageable object will 
   inherently be able to have operations completed on max
   and current health points, including damageing and healing.
**************************************************************/

using Assets.Scripts.Entities.Players;

namespace Assets.Scripts.Entities.Components
{
    public class Damageable
    {
        /***************************************************************
        *  The HealthModifiers struct contains meta-data relating to the
           innate health modifiers that are applied to each class.

        *  These values are declared within a struct to create a centralised 
           component for modifiers which are persistent throughout the game.
        **************************************************************/
        struct HealthModifiers
        {
            public static readonly float WARRIOR_MOD = 0.15f;
            public static readonly float WIZARD_MOD = -0.15f;
            public static readonly float CLERIC_MOD = -0.10f;
        }

        public static readonly float PLAYER_BASE_HP = 50;
        public float maxHealth { get; set; }
        public float currentHealth { get; set; }

        public Damageable()  {}

        /***************************************************************
        * Construction of a Damageable component for a player object.

        @param - classId: an enumerated type that represents the adventuring
        class chosen by a player. This class id is used to determine which
        modifier to use and set the maximun and current health values to.
        **************************************************************/
        public Damageable(PlayerClasses classId)
        {
            switch (classId)
            {
                case PlayerClasses.WARRIOR:
                    this.maxHealth = PLAYER_BASE_HP + (HealthModifiers.WARRIOR_MOD * PLAYER_BASE_HP);
                    break;

                case PlayerClasses.WIZARD:
                    this.maxHealth = PLAYER_BASE_HP + (HealthModifiers.WIZARD_MOD * PLAYER_BASE_HP);
                    break;

                case PlayerClasses.ROGUE:
                    this.maxHealth = PLAYER_BASE_HP;
                    break;

                case PlayerClasses.CLERIC:
                    this.maxHealth = PLAYER_BASE_HP + (HealthModifiers.CLERIC_MOD * PLAYER_BASE_HP);
                    break;
            }

            this.currentHealth = this.maxHealth;
        }

        /***************************************************************
        * Construction of a Damageable component for a Monster object.
        **************************************************************/
        public Damageable(float maxHealth)
        {
            setHealthValues(maxHealth);
        }

        public void setHealthValues(float maxHealth)
        {
            this.maxHealth = maxHealth;
            currentHealth = maxHealth;
        }
    
        /***************************************************************
        @return  - The health value as a decimal value (0-1).
        **************************************************************/
        public float getHealthPercentage() {return (this.currentHealth / this.maxHealth);}
    }
}
