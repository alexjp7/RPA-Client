

using Assets.Scripts.Entities.Components;
using Assets.Scripts.Player.Abilities;
using Assets.Scripts.RPA_Entity_Components;
using System.Collections.Generic;
using Assets.Scripts.Util;

namespace Assets.Scripts.Monsters
{
    public enum MonsterTypes
    {
        FUZZBALL = 0
    }

    public abstract class Monster : Combatable
    {
        public MonsterTypes monsterId { get; protected set; }
        public Renderable assetData;
        public List<Ability> abilities { get; protected set; }
        
        public static float BASE_HP; //Read from file ?

        public Monster()
        {
            this.name = getNamePrefix();
            this.abilities = new List<Ability>();
        }

        protected void setId(MonsterTypes monsterId)
        {
            this.monsterId = monsterId;
            this.healthProperties = new Damageable(monsterId);
        }

        private string getNamePrefix()
        {
            string[] namePrefixes = {"Homeless",
                                     "Self-Employed",
                                     "Hungry",
                                     "Uneducated",
                                     "Clueless",
                                     "Silly"};

            return namePrefixes[Random.getInt(namePrefixes.Length)];
        }

        public abstract int getTarget();
        public abstract Ability selectAbility();

    }
}
