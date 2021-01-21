using Assets.Scripts.Entities.Components;
using System;
using UnityEngine;

namespace Assets.Scripts.Entities.Items
{
    class Item
    {
        /// <summary>
        /// Path on disk where item data is
        /// </summary>
        public static readonly string ITEM_PATH = $"{Application.dataPath}/Resources/data/item_data/items.json";
        /// <summary>
        /// Path on disk where item icons are
        /// </summary>
        public static readonly string ICON_PATH = "textures/icon_textures/item_icons";

        public int id { get; set; }
        public Renderable iconSprite;
        public string name { get; set; }
        public int buyValue { get; set; }
        public int selValue { get; set; }
        public string description { get; set; }

        /// <summary>
        /// Used to polymorphically build special attrivutes of item sub-types
        /// </summary>
        /// <param name="itemBuilder">The itembuilder which holds the reference to the item's members</param>
        internal virtual void build(ItemBuilder itemBuilder){}
    }

    class ItemFactory
    {
        /// <summary>
        /// Factory class for creating new item types.
        /// </summary>
        /// <remarks>
        /// Item type that is consumed on usage. Note: Consumable items <b>are Useable</b> but Useable Items aren't always Consumeable.
        /// </remarks>
        /// <param name="isConsumeable"></param>
        /// <param name="isUseable">hey</param>
        /// <returns></returns>
        public static Item create(bool isConsumeable, bool isUseable)
        {
            if (isConsumeable)
            {
                return new Consumable();
            }
            else if(isUseable)
            {
                return new Equipable();
            }
            else // Likely to be quest items for now.
            {
                return new Item();
            }
        }
    }

}
