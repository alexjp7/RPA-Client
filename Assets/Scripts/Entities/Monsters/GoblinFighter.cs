

namespace Assets.Scripts.Entities.Monsters
{
    class GoblinFighter : Monster
    {
        public GoblinFighter() : base()
        {
            this.name += " Goblin Fighter";
            this.setId(MonsterTypes.GOBLIN_FIGHTER);
            healthProperties.setHealthValues(50);
        }
    }
}
