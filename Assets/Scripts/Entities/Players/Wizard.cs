using Assets.Scripts.Entities.Components;
using Assets.Scripts.Player.Abilities;
using Assets.Scripts.RPA_Entity_Components;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Player_Classes
{
    class Wizard:AdventuringClass
    {
        public static Renderable assetData { get; set; }
        public static string abilityPath { get; set; }
            
        public Wizard(string name) : base(name)
        {
            this.setId(PlayerClasses.WIZARD);
        }
    }
}
