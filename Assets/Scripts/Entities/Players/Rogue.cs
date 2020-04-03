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

using Assets.Scripts.Entities.Components;

namespace Assets.Scripts.Entities.Players
{
    class Rogue : Adventurer
    {
        public static Renderable staticAssets { get; set; }
        public static string abilityPath { get; set; }

        public Rogue(string name) : base(name)
        {
            this.setId(PlayerClasses.ROGUE);

        }
    }
}
