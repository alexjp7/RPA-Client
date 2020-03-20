using System;
using System.Collections.Generic;
using Assets.Scripts.Entities.Monsters;
using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.Monsters
{
    public class MonsterFactory
    {
        public static readonly int MONSTER_PARTY_LIMIT = 4;
        public List<Monster> initMonsterParty(int amount)
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
