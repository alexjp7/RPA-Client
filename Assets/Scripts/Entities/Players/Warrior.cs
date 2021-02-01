/*---------------------------------------------------------------
                        WARRIOR
 ---------------------------------------------------------------*/
/***************************************************************
* With your brute stength combined with your unwavering valor, 
  warriors will serve their party in the front lines through
  devastating swings and severring slashes, controlling the 
  flow and pace of the battle by the virtue of their chilling 
  battlescries and taunts.

* The Warrior player class is architected from a bruting
  and menacing melee champion. Player's who play as the 
  Warrior will inturn choose a more direct playstyle
  with devestating slashes, battle commands(buffs)
  and opportunties to mitigate damage for its team.

    - Highest health points
    - Equipment: Sword/shield wielding
    - Offensive Theme: Sword techniques, brute force
    - Defensive Theme:  Self-fortification, Shield stances
    - Utility: morale boost, battle cries, taunts

**************************************************************/
namespace Assets.Scripts.Entities.Players
{
    using Assets.Scripts.Entities.Components;
    using Assets.Scripts.Util;

    public class Warrior : Adventurer
    {
        public static Renderable staticAssets { get; set; }
        public static string abilityPath { get; set; }

        public Warrior(string name, int id) : base (name, id)
        {
            characterRigRef = GameAssets.INSTANCE.warriorRig;
            this.setCommonData(PlayerClasses.WARRIOR);
        }
    }
}
