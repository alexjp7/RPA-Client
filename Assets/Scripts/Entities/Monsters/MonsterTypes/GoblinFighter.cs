﻿using Assets.Scripts.Entities.Abilities;
using System.Collections.Generic;


namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    class GoblinFighter: Monster
    {
        private static List<Ability> abilityList = AbilityFactory.constructMonsterAbilities(typeof(GoblinFighter).Name);

        public GoblinFighter()
        {
            name += " Goblin Fighter";
            setSpriteData(GetType().Name.ToString());
            abilities = new List<Ability>(abilityList);
            healthProperties.setHealthValues(20);
        }
    }
}
