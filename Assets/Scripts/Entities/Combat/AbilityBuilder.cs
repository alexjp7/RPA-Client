/*---------------------------------------------------------------
                        ABILITY-BUILDER
 ---------------------------------------------------------------*/
/***************************************************************
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

namespace Assets.Scripts.Entities.Combat
{
    using Assets.Scripts.Entities.Components;
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class AbilityBuilder
    {
        private JSONNode jsonNode;
        private string _entityName;
        private string _name;
        private int _id;
        private int[] _typeIds;
        private int _cooldown;
        private string _tooltip;
        private int _statusEffect;
        private Renderable _assetData;
        private AbilityStrength _abilityStrength;
        private ConditionStrength _conditionStrength;
        private string _potencyProperty;
        private MetaType _metaType;
        private int _skillLevel;

        /// <summary>
        /// Passes in JSON node for ability consturction.
        /// </summary>
        /// <param name="jsonNode"></param>
        /// <returns></returns>
        public AbilityBuilder json(JSONNode jsonNode, string entity)
        {
            this.jsonNode = jsonNode;
            _entityName = entity;
            return this;
        }

        /// <summary>
        /// The skill tier/level for a given ability. 
        /// </summary>
        /// <param name="skillLevel">value between 0 and <see cref="Ability.LEVEL_TIER_LIMIT"/></param>
        /// <returns></returns>
        public AbilityBuilder skillLevel(int skillLevel)
        {
            if (skillLevel < 0 || skillLevel >= Ability.LEVEL_TIER_LIMIT)
            {
                throw new ArgumentException($"Skill level {skillLevel} is not permitted. skill levels must be between 0 and {Ability.LEVEL_TIER_LIMIT}");
            }
            _skillLevel = skillLevel;

            return this;
        }

        /// <summary>
        /// Ability display name
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder name()
        {
            _name = jsonNode["name"];

            if (_entityName == null)
            {
                throw new ArgumentException($"Malformed JSON encountered. element [name] could not be resolved.");
            }
            return this;
        }

        /// <summary>
        /// Unique abiliy ID
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder id()
        {
            _id = jsonNode["id"].AsInt;

            if (_entityName == null)
            {
                throw new ArgumentException($"Malformed JSON encountered. element [id] could not be resolved.");
            }
            return this;
        }

        /// <summary>
        /// Array of ability types e.g. Single Attack, MultiAttack. <see cref="AbilityTypes"/>
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder typeIds()
        {
            JSONNode abilityTypeJson = jsonNode["types"];

            if (_entityName == null)
            {
                throw new ArgumentException($"Malformed JSON encountered. element [types] could not be resolved.");
            }

            int tiersAvailable = abilityTypeJson.Count;
            _skillLevel = tiersAvailable == 1 ? 0 : _skillLevel;
            int[] results = new int[abilityTypeJson[_skillLevel].Count];

            for (int i = 0; i < abilityTypeJson[_skillLevel].Count; i++)
            {
                results[i] = abilityTypeJson[_skillLevel][i].AsInt;
            }

            _metaType = AbilityUtils.getMetaType(results[0], out _potencyProperty);

            if (_potencyProperty == "" || _metaType == MetaType.UNIMPLEMENTED)
            {
                throw new ArgumentException($"Malformed JSON in {jsonNode["entity"]} : {jsonNode["name"]} ");
            }

            _typeIds = results;

            return this;
        }

        /// <summary>
        /// The amount of turns an ability is on cooldown for
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder cooldown()
        {
            _cooldown = jsonNode["cooldown"].AsInt;

            if (jsonNode["cooldown"] == null)
            {
                throw new ArgumentException($"Malformed JSON encountered. element [cooldown] could not be resolved.");
            }

            return this;
        }

        /// <summary>
        /// The status effect that an ability applies onto its target when used. <see cref="EffectProcessor"/>
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder statusEffect()
        {
            _statusEffect = jsonNode["status"];
            return this;
        }

        /// <summary>
        /// The data relating to an abilities icon.
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder assetData()
        {
            _assetData = new Renderable(_name, $"{ Ability.BASE_ABILITY_ICON_PATH }{ _entityName }/{ _name}");
            if (_assetData == null)
            {
                throw new ArgumentException($"Malformed JSON encountered. element [assetData] could not be resolved.");
            }
            return this;
        }

        /// <summary>
        /// Constructs the tooltip string for an  ability.
        /// </summary>
        /// <remarks>
        /// This format supports variable tooltip descriptions that reflect the skill progression system of uprading an ability reflecting a more verbose ability description.
        ///<example> 
        ///<para>
        ///For example:
        ///</para>
        /// <list type="bullet">
        /// <item>
        /// <b>base_tooltip</b>: "Deliver a @ strike to an enemy dealing $ damage ",
        /// </item>
        /// <item>
        /// <b>tooltip_variables</b>: ["forceful","brutal","devestating"]
        /// </item>
        /// <item>
        /// <b>damage:</b> [ [5,8], [12,20], [25,40]], //Min-max damage for each tier
        /// </item>
        /// <item>
        /// <b>skillLevel</b>: 0
        /// </item>
        /// </list>
        /// <para>
        /// The @ character will be replaced with tooltip_variables[0] = "forceful". The # Character is used to replace the 2nd tooltip token (not used in this example) The $ character will be replaced with damage[0][0] = 5 as the minimun, and damage[0][1] = 8 as the maxiumun damage.
        /// </para>
        ///Creating the final tooltip string as: <b> "Deliver a forceful strike to an enemy dealing 5-8 damage ".</b>
        /// </example>
        /// </remarks>
        /// <returns></returns>
        public AbilityBuilder toolTip()
        {
            //Construct Tooltip 
            string tooltip = jsonNode["tooltip"].Value;
            if (tooltip == null)
            {
                throw new ArgumentException($"Malformed JSON encountered. element [tooltip] could not be resolved.");
            }

            string abilityPotency = "";

            if (_metaType == MetaType.DAMAGE || _metaType == MetaType.HEALING)
            {
                abilityPotency = jsonNode[_potencyProperty][_skillLevel][0].AsInt + "-" + jsonNode[_potencyProperty][_skillLevel][1].AsInt;
            }
            else if (_metaType == MetaType.EFFECT)
            {
                abilityPotency = jsonNode[_potencyProperty][_skillLevel].AsInt.ToString();
            }

            //Interpolate damage
            tooltip = tooltip.Replace("$", abilityPotency);
            JSONArray toolTipVars = jsonNode["tooltip_vars"].AsArray;

            if (toolTipVars == null)
            {
                throw new ArgumentException($"Malformed JSON encountered. element [tooltip_vars] could not be resolved.");
            }

            //Interpolate Description var-args
            for (int i = 0; i < toolTipVars.Count; i++)
            {
                tooltip = tooltip.Replace(Ability.toolTipVarTokens[i], toolTipVars[i][_skillLevel].Value);
            }

            _tooltip = tooltip;


            return this;
        }

        /// <summary>
        /// The min/max damage that an ability can inflict.
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder abilityStrength()
        {
            _abilityStrength = new AbilityStrength();

            if (_metaType == MetaType.DAMAGE || _metaType == MetaType.HEALING)
            {
                _abilityStrength.min = jsonNode[_potencyProperty][_skillLevel][0].AsInt;
                _abilityStrength.max = jsonNode[_potencyProperty][_skillLevel][1].AsInt;

            }
            return this;
        }

        /// <summary>
        /// The turns applied and potency of an ability, if it has a status effect.
        /// </summary>
        /// <returns></returns>
        public AbilityBuilder conditionStrength()
        {
            string s = _tooltip;
            foreach (int type in _typeIds)
            {
                if (AbilityUtils.effectTypes.Contains((AbilityTypes)type))
                {
                    _conditionStrength = new ConditionStrength();
                    _conditionStrength.turnsApplied = jsonNode["turns_applied"][_skillLevel].AsInt;
                    _conditionStrength.potency = jsonNode["potency"][_skillLevel].AsInt;
                    if (_tooltip != null)
                    {
                        _tooltip = _tooltip.Replace("+", _conditionStrength.turnsApplied.ToString());
                        _tooltip = _tooltip.Replace("*", EffectProcessor.getEffectLabel(_statusEffect, _conditionStrength.potency));
                    }
                    break;
                }
            }

            return this;
        }

        /// <summary>
        /// Constructs the ability from the builder's members.
        /// </summary>
        /// <returns>A fully constructed ability</returns>
        public Ability build()
        {


            Ability ability = new Ability();
            ability.name = this._name;
            ability.id = this._id;
            ability.cooldown = this._cooldown;
            ability.assetData = this._assetData;
            ability.tooltip = this._tooltip;
            ability.typeIds = this._typeIds;
            ability.conditionStrength = this._conditionStrength;
            ability.abilityStrength = this._abilityStrength;
            ability.statusEffect = this._statusEffect;
            ability.setTargetingType();

            return ability;
        }
    }
}
