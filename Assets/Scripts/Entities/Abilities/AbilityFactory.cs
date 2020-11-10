/*---------------------------------------------------------------
                        ABILITY-FACTORY
 ---------------------------------------------------------------*/
/***************************************************************
* This factory class provides helper and construction methods
  that allow for generic ability-related data to be passed in
  and be constructed into runtime-useable player/monster abilites

* The AbilityFactory also provides Meta-data relevant to all 
  ability types.

 * The current implementation for generating abilities involves
   reading and parsing a JSON file on disk which contains
   the name, type, potency, tooltip and status-effect/condition
   of an ability. 

   e.g.
       Abilities: [
       {
          "id": 0,
          "name": "Overhand Strike",
          "icon_path": "warriorStarterSkills_0",
          "tooltip": "Deliver a @ strike to an enemy dealing $ damage ",
          "tooltip_vars":[["forceful","brutal","devestating"]],
          "damage":[[5,8],[12,20],[25,40]],
          "types": [[0]],
          "cooldown":1,
          "status":null
        }]
 * The above shows the fields which are used to create abilities.
 
 * The ability data files can be found in the Assets/Resources/data 
   folder. These are currently placeholder data files as the 
   intention is to develop a persistent data solution without
   having plain-text readable data files. JSON is an easier
   to work with development 'hack'. 

 * In the future, these files could be binary serialized into BSON
   to allow for minimal changes to the implementation, and
   remaing on a JSON format.
**************************************************************/

using System;
using UnityEngine;

using Assets.Scripts.Entities.Components;
using SimpleJSON;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Assets.Scripts.Entities.Abilities
{

    /***************************************************************
    * Abilities can be categorised into 3 archetypes/metatypes
      which generally describe their intended use. 
    
    * Abilties can have multiple sub-types, however their Metatype
      is the first listed type in the types array.
    
    * The logic to determine the Metatype can be seen in the 
      getMetaType() function.
    **************************************************************/
    enum MetaType
    {
        DAMAGE = 0,
        HEALING = 1,
        EFFECT = 2,
        UNIMPLEMENTED = 4
    }

    /***************************************************************
    * AbilityTypes indicate a more specialised description of a 
      what the ability is intended to do.
    
    * The primary type (first type listed) falls into the above
      MetaTypes. 
      
    The categorisation is as follows:
            if type between 0 and  1  = MetaTypes.DAMAGE;
            else if type > 1 AND  <= 4  = MetaTypes.HEALING
            else if type > 4 AND  <= 9  = MetaTypes.EFFECT
    **************************************************************/
    enum AbilityTypes
    {
        //Damage Types
        SINGLE_DAMAGE = 0, MULTI_DAMAGE = 1,
        //Healing Types
        SELF_HEAL = 2, SINGLE_HEAL = 3, MULTI_HEAL = 4,
        //Positve Status Effect Types
        SELF_BUFF = 5, SINGLE_BUFF = 6, MULTI_BUFF = 7,
        //Negative Status Effect Types
        SINGLE_DEBUFF = 8, MULTI_DEBUFF = 9
    }

    class AbilityFactory
    {
        /***************************************************************
        * Targeting Classifications/Groupings for ability Selection
        **************************************************************/
        //Auto-Targeting - Abilities that select the caster OR All allies/enemies
        public readonly static AbilityTypes[] autoTargetingTypes = 
        { 
            AbilityTypes.SELF_HEAL,
            AbilityTypes.SELF_BUFF,
            AbilityTypes.MULTI_DAMAGE,
            AbilityTypes.MULTI_BUFF,
            AbilityTypes.MULTI_HEAL
        };

        //Single-Enemy-Allows for individual enemy selection 
        public readonly static AbilityTypes[] enemyTargetingTypes = 
        {
            AbilityTypes.SINGLE_DAMAGE,
            AbilityTypes.SINGLE_DEBUFF
        };

        //Single-Ally-Allows for individual ally selection 
        public readonly static AbilityTypes[] allyTargetingTypes =
        {
            AbilityTypes.SINGLE_HEAL,
            AbilityTypes.SINGLE_BUFF
        };

        //Self-Targeting - Restricts targeting to self
        public readonly static AbilityTypes[] selfTargetTypes =
        {
            AbilityTypes.SELF_BUFF,
            AbilityTypes.SELF_HEAL
        };

        /***************************************************************
        * Types that imply a status effect
        **************************************************************/
        public readonly static AbilityTypes[] effectTypes =
        {
            AbilityTypes.SELF_BUFF,
            AbilityTypes.SINGLE_BUFF,
            AbilityTypes.SINGLE_DEBUFF,
            AbilityTypes.MULTI_BUFF,
            AbilityTypes.MULTI_DEBUFF,
        };

        /*---------------------------------------------------------------
                            CONSTRUCTION-METHODS
        ---------------------------------------------------------------*/
        //PLAYER ABILTIIES
        /***************************************************************
        * Constructs an ability; setting its name,id,potency,tooltip,
          and sprite fields.

        @param - jSONNode: an element of the JSON abilities array
        @param - iconTexture: The Unity Sprite GameObject with the 
        loaded texture data
        @param - skillLevel: The skill tier; an integer between 0-2
        representing the strength of the ability that is being loaded.
        skillLevel is used to index into potency and tooltip arrays
        to interpolate and set varying skill-tier's fields.

        @return - An ability constructed by the JSON input.
        **************************************************************/
        public static Ability constructPlayerAbility(JSONNode jsonNode, string entityName, int skillLevel)
        {
            Ability newAbility = new Ability();

            string potencyProperty = "";
            //Common Ability Properties
            newAbility.name = jsonNode["name"];
            newAbility.id = jsonNode["id"].AsInt;
            newAbility.typeIds = getAbilityTypes(jsonNode["types"].AsArray, skillLevel);
            newAbility.cooldown = jsonNode["cooldown"].AsInt;
            newAbility.statusEffect = jsonNode["status"];

            //Player Specific Ability Properties
            newAbility.assetData = new Renderable(newAbility.name, $"{ Ability.BASE_ABILITY_ICON_PATH }{ entityName }/{ newAbility.name }");

            MetaType metaType = getMetaType(newAbility.typeIds[0], out potencyProperty);
            if (potencyProperty == "" || metaType == MetaType.UNIMPLEMENTED)
                throw new ArgumentException($"Malformed JSON in {jsonNode["entity"]} : {jsonNode["name"]} ");

            newAbility.tooltip = construcTooltip(jsonNode, skillLevel, metaType, newAbility.typeIds, potencyProperty);
            newAbility.abilityStrength = constructAbilityStrength(jsonNode, potencyProperty, skillLevel, metaType);


            foreach(int type in newAbility.typeIds)
            {
                if(effectTypes.Contains((AbilityTypes)type) )
                {
                    newAbility.conditionStrength = constructionConditionStrength(jsonNode, skillLevel);
                    newAbility.tooltip = newAbility.tooltip.Replace("+", newAbility.conditionStrength.turnsApplied.ToString());
                    newAbility.tooltip = newAbility.tooltip.Replace("*", EffectProcessor.getEffectLabel(newAbility.statusEffect, newAbility.conditionStrength.potency));
                    break;
                }
            }


            newAbility.setTargetingType();

            return newAbility;
        }

        /*---------------------------------------------------------------
                            MONSTER-ABILITES
        ---------------------------------------------------------------*/
        public static List<Ability> constructMonsterAbilities(string monsterName)
        {
            List<Ability> abilities = new List<Ability>();
            string abilityText = File.ReadAllText(Ability.MONSTER_ABILITY_DATA_PATH + monsterName + ".json");

            JSONNode json = JSON.Parse(abilityText);
            JSONArray abilityJson = json["abilities"].AsArray;
            int abilityCount = abilityJson.Count;

            //Construct each ability
            foreach (JSONNode jsonNode in abilityJson)
            {
                Ability newAbility = new Ability();
                string potencyProperty = "";

                newAbility.name = jsonNode["name"];
                newAbility.typeIds = getAbilityTypes(jsonNode["types"].AsArray, 0);
                newAbility.cooldown = jsonNode["cooldown"].AsInt;
                newAbility.statusEffect = jsonNode["status"];

                MetaType metaType = getMetaType(newAbility.typeIds[0], out potencyProperty);
                newAbility.abilityStrength = constructAbilityStrength(jsonNode, potencyProperty, 0, metaType);
                foreach (int type in newAbility.typeIds)
                {
                    if (effectTypes.Contains((AbilityTypes)type))
                    {
                        newAbility.conditionStrength = constructionConditionStrength(jsonNode, 0);
                        break;
                    }
                }
                newAbility.setTargetingType();
                abilities.Add(newAbility);
            }


            return abilities;
        }



        /*---------------------------------------------------------------
                            CONSTRUCTION-METHODS
        ---------------------------------------------------------------*/
        private static AbilityStrength constructAbilityStrength(JSONNode abilityJson,string potencyProperty, int skillLevel, MetaType metaType)
        {
            AbilityStrength abilityStrength = new AbilityStrength();

            if (metaType == MetaType.DAMAGE || metaType == MetaType.HEALING)
            {
                abilityStrength.min = abilityJson[potencyProperty][skillLevel][0].AsInt;
                abilityStrength.max = abilityJson[potencyProperty][skillLevel][1].AsInt;
            }

            return abilityStrength;
        }

        private static ConditionStrength constructionConditionStrength(JSONNode abilityJson, int skillLevel)
        {
            ConditionStrength conditionStrength = new ConditionStrength();
            conditionStrength.turnsApplied = abilityJson["turns_applied"][skillLevel];
            conditionStrength.potency = abilityJson["potency"][skillLevel].AsInt;
            return conditionStrength;
        }


        /***************************************************************
        * Helper method called when constructing an ability. Converts
         the JSON string values of an integer type id into an integer 
         array.

        @param - abilityTypeJson: The JSON array of ability types.
        @param - skillLevel: an integer between 0-2, used to index into
        the typeIds JSON array and retrieve the ability types
        that are relevant to the skill level parameter.

        @return - a list of an ability's type.
        **************************************************************/
        private static int[] getAbilityTypes(in JSONArray abilityTypeJson, int skillLevel)
        {
            int tiersAvailable = abilityTypeJson.Count;
            skillLevel = tiersAvailable == 1 ? 0 : skillLevel;
            int[] results = new int[abilityTypeJson[skillLevel].Count];

            for (int i = 0; i < abilityTypeJson[skillLevel].Count; i++)
            {
                results[i] = abilityTypeJson[skillLevel][i].AsInt;
            }

            return results;
        }

        /***************************************************************
        * Helper method for constructing the tooltip string for an 
          ability. 

        * The tooltip of an ability has a description block and various
          'interpolation tokens'. This format supports for variable 
          tooltip descriptions that reflect the skill progression system
          of uprading an ability reflecting a more verbose ability
          description.

            e.g. base_tooltip = "Deliver a @ strike to an enemy dealing $ damage ",
                 tooltip_variables =   ["forceful","brutal","devestating"] 
                 damage : [ [5,8], [12,20], [25,40]], //Min-max damage for each tier
                 skillLevel = 0

                 The @ character will be replaced with tooltip_variables[0] = "forceful".
                 The # Character is used to replace the 2nd tooltip token (not used in this example) 
                 The $ character will be replaced with damage[0][0] = 5 as the minimun,
                 and damage[0][1] = 8 as the maxiumun damage.

                Creating the final tooltip string as:
                    "Deliver a forceful strike to an enemy dealing 5-8 damage ".
        
        @param - abilityJson: an element of the JSON abilities array.
        @param - skillLevel: an integer between 0-2, used to index into
        the typeIds JSON array and ability potency (damage/healing amount)
        that are relevant to the skill level parameter.
        @param - primaryType: The first type listed in the types array,
        indicating the primary use for that ability (Healing,Damage,Effect).
        @ref-param - abilityStrength: a struct that contains the abilities
        min-max effect (healing/damage ranges). abilityStrength
        is passed in as a reference to allow it to be set by the tooltip
        consruciton logic.

         @return - The completed tooltip description with interpolated
         damage and description values.
        **************************************************************/
        private static string construcTooltip(in JSONNode abilityJson, int skillLevel,MetaType metaType , int[] typeIds, string potencyProperty)
        {
            //Construct Tooltip 
            string tooltip = abilityJson["tooltip"].Value;
            string abilityPotency = "";
            if (metaType == MetaType.DAMAGE || metaType == MetaType.HEALING)
            {
                abilityPotency = abilityJson[potencyProperty][skillLevel][0].AsInt + "-" + abilityJson[potencyProperty][skillLevel][1].AsInt;
            }
            else if (metaType == MetaType.EFFECT)
            {
                abilityPotency = abilityJson[potencyProperty][skillLevel].AsInt.ToString();
            }

            //Interpolate damage
            tooltip = tooltip.Replace("$", abilityPotency);
            JSONArray toolTipVars = abilityJson["tooltip_vars"].AsArray;
            //Interpolate Description var-args
            for (int i = 0; i < toolTipVars.Count; i++)
            {
                tooltip = tooltip.Replace(Ability.toolTipVarTokens[i], toolTipVars[i][skillLevel].Value);
            }

            return tooltip;
        }

        /***************************************************************
        * Helper method called when constructing an ability. used in 
          setting potency property used in JSON parsing while returning
          the meta type. Prevents repeated logic of checking an 
          abilities primary type.

        @param - primaryType: The first type listed in the types array,
        indicating the primary use for that ability (Healing,Damage,Effect).
        @out-param - potencyProperty: sets the key to use in further 
        processing of the JSON object of an ability.

        @return - The metatype of an ability  
        **************************************************************/
        public static MetaType getMetaType(int primaryType, out string potencyProperty)
        {
            MetaType metaType = MetaType.UNIMPLEMENTED;
            potencyProperty = "";

            if (primaryType == 0 || primaryType == 1)
            {
                potencyProperty = "damage";
                metaType = MetaType.DAMAGE;
            }
            else if (primaryType > 1 && primaryType <= 4)
            {
                potencyProperty = "health";
                metaType = MetaType.HEALING;
            }
            else if (primaryType > 4 && primaryType <= 9)
            {
                potencyProperty = "potency";
                metaType = MetaType.EFFECT;
            }

            return metaType;
        }

        /***************************************************************
        * Overload for getMetaType(int) method called when constructing an ability. 
          used in BattleState logic for determining what type of ability
          is being used on a target.

        @param - primaryType: The first type listed in the types array,
        indicating the primary use for that ability (Healing,Damage,Effect).

        @return - The Metatype, i.e. whether the ability is
        classified as an Attack,Heal or Effect ability.  
        **************************************************************/
        public static MetaType getMetaType(int primaryType)
        {
            MetaType metaType = MetaType.UNIMPLEMENTED;

            if (primaryType == 0 || primaryType == 1)
                metaType = MetaType.DAMAGE;
            else if (primaryType > 1 && primaryType <= 4)
                metaType = MetaType.HEALING;
            else if (primaryType > 4 && primaryType <= 9)
                metaType = MetaType.EFFECT;

            return metaType;
        }





        private static void processAbilityStrength(in JSONNode abilityJson, int skillLevel, int primaryType, ref AbilityStrength abilityStrength, ref int statusEffect)
        {
            //Determine ability type and meta type, determine what property is used for each skill type 
            string potencyProperty = "";
            MetaType metaType = getMetaType(primaryType, out potencyProperty);

            if (potencyProperty == "" || metaType == MetaType.UNIMPLEMENTED)
            {
                throw new ArgumentException("Malformed JSON included in ability id: " + abilityJson["id"] + " - " + abilityJson["name"]);
            }

            //Construct Tooltip and set damage values for ability
            if (metaType == MetaType.DAMAGE || metaType == MetaType.HEALING)
            {
                int abilityMin = abilityJson[potencyProperty][skillLevel][0].AsInt;
                int abilityMax = abilityJson[potencyProperty][skillLevel][1].AsInt;

                abilityStrength.min = abilityMin;
                abilityStrength.max = abilityMax;

            }
            else if (metaType == MetaType.EFFECT)
            {
                int potency = abilityJson[potencyProperty][skillLevel].AsInt;
                abilityStrength.min = abilityJson["turns_applied"][skillLevel];
                abilityStrength.max = potency;
                statusEffect = abilityJson["status"];
            }
        }


        /***************************************************************
        * Helper method to return the string of the ability type 
          for a tooltip description. Abilities can have multiple types
          and for each of those, the string label is returned to display
          to the user. Called in AbilityToolTip.showTooltip()

        @param - typeIds: an array of ability types 
        @return - a list of an ability's type labels 
        **************************************************************/
        public static string[] getAbilityTypeLabel(int[] typeIds)
        {
            string[] result = new string[2];

            for (int i = 0; i < typeIds.Length; i++)
            {
                switch ((AbilityTypes)typeIds[i])
                {
                    case AbilityTypes.SINGLE_DAMAGE:
                        result[i] = "Single-Damage";
                        break;
                    case AbilityTypes.MULTI_DAMAGE:
                        result[i] = "Multi-Damage";
                        break;
                    case AbilityTypes.SELF_HEAL:
                        result[i] = "Self-Heal";
                        break;
                    case AbilityTypes.SINGLE_HEAL:
                        result[i] = "Single-Heal";
                        break;
                    case AbilityTypes.MULTI_HEAL:
                        result[i] = "Multi-Heal";
                        break;
                    case AbilityTypes.SELF_BUFF:
                        result[i] = "Self-Buff";
                        break;
                    case AbilityTypes.SINGLE_BUFF:
                        result[i] = "Single-Buff";
                        break;
                    case AbilityTypes.MULTI_BUFF:
                        result[i] = "Mutli-Buff";
                        break;
                    case AbilityTypes.SINGLE_DEBUFF:
                        result[i] = "Single-Debuff";
                        break;
                    case AbilityTypes.MULTI_DEBUFF:
                        result[i] = "Multi-Debuff";
                        break;
                }
            }

            return result;
        } 
    }
} 
