/*---------------------------------------------------------------
                       DAMAGEABLE
 ---------------------------------------------------------------*/
/***************************************************************
 * The Damageable object represents a component that is used
   to contain both meta and instanced based health properties.

*  A class that is composed of a Damageable object will 
   inherently be able to have operations completed on max
   and current health points, including damageing and healing.
**************************************************************/

namespace Assets.Scripts.Entities.Components
{
    using Assets.Scripts.Entities.Players;
    public class Damageable
    {
        /// <summary>
        /// <para>
        /// The HealthModifiers struct contains meta-data relating to the innate health modifiers that are applied to each class.
        /// Warrior, Wizard, Cleric and Rogue.
        /// </para>
        /// These values are declared within a struct to create a centralised component for modifiers which are persistent throughout the game.
        /// </summary>
        struct HealthModifiers
        {
            public static readonly float WARRIOR_MOD = 0.15f;
            public static readonly float WIZARD_MOD = -0.15f;
            public static readonly float CLERIC_MOD = -0.10f;
            public static readonly float ROGUE_MOD = 0.0f;
        }

        public static readonly float PLAYER_BASE_HP = 50;

        /// <summary>
        /// Maximun health value of the damageable object.
        /// </summary>
        public float maxHealth { get; set; }
        /// <summary>
        /// The current health value of the damageable object.
        /// </summary>
        public float currentHealth { get; set; }

        public Damageable() { }

        /// <summary>
        /// Constructs a Damageable component for a player object.
        /// </summary>
        /// <param name="classId">The player class <see cref="Assets.Scripts.Entities.Players.PlayerClasses"/> that represents the adventuring class chosen by a player. 
        /// <para>
        /// This class id is used to determine which
        /// modifier to use and set the maximun and current health values to.
        /// </para> 
        /// </param>
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

        /// <summary>
        /// Construction of a Damageable component for a Monster object.
        /// </summary>
        /// <param name="maxHealth"> The max health of a monster which should be declared within the monster class.</param>
        public Damageable(float maxHealth)
        {
            setHealthValues(maxHealth);
        }

        /// <summary>
        /// Sets the max health and current health values to the passed in parameter.
        /// </summary>
        /// <param name="maxHealth">Maximun health value of the damageable object.</param>
        public void setHealthValues(float maxHealth)
        {
            this.maxHealth = maxHealth;
            currentHealth = maxHealth;
        }

        /// <summary>
        /// Provides the health value as a ratio of current health/max health.
        /// </summary>
        /// <returns>The health value as a decimal value (0-1).</returns>
        public float getHealthPercentage() 
        { 
            return (this.currentHealth / this.maxHealth);
        }
    }
}
