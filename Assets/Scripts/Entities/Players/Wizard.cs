﻿/*---------------------------------------------------------------
                        WIZARD
 ---------------------------------------------------------------*/
/***************************************************************
* Intellect and wisdom guide your desire to unleash your innate
  command over the elements of the natural and unatural world. 
  a Wizard's vision into the future is perceptive and clear,
  armed with prediction and time manipulation spells.

* The Wizard player class is based on the classic
  spellcaster and cojurer of elemental arts. Player's 
  who play as the Wizard will have unlocked inante arcane
  and elemental spellcasting, unique buffs for their party 
  through time distortive cooldown reductions and combat
  predictions.

    - Lowest health points
    - Equipment: Staff/wand/orb wielding
    - Offensive Theme: Elemental/arcane bolts, bursts,
      waves and volleys (not limited to).
    - Defensive Theme:  Gravity/time impairments (on enemies),
      projectile warding arcane barriers
    - Utility: Time distortion (cooldown restoration), 
      illusions, sigils(toggles?)
**************************************************************/
namespace Assets.Scripts.Entities.Players
{
    using Assets.Scripts.Entities.Components;
    class Wizard : Adventurer
    {
        public static Renderable staticAssets { get; set; }
        public static string abilityPath { get; set; }
            
        public Wizard(string name, int id) : base(name, id)
        {
            this.setCommonData(PlayerClasses.WIZARD);
        }
    }
}
