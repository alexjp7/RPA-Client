
/*---------------------------------------------------------------
                       combatant
 ---------------------------------------------------------------*/
/***************************************************************
*  The Combatant type contains fields relating to combat
   including; turn order, health properties (Damageable), abilities
   and the base logic for applying damage, healing and status 
   effects to these types of objects. The combatant base
   provides a abstract type to allow for generic handling
   of both player and monster types.
**************************************************************/
namespace Assets.Scripts.Entities.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SimpleJSON;

    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Containers;
    using Assets.Scripts.UI.Combat;

    /// <summary>
    /// Combatant types used to be able
    /// to determine child object type from parent.
    /// </summary>
    public enum CombatantType
    {
        PLAYER,
        MONSTER
    }

    public abstract class Combatant
    {
        /// <summary>
        /// The root location of sprite textures, relative from the /Assets/Data directory 
        /// </summary>
        private static readonly string BASE_SPRITE_PATH = "textures/sprite_textures/";

        public int id { get; set; }
        public string name { get; set; }
        public CombatantType type { get; protected set; }
        protected string assetPath { get; set; }

        //Components
        public Renderable assetData { get; set; }
        public Damageable healthProperties { get; protected set; }

        public Abilities abilities { get; protected set; }

        public Dictionary<int, Condition> conditions { get; set; }

        /// <summary>
        /// Accessing this member will instantiate a combatants
        /// combat sprite if it is <c>null</c>.
        /// </summary>
        /// <returns>
        /// Returns the combat sprite for a combatant.
        /// </returns>
        public CombatSprite combatSprite
        {
            get
            {
                if (_combatSprite == null)
                {
                    _combatSprite = CombatSprite.create(this);
                }
                return _combatSprite;
            }
        }
        /// <summary>
        /// Instance holder for combat sprite
        /// </summary>
        private CombatSprite _combatSprite;

        /// <summary>
        /// Checks if combatant has an impairing status effect which will prevent them from taking their turn.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// true - the combatant has an impairing effect on their status bar, which prevents them from taking their turn.
        /// </item>
        /// <item>
        /// false - the combatant is <b>not</b> currently affected by an imparing effect and can perform their turn as normal.
        /// </item>
        /// </list>
        /// </returns>
        public bool isImpaired
        {
            get
            {
                foreach (var condition in this.conditions)
                {
                    if (EffectProcessor.ImpairingEffects.Contains((StatusEffect)condition.Key))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Instantaites a combatants renderable asset data, and empty status effect map.
        /// </summary>
        public Combatant()
        {
            assetPath = BASE_SPRITE_PATH;
            assetData = new Renderable();
            conditions = new Dictionary<int, Condition>();
        }

        /// <summary>
        /// <para>
        /// Sets the path to load/retrieve the asset of this combatant.
        /// </para>
        /// Sprite asset path's are donated by the monster's name appended onto the base asset directory location
        /// </summary>
        /// <param name="spriteName"> The <c>toString()</c> value of the player/monster enum.</param>
        protected void setSpritePath(string spriteName)
        {
            assetData.path = assetPath + spriteName;
        }

        /// <summary>
        /// Provides the base logic for dealing damage.
        /// </summary>
        /// <param name="minDamage">The lower bound of the damage being applied that is used to calculate the actual amount dealt.</param>
        /// <param name="maxDamage">The upper bound of the damage being applied that is used to calculate the actual amount dealt.</param>
        /// <returns>The actual damage dealt.</returns>
        public virtual int applyDamage(int minDamage, int maxDamage)
        {
            float damageDealt = Util.Random.getInt(minDamage, maxDamage);
            float totalDamageModifier = 0;
            float damageAmped = 0;
            float damageAbsorbed = 0;

            if (conditions.Count > 0)
            {
                if (conditions.ContainsKey((int)StatusEffect.DAMAGE_TAKEN_UP))
                {
                    damageAmped = conditions[(int)StatusEffect.DAMAGE_TAKEN_UP].potency;
                }

                if (conditions.ContainsKey((int)StatusEffect.DAMAGE_TAKEN_DOWN))
                {
                    damageAbsorbed = conditions[(int)StatusEffect.DAMAGE_TAKEN_DOWN].potency;
                }
            }

            totalDamageModifier = damageAmped + damageAbsorbed;

            //Apply Damage Modifiers
            if (totalDamageModifier < 0)
            {
                damageDealt = damageDealt * (1 - Math.Abs(totalDamageModifier) / 100);
            }
            else if (totalDamageModifier > 0)
            {
                damageDealt = damageDealt * (1 + Math.Abs(totalDamageModifier) / 100);
            }

            healthProperties.currentHealth -= damageDealt;

            return (int)damageDealt;
        }


        /// <summary>
        /// For applying a fixed amoount of damage
        /// </summary>
        /// <param name="damage">the precalculated damage to be dealt</param>
        /// <returns>The actaul damage dealt</returns>
        public virtual int applyDamage(int damage)
        {
            healthProperties.currentHealth -= damage;
            return (int)damage;
        }

        /// <summary>
        /// Provides the base logic for applying a hp recovery/healing effect to a combatant.
        /// </summary>
        /// <remarks>This is for applying variable healing to the target.</remarks>
        /// <param name="minValue">The lower bound of the healing being applied that is used to calculate the actual amount healed. </param>
        /// <param name="maxValue">The upper bound of the healing being applied that is used to calculate the actual amount healed.</param>
        /// <returns>The amount healed</returns>
        public virtual int applyHealing(int minValue, int maxValue)
        {
            float damageHealed = Util.Random.getInt(minValue, maxValue);
            healthProperties.currentHealth += damageHealed;
            healthProperties.currentHealth = Math.Min(healthProperties.currentHealth, healthProperties.maxHealth);
            return (int)damageHealed;
        }
        /// <summary>
        /// Provides the base logic for applying a hp recovery/healing effect to a combatant. 
        /// </summary>
        /// <remarks>
        /// This provides a flat healing value to the target
        /// </remarks>
        /// <param name="minValue">The lower bound of the healing being applied that is used to calculate the actual amount healed. </param>
        /// <param name="maxValue">The upper bound of the healing being applied that is used to calculate the actual amount healed.</param>
        /// <returns>The amount healed</returns>
        public virtual int applyHealing(int value)
        {
            healthProperties.currentHealth += value;
            healthProperties.currentHealth = Math.Min(healthProperties.currentHealth, healthProperties.maxHealth);
            return (int)value;
        }
        /// <summary>
        /// <para>
        /// Applies the status effect to this combatant.
        /// </para>
        /// See the <see cref="Assets.Scripts.Entities.Combat.EffectProcessor"/> class for effect processing logic.
        /// </summary>
        /// <param name="statusEffect">ID for an ability status effect. see EffectProcess.cs.</param>
        /// <param name="potency">The strength or duration if applicable of the status effect</param>
        /// <param name="turnsApplied">The amount of combat turns the affect will be applied for.</param>
        public virtual void applyEffect(int statusEffect, int potency, int turnsApplied)
        {
            EffectProcessor.getInstance().applyEffect(this, statusEffect, potency, turnsApplied);
        }

        /// <summary>
        /// Applies the status effect to this combatant
        /// </summary>
        /// <param name="condition"></param>
        public virtual void applyEffect(in Condition condition)
        {
            applyEffect(condition.effectId, condition.potency, condition.turnsRemaining);
        }

        /// <summary>
        /// Updates cooldown counters for each ability.
        /// </summary>
        public virtual void updateAbilityCooldowns()
        {
            abilities.updateCooldowns();
        }

        /// <summary>
        /// Resets all abilities cooldowns, so they can be used for a new encounter.
        /// </summary>
        public virtual void resetAbilityCooldowns()
        {
            abilities.resetCooldowns();
        }

        /// <summary>
        /// Updates cooldown counters for each ability, and return the list of removed conditions to UI layer. 
        /// </summary>
        /// <returns> The list of conditions that have been removed. </returns>
        public List<int> updateConditionDurations()
        {
            List<int> removedConditions = new List<int>();

            foreach (var condition in conditions)
            {
                int effectDuraction = condition.Value.reduceEffect(1);
                if (effectDuraction <= 0)
                {
                    removedConditions.Add(condition.Value.effectId);
                }
            }
            removedConditions.ForEach(condition => conditions.Remove(condition));

            return removedConditions;
        }

        /// <summary>
        /// Provides the combatants Max HP.
        /// </summary>
        /// <returns>The value of the combatants <b>MAX</b> health.</returns>
        public float getMaxHp()
        {
            return healthProperties.maxHealth;
        }

        /// <summary>
        /// Provides teh combatants current HP.
        /// </summary>
        /// <returns>The value fo the combatnts <b>CURRENT</b> health.</returns>
        public float getCurrentHp()
        {
            return healthProperties.currentHealth;
        }

        /// <summary>
        /// Checks to see if the combatant is still alive.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// true - The combatant's current health points if greater then <value>0</value>
        /// </item>
        /// <item>
        /// false - The combatant's current health points has fallen below <value>0</value>, and should be removed from the battle.
        /// </item>
        ///</list>
        /// </returns>
        public bool isAlive()
        {
            return healthProperties.currentHealth > 0;
        }

        /// <summary>
        /// Helper function used to calculate the combatant's health as a percentage.
        /// </summary>
        /// <returns> the health precentage as a value between <value> 0 - 1</value></returns>
        public float getHealthPercent()
        {
            return healthProperties.getHealthPercentage();
        }

        /// <summary>
        /// Resets the combatants health and cooldowns.
        /// </summary>
        public void reset()
        {
            healthProperties.currentHealth = healthProperties.maxHealth;
            resetAbilityCooldowns();
        }

        public JSONObject toJson()
        {
            JSONObject json = new JSONObject();

            json.Add("id", id);
            json.Add("name", name);
            json.Add("type", type.ToString());
            json.Add("maxHp", healthProperties.maxHealth);

            return json;
        }

        public override string ToString()
        {
            return toJson().ToString();
        }
    }
}
