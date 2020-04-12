﻿/*---------------------------------------------------------------
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
using System.Reflection;

namespace Assets.Scripts.Entities.Monsters
{
    public class MonsterFactory
    {
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

        /***************************************************************
        @return - A list of MonsterTypes found in the MonsterTypes
        namesapce. This list is used in dynamic creation of monster
        parties.
        **************************************************************/
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

        /***************************************************************
        * Instantiates a list of monsters.
        @param - amount: the amount of monsters desired to be made.
        
        @return - The list of monster objects used in the battle state.
        **************************************************************/
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

        /***************************************************************
        @return - a single random monster, if its sprite data was 
        no loadable then return it as null which ensures it is not added
        to the monsterparty.
        **************************************************************/
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

        /***************************************************************
        @param - monsterType: the type name as specified in the MonsterTypes
        namespace.

        @return - a monster instance through a passed in class name
        **************************************************************/
        public Monster getInstance(Type monsterType)
        {
            return Activator.CreateInstance(monsterType) as Monster;
        }
    }
}
