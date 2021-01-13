namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Components;

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
