using Assets.Scripts.Entities.Components;
using Assets.Scripts.RPA_Entity_Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Player_Classes
{
    class Rogue: AdventuringClass
    {
        public static Renderable assetData { get; set; }
        public static string abilityPath { get; set; }

        public Rogue(string name) : base(name)
        {
            this.setId(PlayerClasses.ROGUE);
        }
    }
}
