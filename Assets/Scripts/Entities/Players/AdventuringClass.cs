
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Player.Abilities;
using Assets.Scripts.RPA_Entity_Components;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Player_Classes
{
    public enum PlayerClasses
    {
        WARRIOR = 0,
        WIZARD = 1,
        ROGUE = 2,
        CLERIC = 3
    }

    public abstract class AdventuringClass : Combatable
    {
        public static readonly int ABILITY_LIMIT = 5;
        public PlayerClasses classId { get; protected set; }

        public AdventuringClass(string name)
        {
            this.abilities = new List<Ability>();
            base.name = name;
        }
        protected void setId(PlayerClasses classId)
        {
            this.classId = classId;
            this.healthProperties = new Damageable(classId);
        }


        /******************STATIC INITIALIZERS***************/
        //Sets ability path for each class, called on initial meta data load in character creation
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

        //Returns the sprite model for a class 
        public static Sprite getClassSprite(int classId)
        {
            Sprite classSprite = null;
            switch ((PlayerClasses)classId)
            {
                case PlayerClasses.WARRIOR:
                    classSprite = Warrior.assetData.model;
                    break;

                case PlayerClasses.WIZARD:
                    classSprite = Wizard.assetData.model;
                    break;

                case PlayerClasses.ROGUE:
                    classSprite = Rogue.assetData.model;
                    break;

                case PlayerClasses.CLERIC:
                    classSprite = Cleric.assetData.model;
                    break;
            }
            return classSprite;
        }

        //Creates a unique list of classIds and loads unique assets once
        public static void setClassSprites(in List<RPA_Player.Player> players)
        {
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
                        Warrior.assetData = new Renderable();
                        Warrior.assetData.model = sprite;
                        abilityPath = Warrior.abilityPath;
                        break;

                    case PlayerClasses.WIZARD:
                        Wizard.assetData = new Renderable();
                        Wizard.assetData.model = sprite;
                        abilityPath = Wizard.abilityPath;
                        break;

                    case PlayerClasses.ROGUE:
                        Rogue.assetData = new Renderable();
                        Rogue.assetData.model = sprite;
                        abilityPath = Rogue.abilityPath;
                        break;

                    case PlayerClasses.CLERIC:
                        Cleric.assetData = new Renderable();
                        Cleric.assetData.model = sprite;
                        abilityPath = Cleric.abilityPath;
                        break;
                    default:
                        throw new NotImplementedException("Class Doesn't Exist");
                }
            }
        }

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

        //@param: isInitialLoad  - flags whetther initial ability pool should be loaded or 
        public void loadAbilities(string path, bool isInitialLoad)
        {
            if (path == "") return;
            if (this.classId == PlayerClasses.ROGUE) throw new NotImplementedException("Abilities Don't Exist For " + this.classId);

            int skillLevel = 0;
            string abilityJson = File.ReadAllText(path);
            JSONNode json = JSON.Parse(abilityJson);
            JSONArray abilities = isInitialLoad ? json["starting_abilities"].AsArray : json["abilities"].AsArray;
            Sprite[] iconTextures = Resources.LoadAll<Sprite>(json["starter_ability_icons"]);

            //Construct abilities
            for (int i = 0; i < abilities.Count; i++)
            {
                Sprite abilityTexture = iconTextures.Where(texture => texture.name == abilities[i]["icon_path"].Value).First<Sprite>();
                this.abilities.Add(AbilityFactory.constructAbility(abilities[i], abilityTexture, skillLevel));
            }
        }
    }
}
