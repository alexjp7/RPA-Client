/*---------------------------------------------------------------
                ADVENTURING-CLASS
 ---------------------------------------------------------------*/
/***************************************************************
* This class provides an abstract layer ontop of the more concrete
   class types (Warrior,Wizard,Rogue and Cleric), and allows for
   generic handling of common components and logic for the player
   classes to adhere too.

* The Adventurer defines and implements static asset
  loading functions used for loading sprite and ability data.

* To allow for generic handling of player and monster types,
  this class inherits from the Combatable type, which supports
  polymorphic collections between players/monsters.
**************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Abilities;

using SimpleJSON;

namespace Assets.Scripts.Entities.Players
{
    public enum PlayerClasses: int
    {
        WARRIOR = 0,
        WIZARD = 1,
        ROGUE = 2,
        CLERIC = 3
    }

    public class Adventurer : Combatable
    {
        public static readonly int ABILITY_LIMIT = 5;
        public static readonly string PLAYER_SPRITE_PATH = "player_textures/";

        public PlayerClasses classId { get; protected set; }

        /***************************************************************
        @param - name: sets the Combatable base object's name field.
        **************************************************************/
        public Adventurer(string _name, int id)
        {
            this.id = id;
            assetPath += PLAYER_SPRITE_PATH;
            name = _name;
            type = CombatantType.PLAYER;
            abilities = new List<Ability>();
        }

        public Adventurer(string name)
        {
            this.name = name;
        }

        /***************************************************************
        * the setID() member function allows for a defered setting 
          of the ID until the sub-class constructor
          is called.
        
        * Based on the classId pased in, the health properties are
          set by the logic within the consturctor of this player's
          damageable instance.

        @param - classId: The sub-classes adventuring class type
        (Warrior, Wizard, Rogue or Cleric). 
        **************************************************************/
        protected void setId(PlayerClasses classId)
        {
            this.classId = classId;
            healthProperties = new Damageable(classId);
            assetData.name = classId.ToString();
            setSpritePath(classId.ToString());
        }

        /***************************************************************
        * Loads and constructs the abilities from disk for the 
          client side player.

        @param: isInitialLoad  - flags whetther initial ability pool 
        should be  loaded or an ability list reflective of the current 
        progress in a game session.
        **************************************************************/
        public void loadAbilities(string path, bool isInitialLoad)
        {
            if (path == "") return;
            int skillLevel = 0;
            string abilityText = File.ReadAllText(path);
            JSONNode json = JSON.Parse(abilityText);
            JSONArray abilityJson = isInitialLoad ? json["starting_abilities"].AsArray : json["abilities"].AsArray;
            string entityName = json["entity"];
            //Construct abilities
            for (int i = 0; i < abilityJson.Count; i++)
            {
                abilities.Add(AbilityFactory.constructPlayerAbility(abilityJson[i], entityName, skillLevel));
            }
        }

        /*---------------------------------------------------------------
                            STATIC INITIALIZERS
         ---------------------------------------------------------------*/
        /***************************************************************
        * Sets ability path for each class.
        
        * Called on initial meta data load in character creation.

        @param - classId: The sub-class's adventuring class type
        (Warrior, Wizard, Rogue or Cleric). 
        @param - path: the directory path read from the meta.json
        file that is the relative location from the  Resources
        folder.
        **************************************************************/
        public static void setDataPaths(int classId, string path)
        {
            switch ((PlayerClasses)classId)
            {
                case PlayerClasses.WARRIOR:
                    Warrior.abilityPath = path;
                    break;

                case PlayerClasses.WIZARD:
                    Wizard.abilityPath = path;
                    break;

                case PlayerClasses.ROGUE:
                    Rogue.abilityPath = path;
                    break;

                case PlayerClasses.CLERIC:
                    Cleric.abilityPath = path;
                    break;
            }
        }

        /***************************************************************
        @param - classId: The sub-class's adventuring class type
        (Warrior, Wizard, Rogue or Cleric). 

        @return - The Unity sprite object for the Warrior, Wizard,
        Rogue, Cleric.
        **************************************************************/
        public static Sprite getClassSprite(int classId)
        {
            Sprite classSprite = null;
            switch ((PlayerClasses)classId)
            {
                case PlayerClasses.WARRIOR:
                    classSprite = Warrior.staticAssets.sprite;
                    break;

                case PlayerClasses.WIZARD:
                    classSprite = Wizard.staticAssets.sprite;
                    break;

                case PlayerClasses.ROGUE:
                    classSprite = Rogue.staticAssets.sprite;
                    break;

                case PlayerClasses.CLERIC:
                    classSprite = Cleric.staticAssets.sprite;
                    break;
            }
            return classSprite;
        }


        /***************************************************************
        * Creates a unique list of classIds and loads unique assets once.
            e.g. The player party is 2  Wizards and 2 Rogues,
                 the Wizard and Rogue models are only loaded once.

        @param - players: the list of players in the game, used
        to iterate over and generate a unique class Id set.
        **************************************************************/
        public static void setClassSprites(in List<Player> players)
        {
            /*
            Sprite[] playerTextures = Resources.LoadAll<Sprite>("Textures/class_icons/playerClasses");
            HashSet<int> classSet = new HashSet<int>();
            players.ForEach(player => classSet.Add(player.adventuringClass));

            string abilityPath = "";
            foreach (int playerClass in classSet)
            {
                Sprite sprite = playerTextures.Where(texture => texture.name == playerClass.ToString()).First<Sprite>();
                switch ((PlayerClasses)playerClass)
                {
                    case PlayerClasses.WARRIOR:
                        Warrior.staticAssets = new Renderable();
                        Warrior.staticAssets.model = sprite;
                        abilityPath = Warrior.abilityPath;
                        break;

                    case PlayerClasses.WIZARD:
                        Wizard.staticAssets = new Renderable();
                        Wizard.staticAssets.model = sprite;
                        abilityPath = Wizard.abilityPath;
                        break;

                    case PlayerClasses.ROGUE:
                        Rogue.staticAssets = new Renderable();
                        Rogue.staticAssets.model = sprite;
                        abilityPath = Rogue.abilityPath;
                        break;

                    case PlayerClasses.CLERIC:
                        Cleric.staticAssets = new Renderable();
                        Cleric.staticAssets.model = sprite;
                        abilityPath = Cleric.abilityPath;
                        break;
                    default:
                        throw new NotImplementedException("Class Doesn't Exist");
                }
            }
            */
        }


        /***************************************************************
        @param - classId: The sub-class's adventuring class type
        (Warrior, Wizard, Rogue or Cleric). 

        @return - the directory path read from the warrior,wizard,rogue
        or cleric JSON file where the to find the icons for a classes
        abilities.
        **************************************************************/
        public static string getAbilityPath(PlayerClasses classId)
        {
            string result = "";

            switch (classId)
            {
                case PlayerClasses.WARRIOR:
                    result = Warrior.abilityPath;
                    break;

                case PlayerClasses.WIZARD:
                    result = Wizard.abilityPath;
                    break;

                case PlayerClasses.ROGUE:
                    result = Rogue.abilityPath;
                    break;

                case PlayerClasses.CLERIC:
                    result = Cleric.abilityPath;
                    break;
            }
            return result;
        }
    }
}
