namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Components;

    class Harpy : Monster
    {
        private static Containers.Abilities harpyabilities = new Containers.Abilities(Ability.MONSTER_ABILITY_DATA_PATH + typeof(Harpy).Name,  CombatantType.MONSTER);

        public Harpy()
        {
            name += " Harpy";
            setSpriteData(GetType().Name.ToString());
            abilities = new Containers.Abilities(harpyabilities);
            healthProperties.setHealthValues(20);
        }

    }
}
