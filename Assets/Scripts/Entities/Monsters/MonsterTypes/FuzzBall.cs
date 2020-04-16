﻿using Assets.Scripts.Entities.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    class FuzzBall : Monster
    {
        private static List<Ability> abilityList = AbilityFactory.constructMonsterAbilities(typeof(FuzzBall).Name);

        public FuzzBall()
        {
            name += " FuzzBall";
            setSpriteData(GetType().Name.ToString());
            abilities = new List<Ability>(abilityList);
            healthProperties.setHealthValues(20);
        }

    }
}
