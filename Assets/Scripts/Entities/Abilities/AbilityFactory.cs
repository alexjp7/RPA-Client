using System;
using System.Collections.Generic;
using Assets.Scripts.RPA_Entity_Components;
using SimpleJSON;
using UnityEngine;

namespace Assets.Scripts.Player.Abilities
{
    enum MetaTypes
    {
        DAMAGE = 0,
        HEALING = 1,
        EFFECT = 2,
        UNIMPLEMENTED = 4
    }

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

        public static void interpretAbility(int[] typeIds, in List<RPA_Player.Player> players) //List enemies
        {
            for (int i = 0; i < typeIds.Length; i++)
            {
                switch ((AbilityTypes)typeIds[i])
                {
                    case AbilityTypes.SINGLE_DAMAGE:
                        break;
                    case AbilityTypes.MULTI_DAMAGE:
                        break;
                    case AbilityTypes.SELF_HEAL:
                        break;
                    case AbilityTypes.SINGLE_HEAL:
                        break;
                    case AbilityTypes.MULTI_HEAL:
                        break;
                    case AbilityTypes.SELF_BUFF:
                        break;
                    case AbilityTypes.SINGLE_BUFF:
                        break;
                    case AbilityTypes.MULTI_BUFF:
                        break;
                    case AbilityTypes.SINGLE_DEBUFF:
                        break;
                    case AbilityTypes.MULTI_DEBUFF:
                        break;
                }
            }
        }
    
        //Healper method to return the string of the abilit type for the abilit tooltip
        public static string[] getAbilityType(int[] typeIds)
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

        public static int[] getAbilityTypes(in JSONArray abilityTypeJson, int skillLevel)
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

        //Overload used in setting potency property used in JSON parsing
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

        //Overload - doesn't set JSON property
        public static MetaTypes getMetaType(int type)
        {
            MetaTypes metaType = MetaTypes.UNIMPLEMENTED;

            if (type == 0 || type == 1)
            {
                metaType = MetaTypes.DAMAGE;
            }
            else if (type > 1 && type <= 4)
            {
                metaType = MetaTypes.HEALING;
            }
            else if (type > 4 && type <= 9)
            {
                metaType = MetaTypes.EFFECT;
            }

            return metaType;
        }

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
