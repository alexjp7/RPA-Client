using Assets.Scripts.Player_Classes;
using System;
using Assets.Scripts.Monsters;

namespace Assets.Scripts.Entities.Components
{
    public class Damageable
    {
        //Structure to hold all static modifiers to HP values of classes
        struct HealthModifiers
        {
            public static readonly float WARRIOR_MOD = 0.15f;
            public static readonly float WIZARD_MOD = -0.15f;
            public static readonly float CLERIC_MOD = -0.10f;
        }

        public static readonly float PLAYER_BASE_HP = 50;
        public float maxHealth { get; set; }
        public float currentHealth { get; set; }

        //Constructor for Players
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

        //Constructor for Monsters
        public Damageable(MonsterTypes monsterId)
        {
            switch (monsterId)
            {
                case MonsterTypes.FUZZBALL:
                    this.maxHealth =  20 + (20 * (Game.connectedPlayers - 1));
                    break;
            }

            this.currentHealth = this.maxHealth;
        }

        public void recieveDamage(float damage){throw new NotImplementedException();}
        public float getHealthPercentage() {return (this.currentHealth / this.maxHealth);}
    }
}
