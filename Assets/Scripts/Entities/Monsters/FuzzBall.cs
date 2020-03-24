/*---------------------------------------------------------------
                        FUZZBALL
 ---------------------------------------------------------------*/
/***************************************************************
* The intention for the FuzzBall monster type is to have it
  as one of the starting monsters the players will have to face
  off against.

* The Fuzzball will have simple mechanics and attack patterns
  and will hopefully serve as an introductory enemy in the game.

 * The FuzzBall monster is currently serving as a placeholder
   enemy for developing key features/systems of the monter party.
  **************************************************************/
using Assets.Scripts.Monsters;
using Assets.Scripts.Player.Abilities;
using System;

namespace Assets.Scripts.Entities.Monsters
{
    public class FuzzBall : Monster
    {

        public FuzzBall()
        {
            this.name += " FuzzBall";
            this.setId(MonsterTypes.FUZZBALL);
        }


        public override int getTarget()
        {
            throw new NotImplementedException();
        }

        public override Ability selectAbility()
        {
            throw new NotImplementedException();
        }
    }
}
