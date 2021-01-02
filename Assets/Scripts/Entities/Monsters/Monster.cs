/*---------------------------------------------------------------
                       MONSTER (Abstract Model)
 ---------------------------------------------------------------*/
/***************************************************************
* The Monster class provides a base class that all monster
  types inherit from.

* All Monster types inherit the Combatant base, allowing
  for polymorphic collections to exist between monsters and
  player adventuring classes.

* In the current state, the logic handling monster AI will be
  subject to alot of change until a suitable playable state is 
  reached
**************************************************************/

using System.Collections.Generic;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Combat;
using Assets.Scripts.GameStates;
using Assets.Scripts.Util;
using System;
using Assets.Scripts.Entities.Players;
using UnityEngine;
using SimpleJSON;

#pragma warning disable 1234
namespace Assets.Scripts.Entities.Monsters
{
    /***************************************************************
    * The intention of the MonsterTypes enum is to provide generic
      and readable identification of monsters types and their sub-types. 
    
    * These types when implemented will ideally each house 
      similar logic/handling of targeting, attack patterns and 
      overall monster behaviours.

        e.g. A Superior FuzzBall and Elite FuzzBall would be both
             be identified as a FuzzBall type, and will both have
             a FuzzBall superclass. When the MonsterFactory
             initializes the monster party, it will be able to popualte
             various party types composed of various subtypes of 
             FuzzBalls.
            
    * This notion will be consistent across all monster types
      and will hopefully/eventually support paramterised creation
      to enable a more customizable party creation base on the state
      of a running game.
    **************************************************************/

    public enum Personality
    {
        AGGRESSIVE = 0,
        CONSERVATIVE = 1,
        SUPPORTIVE = 2
    }

    public abstract class Monster : Combatant
    {
        //Used to send to other clients to communicate targeting information
        public static int monsterCount = 0;

        /***************************************************************
        * During construction, all monsters names and abilities
          are initialised, first through setting the name to the randonly
          selected prefix (for novelty sake), and then the subclass
          concatenates the remaining monster name. 
           
            e.g.  Monster.name = "Homeless"; //Prefix
                  Monster.FuzzBall.name += " FuzzBall"; //Sub-class name
                  name = "Homeless FuzzBall"; 
        **************************************************************/
        public Monster()
        {
            id = monsterCount++;
            assetPath += "monster_textures/";
            name = getNamePrefix();
            healthProperties = new Damageable();
            type = CombatantType.MONSTER;
        }

        /***************************************************************
        * the setID() member function allows for a defered setting 
          of the Monster's ID until the sub-class constructor
          is called.
        
        * Based on the monsterId pased in, the health properties are
          set by the logic within the consturctor of this monster's
          damageable instance.

        @param - monsterId: The sub-classes monster type. 
        **************************************************************/
        protected void setSpriteData(string typenName)
        {
            setSpritePath(typenName);
            assetData.name = typenName;
        }

        /***************************************************************
        * Provides a comical/novel naming prefix for all monsters to
          include before their name.
        
        * The name prefixx possibilities are privded in a static array
          which is then randomly indexed into to give a random assortment
          of monster prefixes.

        @return - a name prefix for monsters.
        **************************************************************/
        private string getNamePrefix()
        {
            string[] namePrefixes = 
            {
                "Homeless",
                "Self-Employed",
                "Hungry",
                "Uneducated",
                "Clueless",
                "Silly",
                "Self-Deprecating",
                "Disorganised", 
                "Self-Indulged",
                "Clumsy",
                "Contaminated",
                "Forward-Thinking"
            };

            return namePrefixes[Util.Random.getInt(namePrefixes.Length)];
        }



        public virtual List<Combatant> getTargets(out Ability abilityUsed, in List<Combatant> monsterParty,  List<Combatant> playerParty)
        {
            List<Combatant> targets = new List<Combatant>();
            abilityUsed = selectAbility(monsterParty, playerParty, ref targets);

            return targets;
        }

        private Ability selectAbility(List<Combatant> monsterParty, List<Combatant> playerParty, ref List<Combatant> targets)
        {
            /*AVAILABLE METHODS FOR COMBATABLES*/
            //Current Health
            float monsterCurrentHealth = monsterParty[0].healthProperties.currentHealth;
            //Max Health
            float playerMaxHealth = playerParty[0].healthProperties.maxHealth;
            //Health percent
            float healthPercent = playerParty[0].getHealthPercent();

            //To See what conditions a combatant has - Map status effect id -> condition object
            Dictionary<int, Condition> playerConditions = playerParty[0].conditions; 
            Dictionary<int, Condition> monsterConditions = monsterParty[0].conditions;

            //INSERT ABILITY SELECTION LOGIC HERE
        
            Ability abilityUsed = abilities[1].isOnCooldown ? abilities[0]: abilities[1];

            //Ability Processing
            //MetaType = Damage, Healing, Effect
            MetaType metaType = AbilityUtils.getMetaType(abilityUsed.typeIds[0]);
            AbilityTypes abilityType = (AbilityTypes)abilityUsed.typeIds[0];

            switch (abilityType)
            {
                //Target will always be self
                case AbilityTypes.SELF_HEAL: case AbilityTypes.SELF_BUFF:
                    targets.Add(this);
                    break;

                //INSERT TARGETING LOGIC HERE
                case AbilityTypes.SINGLE_DAMAGE: case AbilityTypes.SINGLE_DEBUFF:

                    //Test - First alive enemy player
                    foreach (var player in playerParty)
                    {
                        if (player.isAlive())
                        {
                            targets.Add(player);
                            break;
                        }
                    }

                    break;

                //INSERT TARGETING LOGIC HERE
                case AbilityTypes.SINGLE_HEAL: case AbilityTypes.SINGLE_BUFF:
                    
                    //Test - First alive allied monster
                    foreach (var monster in monsterParty)
                    {
                        if (monster.isAlive())
                        {
                            targets.Add(monster);
                            break;
                        }
                    }

                    break;

                //Target Will alaways be allies
                case AbilityTypes.MULTI_HEAL: case AbilityTypes.MULTI_BUFF:

                    foreach (var monster in monsterParty)
                        if (monster.isAlive()) targets.Add(monster);

                    break;

                //Target Will alaways be enemies
                case AbilityTypes.MULTI_DAMAGE: case AbilityTypes.MULTI_DEBUFF:

                    foreach (var player in playerParty)
                        if (player.isAlive()) targets.Add(player);

                    break;

                default:
                    Debug.LogError($"Unimplemented or malformed ability type encountered during {this.name} ability selection {new System.Diagnostics.StackFrame().ToString()}");
                    break;
            }


            return abilityUsed;
        }
    }
}
