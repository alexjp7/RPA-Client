using Assets.Scripts.Entities.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    class Slug : Monster
    {
        private static List<Ability> abilityList = AbilityFactory.constructMonsterAbilities(typeof(Slug).Name);

        public Slug()
        {
            name += " Slug";
            setSpriteData(GetType().Name.ToString());
            abilities = new List<Ability>(abilityList);
            healthProperties.setHealthValues(20);
        }

    }
}
