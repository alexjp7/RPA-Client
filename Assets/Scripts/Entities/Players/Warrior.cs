
using Assets.Scripts.Entities.Components;
using Assets.Scripts.RPA_Entity_Components;

namespace Assets.Scripts.Player_Classes
{
    public class Warrior : AdventuringClass
    {
        public static Renderable assetData { get; set; }
        public static string abilityPath { get; set; }

        public Warrior(string name) : base (name)
        {
            this.setId(PlayerClasses.WARRIOR);
        }
    }
}
