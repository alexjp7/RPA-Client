

namespace Assets.Scripts.Entities.Monsters
{
    class GoblinFighter : Monster
    {
        public GoblinFighter() : base()
        {
            this.name += " Goblin Fighter";
            this.setId(MonsterTypes.GoblinFighter);
            healthProperties.setHealthValues(50);
        }
    }
}
