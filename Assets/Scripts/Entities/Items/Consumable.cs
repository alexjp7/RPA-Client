namespace Assets.Scripts.Entities.Items
{
    using System;
    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Components;

    class Consumable : Item, Useable
    {
        /// <summary>
        /// Condition applied upon use of a consumable item.
        /// </summary>
        public Condition condition { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void use(Combatant target)
        {
            target.applyEffect(condition);
        }

        /// <summary>
        /// <inheritdoc/>
        /// Sets the value for a consumeable items condition.
        /// </summary>
        /// <param name="builder"></param>
        internal override void build(ItemBuilder builder)
        {
            this.condition = builder._condition;
        }
    }
}
