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
using System;
using System.Collections.Generic;
using Assets.Scripts.Entities.Monsters;

namespace Assets.Scripts.Monsters
{
    public class MonsterFactory
    {
        public static readonly int MONSTER_PARTY_LIMIT = 4;
        public List<Monster> createMonsterParty(int amount)
        {
            amount = Math.Min(amount, MONSTER_PARTY_LIMIT);

            int randomMonsterIndex = Util.Random.getInt(Enum.GetNames(typeof(MonsterTypes)).Length);
            List<Monster> monsterParty = new List<Monster>();
            for (int i = 0; i < amount; i++)
            {
                Monster monster = getMonster();
                if(monster != null) monsterParty.Add(monster);
            }
            return monsterParty;
        }

        private Monster getMonster()
        {
            Monster randomMonster = null;
            int randomMonsterIndex = Util.Random.getInt(Enum.GetNames(typeof(MonsterTypes)).Length);
            switch ((MonsterTypes)randomMonsterIndex)
            {
                case MonsterTypes.FUZZBALL:
                    randomMonster = new FuzzBall();
                    break;
            }


            return randomMonster;
        }




    }
}
