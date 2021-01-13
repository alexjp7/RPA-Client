/*---------------------------------------------------------------
                        ABILITY
 ---------------------------------------------------------------*/
/***************************************************************
*  The ability class is the model that represents player and 
   monster combative abilities.
**************************************************************/
namespace Assets.Scripts.Entities.Combat
{
    using SimpleJSON;
    using System;

    using Assets.Scripts.Entities.Components;

    /// <summary>
    /// <para>
    /// ToolTipTokens represent the <b>placeholder</b> special characters which are used to delimit and interpolate
    /// ability data into the tooltip description for an ability.
    /// </para>
    /// These values are present in the reading of the JSON file that contains the data for each ability,
    /// and is utilised during the <see cref="Containers.Abilities.loadAbilityData(string, Components.CombatantType, bool)"/>.
    /// </summary>
    enum ToolTipTokens
    {
        /// <summary>
        /// Interpolates the damage/potency of an ability
        /// </summary>
        POTENCY = '$',
        /// <summary>
        /// Interpolates the status/condition that an ability can inflict/apply.
        /// </summary>
        EFFECT = '*' 
    }


    /// <summary>
    /// The AbilityStrength struct provides a generic type to represent the potency of an ability that have a varied effect potency.
    /// </summary>
    /// <remarks>
    /// For abilities that do NOT have a range in potency, the MAX  value contains the effect strenght.
    /// e.g. percentage based ability strengths.
    /// </remarks>
    public struct AbilityStrength
    {
        public int min, max; 
        public string toString()
        {
            return max == -1 ? " Ability_Potency: " + min : " Ability_Min " + min + ", Ability_Max " + max;
        }
    }

    /// <summary>
    /// <para>
    /// The properties relating to a conditions potency and for how long it is applied. 
    /// </para>
    /// potency can relate to the amount of damage of time
    /// </summary>
    public struct ConditionStrength
    {
        public int potency, turnsApplied;
    }

    /// <summary>
    /// Targeting types used for player/monster abilities. targeting types are used to determine whether an ability will target allies, enemies
    /// </summary>
    public enum TargetingType
    {
        /// <summary>
        /// An ability that has an <b>AUTO</b> targeting type automatically will select the appropriate targets, without offering a targeting
        /// selection for the combatant. 
        /// See <see cref="AbilityUtils.autoTargetingTypes">Auto types</see>
        /// </summary>
        AUTO = 0,

        /// <summary>
        /// An ability that has an <b>ENEMY</b> targeting type will offer a selection of available enemy combatants.
        /// See <see cref="AbilityUtils.enemyTargetingTypes">Enemy types</see>
        /// </summary>
        ENEMY = 1,

        /// <summary>
        /// An ability that has an <b>ALLIED</b> targeting type will ofer a selection of availale ally combatants. 
        /// See <see cref="AbilityUtils.allyTargetingTypes">Ally types</see>
        /// </summary>
        ALLIED = 2,
    }

    /// <summary>
    /// Data model which contains access to a combatants ability damage, potency, description and icons.
    /// </summary>
    public class Ability
    {
        /// <summary>
        /// Each ability will have a defined amount of tiers of which reflect the abilities overall strength. This limit
        /// denotes the maximun level that each ability can be progressed to.
        /// </summary>
        /// <remarks>
        /// Progressing an ability up a level-tier during gameplay has the potential to alter the effects or nature of 
        /// the ability as comapred to its base tier.
        /// </remarks>
        public static readonly int LEVEL_TIER_LIMIT = 3;

        /// <summary>
        /// Location on disk where the ability data can be found.
        /// </summary>
        public static readonly string MONSTER_ABILITY_DATA_PATH = "Assets/Resources/data/ability_data/monsters/";

        /// <summary>
        /// Location on disk where ability icon's can be found.
        /// </summary>
        public static readonly string BASE_ABILITY_ICON_PATH = "textures/icon_textures/ability_icons/";

        /// <summary>
        /// <para>
        /// Placeholder tokens which are used to interpolate the appropriate tooltip description based on the abilities tier.
        /// The amount of tokens in this array represents the amount of variable descriptions that can exist in an ability description.
        /// </para>
        /// <para>
        /// Each element in this array corresponds to a top-level element in the <c>tooltip_vars</c> which can be found 
        /// in an abilities JSON definition .
        /// </para>
        /// </summary>
        /// <remarks>
        /// When an ability advances we want to ensure that a fitting description accompanies differing tiers of an ability.
        /// This is mainly to incorproate a degree of 'flavour text' onto an abilities description.
        /// </remarks>
        public static readonly string[] toolTipVarTokens = { "@", "#" };

        public bool isEnemyTargetable { get; private set;}

        //Read in values
        public TargetingType targetingType { get; private set; }
        public string iconPath { get; set; }
        public int id { get; set; }
        public string tooltip { get; set; }
        public string name { get; set; }
        public int cooldown { get; set; }
        public int[] typeIds { get; set; }
        public int statusEffect; 
        public Renderable assetData { get; set; }
        public AbilityStrength abilityStrength { get; set; }
        public ConditionStrength conditionStrength { get; set; }

        //Cooldown trackers
        public bool isOnCooldown { get; private set; }
        public int lastTurnUsed { get; private set; }
        public int cooldownTracker { get; set;}

        
        public Ability() { }

        public Ability(int id,int[] typeIds ,string name, string tooltip, int cooldown,
                       int statusEffect, AbilityStrength abilityStrength, Renderable assetData)
        {
            this.id = id;
            this.typeIds = typeIds;
            this.name = name;
            this.tooltip = tooltip;
            this.cooldown = cooldown;
            this.abilityStrength = abilityStrength;
            this.assetData = assetData;
            this.isOnCooldown = false;
            this.statusEffect = statusEffect;
            this.lastTurnUsed = -1;
            this.cooldownTracker = cooldown;
            setTargetingType();
          
        }
        /// <summary>
        /// Sets general <see cref="targetingType">targeting type</see> grouping of an ability based on its exact targeting nature.
        /// </summary>
        public void setTargetingType()
        {
            if (Array.Exists(AbilityUtils.enemyTargetingTypes, type => (int)type == typeIds[0]) ) 
            {
                targetingType = TargetingType.ENEMY;
            }
            else if(Array.Exists(AbilityUtils.allyTargetingTypes, type => (int)type == typeIds[0]) )
            {
                targetingType = TargetingType.ALLIED;
            }
            else if (Array.Exists(AbilityUtils.autoTargetingTypes, type => (int)type == typeIds[0]) )
            {
                targetingType = TargetingType.AUTO;
            }
        }

        /// <summary>
        ///  Checks if the amount of turns since last use has exceeded the cooldown to flag if the ability is ready for use.
        /// </summary>
        public void updateCooldown()
        {
            if (lastTurnUsed == -1) return;

            //Don't put abilities with 1 turn CD on cooldown.
            if (cooldown > 1)
            {
                cooldownTracker --;
                if ((cooldown) - cooldownTracker > cooldown)
                {
                    resetCooldown();
                }
                else
                {
                    isOnCooldown = true;
                }
            }
        }

        /// <summary>
        /// Resets the cooldown of the ability to its initial value;
        /// </summary>
        public void resetCooldown()
        {
            isOnCooldown = false;
            cooldownTracker = cooldown;
            lastTurnUsed = -1;

        }

        /// <summary>
        /// <para>
        /// When an abilities is used, we want to keep track of the turn count of when it was last used.
        /// </para>
        /// This provides a mechanism for updating cooldowns, and deteremining whetherh the ability is usable again through <see cref="updateCooldown"/>
        /// </summary>
        /// <param name="turn">The turn count of when the ability was used.</param>
        public void setLastTurnUsed(int turn)
        {
            lastTurnUsed = turn;
            if (turn != -1)
            {
                updateCooldown();
            }
        }


        public JSONObject toJson()
        {
            JSONObject json = new JSONObject();
            JSONArray typeIdJson = new JSONArray();
            JSONArray abilityStrenghJson = new JSONArray();
            JSONArray conditionStrengthJson = new JSONArray();

            foreach(var type in typeIds)
            {
                typeIdJson.Add(type);
            }

            abilityStrenghJson.Add(abilityStrength.min);
            abilityStrenghJson.Add(abilityStrength.max);

            conditionStrengthJson.Add(conditionStrength.potency);
            conditionStrengthJson.Add(conditionStrength.turnsApplied);

            json.Add("name", name);
            json.Add("type_ids", typeIdJson);
            json.Add("status_effet", statusEffect);
            json.Add("ability_strength", abilityStrenghJson);
            json.Add("condition_strength", conditionStrengthJson);
            json.Add("targeting_type", (int) targetingType);

            return json;
        }
        public override string ToString()
        {
            return "Ability { id:" + id + ", name: " + name + ", tooltip: " + tooltip + ", cooldown: " + cooldown + "," + abilityStrength.toString() + "}";
        }
    }
}
