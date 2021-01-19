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
namespace Assets.Scripts.Entities.Monsters
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;

    using Assets.Scripts.RPA_Game;
    public class MonsterFactory
    {
        /// <summary>
        /// String value of the monstertypes namespace, which is used to load monster classes at runtime.
        /// </summary>
        private static readonly string monsterTypesNameSpace = "Assets.Scripts.Entities.Monsters.MonsterTypes";

        private static List<Type> _monsterTypes;
        private static List<Type> monsterTypes
        {
            get
            { 
                if(_monsterTypes == null) 
                {
                    _monsterTypes = getMonsterTypes();
                }

                return _monsterTypes;
            }

        }

        /// <summary>
        /// Generates a list of monters which are loaded from the <see cref="MonsterTypes"/> namespace.
        /// </summary>
        /// <returns>A list of monsters to be entered into combat</returns>
        private static List<Type> getMonsterTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> classList = new List<Type>();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.Namespace == monsterTypesNameSpace)
                {
                    classList.Add(type);
                }
            }

            if(classList.Count == 0)
            {
                throw new NotImplementedException($"ERROR: Failed to load Monster Assemblies. Could not find any MonsterTypes in the '{monsterTypesNameSpace}' namespace");
            }

            return classList;
        }

        private System.Random rand;

        public MonsterFactory()
        {
            rand = new System.Random();
        }

        /// <summary>
        ///  Instantiates a list of monsters.
        /// </summary>
        /// <param name="amount">the amount of monsters desired to be made.</param>
        /// <returns>The list of monster objects used in the battle state.</returns>
        public List<Monster> createMonsterParty(int amount)
        {
            List<Monster> monsterParty = new List<Monster>();
            for (int i = 0; i < amount; i++)
            {
                Monster monster = getMonster();
                if(monster != null) monsterParty.Add(monster);
            }

            return monsterParty;
        }

        /// <summary>
        /// Generates monsters that were loaded by games party leader 
        /// </summary>
        /// <param name="monsters">mapped type name and monster name from</param>
        /// <returns>The list of monster objects used in the battle state.</returns>
        public List<Monster> createMonsterParty(List<KeyValuePair<string, string>> monsters)
        {
            List<Monster> monsterParty = new List<Monster>();
            foreach (var monster in monsters)
            {
                Monster m = getMonster(monster.Key, monster.Value);
                if (m != null) monsterParty.Add(m);
            }

            Monster.monsterCount = 0;
            return monsterParty;
        }

        /// <returns> A single random monster, if its sprite data was no loadable 
        /// then return it as null which ensures it is not added to the monsterparty.
        /// </returns>
        private Monster getMonster()
        {   
            //Get random mosnter type
            Monster newMonster = getInstance(monsterTypes[rand.Next(monsterTypes.Count)]); ;

            if (newMonster.assetData.sprite == null)
            {
                newMonster = null;
            }
            else
            {
                //Scale monsters based on party size
                newMonster.healthProperties.maxHealth = newMonster.getMaxHp() + (newMonster.getMaxHp() * (Game.players.Count()-1));
                newMonster.healthProperties.currentHealth = newMonster.healthProperties.maxHealth;
            }

            return newMonster;
        }

        /// <summary>
        /// a single random monster, if its sprite data was  no loadable then return it as null which ensures it is not added to the monsterparty.
        ///  Used when monter party has been generated by a game's party leader and this clientneeds to recreate it.
        /// </summary>
        /// <param name="type">A monster's class type, which is loaded from <see cref="MonsterTypes "/>namespace</param>
        /// <param name="name">Display Name of the monster</param>
        /// <returns></returns>
        private Monster getMonster(string type, string name)
        {
            //Get random mosnter type
            Type monsterType = monsterTypes.Find(mType => mType.Name == type);
            if (monsterType == null) return null;

            Monster newMonster = getInstance(monsterType);

            if (newMonster.assetData.sprite == null)
            {
                newMonster = null;
            }
            else
            {
                newMonster.name = name;
                //Scale monsters based on party size
                newMonster.healthProperties.maxHealth = newMonster.getMaxHp() + (newMonster.getMaxHp() * (Game.players.Count() - 1));
                newMonster.healthProperties.currentHealth = newMonster.healthProperties.maxHealth;
            }

            return newMonster;
        }

        /// <param name="monsterType"> monsterType: the type name as specified in the MonsterTypes namespace.</param>
        /// <returns>a monster instance through a passed in class name</returns>
        public Monster getInstance(Type monsterType)
        {
            return Activator.CreateInstance(monsterType) as Monster;
        }
    }
}
