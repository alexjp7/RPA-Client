namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Components;
    class GoblinFighter: Monster
    {
        private static Containers.Abilities harpyabilities = new Containers.Abilities(Ability.MONSTER_ABILITY_DATA_PATH + typeof(GoblinFighter).Name, CombatantType.MONSTER);

        public GoblinFighter()
        {
            name += " Goblin Fighter";
            setSpriteData(GetType().Name.ToString());
            abilities = new Containers.Abilities(harpyabilities);
            healthProperties.setHealthValues(20);
        }
    }
}
