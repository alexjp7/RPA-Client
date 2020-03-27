﻿/*---------------------------------------------------------------
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
    enum MetaTypes
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

            for(int i = 0; i < typeIds.Length; i++)
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
        public static Ability constructAbility(JSONNode jSONNode, in Sprite iconTexture, int skillLevel)
        {
            AbilityStrength abilityStrength = new AbilityStrength();
            int[] types = getAbilityTypes(jSONNode["types"].AsArray, skillLevel);
            string tooltip = construcTooltip(jSONNode, skillLevel, types[0], ref abilityStrength);
            Renderable assetData = new Renderable();
            assetData.model = iconTexture;

            return new Ability(jSONNode["id"].AsInt, 
                                types,jSONNode["name"].Value, 
                                tooltip, 
                                jSONNode["cooldown"], 
                                abilityStrength,
                                assetData);
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
        public static MetaTypes getMetaType(int primaryType, out string potencyProperty)
        {
            MetaTypes metaType = MetaTypes.UNIMPLEMENTED;
            potencyProperty = "";

            if (primaryType == 0 || primaryType == 1)
            {
                potencyProperty = "damage";
                metaType = MetaTypes.DAMAGE;
            }
            else if (primaryType > 1 && primaryType <= 4)
            {
                potencyProperty = "health";
                metaType = MetaTypes.HEALING;
            }
            else if (primaryType > 4 && primaryType <= 9)
            {
                potencyProperty = "potency";
                metaType = MetaTypes.EFFECT;
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
        public static MetaTypes getMetaType(int primaryType)
        {
            MetaTypes metaType = MetaTypes.UNIMPLEMENTED;

            if (primaryType == 0 || primaryType == 1)
                metaType = MetaTypes.DAMAGE;
            else if (primaryType > 1 && primaryType <= 4)
                metaType = MetaTypes.HEALING;
            else if (primaryType > 4 && primaryType <= 9)
                metaType = MetaTypes.EFFECT;

            return metaType;
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
        private static string construcTooltip(in JSONNode abilityJson, int skillLevel, int primaryType, ref AbilityStrength abilityStrength)
        {
            //Determine ability type and meta type, determine what property is used for each skill type 
            string potencyProperty = "";
            MetaTypes metaType = getMetaType(primaryType, out potencyProperty);

            if (potencyProperty == "" || metaType == MetaTypes.UNIMPLEMENTED)
            {
                throw new ArgumentException("Malformed JSON included in ability id: " + abilityJson["id"] + " - " + abilityJson["name"]);
            }

            //Construct Tooltip and set damage values for ability
            string tooltip = abilityJson["tooltip"].Value;
            string abilityPotency = "";
            if (metaType == MetaTypes.DAMAGE || metaType == MetaTypes.HEALING)
            {
                int abilityMin = abilityJson[potencyProperty][skillLevel][0].AsInt;
                int abilityMax = abilityJson[potencyProperty][skillLevel][1].AsInt;

                abilityPotency = abilityMin + "-" + abilityMax;
                abilityStrength.min = abilityMin;
                abilityStrength.max= abilityMax;

            }
            else if (metaType == MetaTypes.EFFECT)
            {
                int potency = abilityJson[potencyProperty][skillLevel].AsInt;
                abilityPotency = potency.ToString();
                abilityStrength.min = potency;
                abilityStrength.max = -1;
            }

            //Interpolate damage
            tooltip = tooltip.Replace("$", abilityPotency);
            JSONArray toolTipVars = abilityJson["tooltip_vars"].AsArray;
            //Interpolate Description var-args
            for (int i = 0; i < toolTipVars.Count; i++)
                tooltip = tooltip.Replace(Ability.toolTipVarTokens[i], toolTipVars[i][skillLevel].Value);

            return tooltip;
        }
    }
}