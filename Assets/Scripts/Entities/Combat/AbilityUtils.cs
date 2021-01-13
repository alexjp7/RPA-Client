namespace Assets.Scripts.Entities.Combat
{
    /// <summary>
    /// <para>
    /// Abilities can be categorised into 3 archetypes/metatypes which generally describe their intended use. 
    /// </para> 
    /// <para>
    ///  Abilties can have multiple sub-types, however their Metatype is the first listed type in the types array.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The logic to determine the Metatype can be seen in <see cref="AbilityUtils.getMetaType(int)"/>.
    /// </remarks>
    enum MetaType
    {
        /// <summary>
        /// An abilitiy that primarly inflicts damage to an enemy target.
        /// </summary>
        DAMAGE = 0,
        /// <summary>
        /// An ability that primarily restores health to an ally target.
        /// </summary>
        HEALING = 1,
        /// <summary>
        /// An ability that inflicts a status effect. See  <see cref="EffectProcessor"/> for effect types.
        /// </summary>
        /// <remarks>
        /// Abilities that have an <c>EFFECT</c> should have a  'status', 'potency' and 'turns_applied' defined in its JSON defintion.
        /// </remarks>
        EFFECT = 2,
        /// <summary>
        /// Used for error handling to flag undefined or misisng type IDs.
        /// </summary>
        UNIMPLEMENTED = 4
    }

    /// <summary>
    /// AbilityTypes indicate a more specialised description of a what the ability is intended to do. 
    /// </summary>
    /// <remarks>
    /// <para>The primary type (first type listed)  into the above MetaTypes.</para>
    /// The categorisation is as follows:
    /// <list type="bullet">
    /// <item> if type between 0 and 1 = <c>DAMAGE</c> </item>
    /// <item> else if type between 1 AND  4  = <c>HEALING </c></item>
    /// <item> else if type between 5 AND  9 = <c>EFFECT</c></item>
    /// </list>
    /// </remarks>
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

    /// <summary>
    /// The AbilityUtils provides targeting type collections and and helper methods to assist with ability labels and metatype logic relevant to all  ability types.
    /// </summary>
    class AbilityUtils
    {

        /// <summary>
        /// Auto-Targeting - Abilities that select the caster OR All allies/enemies
        /// </summary>
        public readonly static AbilityTypes[] autoTargetingTypes =
        {
            AbilityTypes.SELF_HEAL,
            AbilityTypes.SELF_BUFF,
            AbilityTypes.MULTI_DAMAGE,
            AbilityTypes.MULTI_BUFF,
            AbilityTypes.MULTI_HEAL
        };

        /// <summary>
        /// Single-Enemy-Allows for individual enemy selection.
        /// </summary>
        public readonly static AbilityTypes[] enemyTargetingTypes =
        {
            AbilityTypes.SINGLE_DAMAGE,
            AbilityTypes.SINGLE_DEBUFF
        };

        /// <summary>
        /// Single-Ally-Allows for individual ally selection.
        /// </summary>
        public readonly static AbilityTypes[] allyTargetingTypes =
        {
            AbilityTypes.SINGLE_HEAL,
            AbilityTypes.SINGLE_BUFF
        };

        /// <summary>
        /// Self-Targeting - Restricts targeting to self .
        /// </summary>
        public readonly static AbilityTypes[] selfTargetTypes =
        {
            AbilityTypes.SELF_BUFF,
            AbilityTypes.SELF_HEAL
        };

        /// <summary>
        /// Types that imply a status effect.
        /// </summary>
        public readonly static AbilityTypes[] effectTypes =
        {
            AbilityTypes.SELF_BUFF,
            AbilityTypes.SINGLE_BUFF,
            AbilityTypes.SINGLE_DEBUFF,
            AbilityTypes.MULTI_BUFF,
            AbilityTypes.MULTI_DEBUFF,
        };

        /// <summary>
        /// Helper method called when constructing an ability. used in setting potency property used in JSON parsing while returning the meta type.
        /// </summary>
        /// <remarks>
        /// Prevents repeated logic of checking an  abilities primary type.
        /// </remarks>
        /// <param name="primaryType">The first element (<c>typeIds[0]</c>) of an abilities type Ids. Indactes the primary use for that ability (Healing,Damage,Effect).</param>
        /// <param name="potencyProperty">sets the key to use in further  processing of the JSON object of an ability.</param>
        /// <returns>The metatype of an ability.</returns>
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

        /// <summary>
        /// Overload for <see cref="AbilityUtils.getMetaType(int)"/>. Used in BattleState logic for determining what type of ability is being used on a target.
        /// </summary>
        /// <param name="primaryType">The first element (<c>typeIds[0]</c>) of an abilities type Ids. Indactes the primary use for that ability (Healing,Damage,Effect).</param>
        /// <returns>The Metatype, i.e. whether the ability is classified as an Attack,Heal or Effect ability.</returns>
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

        /// <summary>
        /// Helper method to return the string of the ability type  for a tooltip description. Abilities can have multiple types and for each of those, the string label is returned to display to the user.
        /// </summary>
        /// <param name="typeIds">an array of ability types  <see cref="AbilityTypes"/></param>
        /// <returns>An array of string labels which correspond to the abilities types.</returns>
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
