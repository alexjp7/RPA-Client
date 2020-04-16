using Assets.Scripts.Entities.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    class Harpy : Monster
    {
        private static List<Ability> harpyAbilities = AbilityFactory.constructMonsterAbilities(typeof(Harpy).Name);

        public Harpy()
        {
            name += " Harpy";
            setSpriteData(GetType().Name.ToString());
            abilities = new List<Ability>(harpyAbilities);
            healthProperties.setHealthValues(20);
        }

    }
}
