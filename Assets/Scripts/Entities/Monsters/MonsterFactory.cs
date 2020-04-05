/*---------------------------------------------------------------
                       MONSTER-FACTORY
 ---------------------------------------------------------------*/
/***************************************************************
* This factory class provides helper and construction methods
  that allow for monster party creation.

* the public createMonsterParty() is the main method called
  in this factory, and returns the fully constructed list of 
  monsters, including their; hp, abilities, combat behaviours,
  asset/sprite data.

* This factory will eventually include the creation of sub-types
  of monsters, and will either delegate construction to a
  sub-type factory, or include private sub-type construction
  logic.
**************************************************************/

using Assets.Scripts.RPA_Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Entities.Monsters
{
    public class MonsterFactory
    {
        /***************************************************************
        * Instantiates a list of monsters.
        @param - amount: the amount of monsters desired to be made.
        
        @return - The list of monster objects used in the battle state.
        **************************************************************/
        public List<Monster> createMonsterParty(int amount)
        {
            int randomMonsterIndex = Util.Random.getInt(Enum.GetNames(typeof(MonsterTypes)).Length);
            List<Monster> monsterParty = new List<Monster>();
            for (int i = 0; i < amount; i++)
            {
                Monster monster = getMonster();
                if(monster != null) monsterParty.Add(monster);
            }

            return monsterParty;
        }

        /***************************************************************
        @return - a single random mosnter from the 
        **************************************************************/
        private Monster getMonster()
        {
            Monster randomMonster = null;
            int randomMonsterIndex = Util.Random.getInt(Enum.GetNames(typeof(MonsterTypes)).Length);
            switch ((MonsterTypes)randomMonsterIndex)
            {
                case MonsterTypes.FUZZBALL:
                    randomMonster = new FuzzBall();
                    break;

                case MonsterTypes.GOBLIN_FIGHTER:
                    randomMonster = new GoblinFighter();
                    break;
            }

            //Scale monsters based on party size
            randomMonster.healthProperties.maxHealth = randomMonster.getMaxHp() + (randomMonster.getMaxHp() * Game.players.Count());
            randomMonster.healthProperties.currentHealth = randomMonster.healthProperties.maxHealth;
            return randomMonster;
        }
    }
}
