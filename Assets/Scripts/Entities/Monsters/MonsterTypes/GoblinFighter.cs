

using System.Collections.Generic;
using Assets.Scripts.Entities.Abilities;
using UnityEngine;

namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    class GoblinFighter : Monster
    {
        private static List<Ability> goblinFigherAbilities = AbilityFactory.constructMonsterAbilities(typeof(GoblinFighter).Name);

        public GoblinFighter() : base()
        {
            name += " Goblin Fighter";
            setSpriteData((typeof(GoblinFighter).Name));
            abilities = new List<Ability>(goblinFigherAbilities);
            healthProperties.setHealthValues(100);
        }
    }
}
