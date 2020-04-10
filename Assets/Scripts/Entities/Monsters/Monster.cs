/*---------------------------------------------------------------
                       MONSTER (Abstract Model)
 ---------------------------------------------------------------*/
/***************************************************************
* The Monster class provides a base class that all monster
  types inherit from.

* All Monster types inherit the Combatable base, allowing
  for polymorphic collections to exist between monsters and
  player adventuring classes.

* In the current state, the logic handling monster AI will be
  subject to alot of change until a suitable playable state is 
  reached
**************************************************************/

using System.Collections.Generic;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Abilities;

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
    public enum MonsterTypes
    {
        FuzzBall = 0,
        GoblinFighter = 1,
    }

    public abstract class Monster : Combatable
    {
        public MonsterTypes monsterId { get; protected set; }

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
            assetPath += "monster_textures/";
            name = getNamePrefix();
            abilities = new List<Ability>();
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
        protected void setId(MonsterTypes _monsterId)
        {
            monsterId = _monsterId;
            healthProperties = new Damageable();
            setSpritePath(monsterId.ToString());
            assetData.spriteName = monsterId.ToString();
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
            string[] namePrefixes = {"Homeless",
                                     "Self-Employed",
                                     "Hungry",
                                     "Uneducated",
                                     "Clueless",
                                     "Silly"};

            return namePrefixes[Util.Random.getInt(namePrefixes.Length)];
        }


    }
}
