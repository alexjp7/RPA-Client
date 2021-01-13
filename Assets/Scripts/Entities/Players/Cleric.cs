
/*---------------------------------------------------------------
                        CLERIC
 ---------------------------------------------------------------*/
/***************************************************************
 * Devoted to purity and prosperity, a Cleric's devotion to 
   resplendant healing and protective magics restore those 
   around them and foil those who are unfaithfull.

* The Cleric player class themetically revolves around
  a devoted follower of the divine and holy. Player's
  who play as the Cleric will have innate abilities
  of healing and restoration, while also providing
  devestating smites and other offensive capabilties
  and buffs.

    - Slightly below average health points
    - Equipment: Mace/holy Symbol wielding
    - Offensive Theme: Holy rays, divine burst, smites
    - Defensive Theme: healing, devotion
    - Utility: resurrection, wound recovery (post combat),
      divine guidance(higher chance to crit)
**************************************************************/
namespace Assets.Scripts.Entities.Players
{
    using Assets.Scripts.Entities.Components;
    class Cleric : Adventurer
    {
        public static Renderable staticAssets { get; set; }
        public static string abilityPath { get; set; }

        public Cleric(string name, int id) : base(name, id)
        {
            this.setCommonData(PlayerClasses.CLERIC);
        }
    }
}
