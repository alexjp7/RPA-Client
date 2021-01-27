namespace Assets.Scripts.Entities.Items
{
    using Assets.Scripts.Entities.Components;
    using System;

    class Equipable : Item, Useable
    {
        public void use(Combatant target)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        internal override void build(ItemBuilder itemBuilder)
        {
            //To-do: Figure up equiping stuff.
        }
    }
}
