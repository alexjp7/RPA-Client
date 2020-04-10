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

namespace Assets.Scripts.Entities.Monsters
{
    public class FuzzBall : Monster
    {
        public FuzzBall()
        {   
            this.name += " FuzzBall";
            setId(MonsterTypes.FuzzBall);
            healthProperties.setHealthValues(20);
        }
    }
}
