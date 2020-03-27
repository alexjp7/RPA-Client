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
       during the AdventuringClass.loadAbilities() procedure.
    **************************************************************/
    enum ToolTipTokens
    {
        POTENCY = '$', //Interpolates the damage/potency of an ability
        EFFECT = '*' // Interpolates the status/condition that an ability can inflict/apply.
    }

    /***************************************************************
    * The AbilityStrength struct provides a generic type to represent
      the potency of an ability that have a varied effect potency.
    
    * For abilities that do NOT have a range in potency, the minimun
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

        public static readonly int LEVEL_TIER_LIMIT= 3;
        public static readonly string[] toolTipVarTokens = { "@", "#" };
        public Renderable assetData { get; set; }

        public AbilityStrength abilityStrength { get; set; }
        public int id { get; set; }
        public string tooltip { get; set; }
        public string name { get; set; }
        public int cooldown{ get; set; }
        public int[] typeIds;
        
        public Ability() { }

        public Ability(int id,int[] typeIds ,string name, string tooltip, int cooldown, AbilityStrength abilityStrength, Renderable assetData)
        {
            this.id = id;
            this.typeIds = typeIds;
            this.name = name;
            this.tooltip = tooltip;
            this.cooldown = cooldown;
            this.abilityStrength = abilityStrength;
            this.assetData = assetData;
        }

        public override string ToString()
        {
            return "Ability { id:" + id + ", name: " + name + ", tooltip: " + tooltip + ", cooldown: " + cooldown + "," + abilityStrength.toString() + "}";
        }
    }
}
