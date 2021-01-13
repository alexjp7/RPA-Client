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
  this class inherits from the Combatant type, which supports
  polymorphic collections between players/monsters.
**************************************************************/
namespace Assets.Scripts.Entities.Players
{
    using UnityEngine;
    using SimpleJSON;

    using Assets.Scripts.Entities.Components;
    using Assets.Scripts.Entities.Containers;

    public enum PlayerClasses : int
    {
        WARRIOR = 0,
        WIZARD = 1,
        ROGUE = 2,
        CLERIC = 3
    }

    public class Adventurer : Combatant
    {
        public static readonly int ABILITY_LIMIT = 5;
        public static readonly string PLAYER_SPRITE_PATH = "player_textures/";

        public PlayerClasses classId { get; protected set; }

        /***************************************************************
        @param - name: sets the Combatant base object's name field.
        **************************************************************/
        public Adventurer(string _name, int id)
        {
            this.id = id;
            assetPath += PLAYER_SPRITE_PATH;
            name = _name;
            type = CombatantType.PLAYER;
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
        protected void setCommonData(PlayerClasses classId)
        {
            this.classId = classId;
            abilities = new Abilities(getAbilityPath(classId), type, true);
            healthProperties = new Damageable(classId);
            assetData.name = classId.ToString();
            setSpritePath(classId.ToString());
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
        public override string ToString()
        {
            JSONObject parentJSON = toJson();
            parentJSON.Add("class", classId.ToString());
            return parentJSON.ToString();
        }
    }
}
