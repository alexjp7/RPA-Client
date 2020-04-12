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

using Assets.Scripts.Entities.Abilities;
using System.Collections.Generic;

namespace Assets.Scripts.Entities.Monsters.MonsterTypes
{
    public class FuzzBall : Monster
    {
        private static List<Ability> fuzzBallAbilities = AbilityFactory.constructMonsterAbilities(typeof(FuzzBall).Name);

        public FuzzBall()
        {   
            name += " FuzzBall";
            setSpriteData((typeof(FuzzBall).Name));
            abilities = new List<Ability>(fuzzBallAbilities);
            healthProperties.setHealthValues(20);
        }
              
    }
}
