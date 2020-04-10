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
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace Assets.Scripts.Entities.Monsters
{
    public class MonsterFactory
    {
        private System.Random rand;

        public MonsterFactory()
        {
            rand = new System.Random();
        }

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
            string monsterName = ((MonsterTypes)rand.Next(Enum.GetNames(typeof(MonsterTypes)).Length)).ToString();
            try
            {
                randomMonster = getInstance(monsterName);
                //Scale monsters based on party size
                randomMonster.healthProperties.maxHealth = randomMonster.getMaxHp() + (randomMonster.getMaxHp() * Game.players.Count());
                randomMonster.healthProperties.currentHealth = randomMonster.healthProperties.maxHealth;
            }
            catch(ArgumentNullException ex)
            {
                Debug.LogErrorFormat("ERROR - Type name inconsistent between Class definition and matching entry in the MonsterTypes enum  for '{0}'. Ensure a concrete monster class has been defined and implemented for '{0}'" +
                                     "\nSOLUTION - 1) Check '{1}' for missing or missmatched class definition. 2) Ensure the implemented class is a derived type of Monster and calls the setId() member function with a valid MonsterType argument during construction. {2}", monsterName, typeof(Monster).Namespace, ex.StackTrace);


            }

            return randomMonster;
        }

        /***************************************************************
        @param - className: String value that represents the type name
        as specified in the Monster namespace.

        @return - a monster instance through a passed in class name
        **************************************************************/
        public Monster getInstance(string className)
        {
            Type t = Type.GetType(typeof(Monster).Namespace + "." + className);
            return Activator.CreateInstance(t) as Monster;
        }
    }
}
