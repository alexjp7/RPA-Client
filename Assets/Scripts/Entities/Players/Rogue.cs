/*---------------------------------------------------------------
                        ROGUE
 ---------------------------------------------------------------*/
/***************************************************************
 * Armed with cunning and missdirection, a Rogue's ability to 
   meld in with the shadows and deliver swift and potent strikes 
   while traversing the battlefield unnoticed is unmathced.

* The Rogue player class is based on the slippery and 
  self-relient assassin. Player's who play as the 
  Rogue will the innate ability of evasion and stealth,
  while providing the team with more loot with its
  thievery.

    - Average health points
    - Equipment: Daggers/Bow Wielding
    - Offensive Theme: backstabs, dagger flurys, poison 
    - Defensive Theme: evasion, smoke-bombs, stealth 
    - Utility: restraining/incapacitating enemies, thievery, 
      preparations
**************************************************************/
using Assets.Scripts.RPA_Entity_Components;

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
