using Assets.Scripts.Entities.Combat;
using Assets.Scripts.Entities.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    class Slug : Monster
    {
        private static Containers.Abilities harpyabilities = new Containers.Abilities(Ability.MONSTER_ABILITY_DATA_PATH + typeof(Slug).Name, CombatantType.MONSTER);

        public Slug()
        {
            name += " Slug";
            setSpriteData(GetType().Name.ToString());
            abilities = new Containers.Abilities(harpyabilities);
            healthProperties.setHealthValues(20);
        }

    }
}
