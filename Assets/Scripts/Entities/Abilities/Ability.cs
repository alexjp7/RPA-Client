/*---------------------------------------------------------------
                        ABILITY
 ---------------------------------------------------------------*/
/***************************************************************
*  The ability class is the model that represents player and 
   monster combative abilities.
**************************************************************/

using Assets.Scripts.Entities.Components;

namespace Assets.Scripts.Entities.Abilities
{

    /***************************************************************
    * ToolTipTokens represent the placeholder special characters 
      which are used to delimit and interpolate ability data
      into the tooltip description for an ability.
    
     * These values are present in the reading of the JSON file
       that contains the data for each ability, and is utilised
       during the Adventurer.loadAbilities() procedure.
    **************************************************************/
    enum ToolTipTokens
    {
        POTENCY = '$', //Interpolates the damage/potency of an ability
        EFFECT = '*' // Interpolates the status/condition that an ability can inflict/apply.
    }

    /***************************************************************
    * The AbilityStrength struct provides a generic type to represent
      the potency of an ability that have a varied effect potency.
    
    * For abilities that do NOT have a range in potency, the MAX
      value contains the effect strenght

      e.g. percentage based ability strenghts.
    **************************************************************/
    public struct AbilityStrength
    {
        public int min, max; 
        public string toString()
        {
            return max == -1 ? " Ability_Potency: " + min : " Ability_Min " + min + ", Ability_Max " + max;
        }
    }

    public class Ability
    {
        public static readonly int LEVEL_TIER_LIMIT = 3;
        public static readonly string MONSTER_ABILITY_DATA_PATH = "Assets/Resources/data/ability_data/monsters/";
        public static readonly string BASE_ABILITY_ICON_PATH = "textures/icon_textures/ability_icons/";
        public static readonly string[] toolTipVarTokens = { "@", "#" };

        //Read in values
        public string iconPath { get; set; }
        public int id { get; set; }
        public string tooltip { get; set; }
        public string name { get; set; }
        public int cooldown { get; set; }
        public int[] typeIds { get; set; }
        public int statusEffect; 
        public Renderable assetData { get; set; }
        public AbilityStrength abilityStrength { get; set; }

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
        }

        /***************************************************************
        * Checks if the amount of turns since last use has exceeded
          the cooldown to flag if the ability is ready for use.
        @param - turnCount: the amount turns taken since
        an abilites use.
        *********************************** ***************************/
        public void updateCooldown()
        {
            isOnCooldown = false;

            if (lastTurnUsed == -1) return;
            if (cooldown > 1)
            {
                cooldownTracker --;
                if ((cooldown) - cooldownTracker > cooldown)
                {
                    cooldownTracker = cooldown;
                }
                else
                {
                    isOnCooldown = true;
                }
            }
        }
        
        public void setLastTurnUsed(int turn)
        {
            lastTurnUsed = turn;
            if(turn != -1) updateCooldown();
        }
        public override string ToString()
        {
            return "Ability { id:" + id + ", name: " + name + ", tooltip: " + tooltip + ", cooldown: " + cooldown + "," + abilityStrength.toString() + "}";
        }
    }
}
