
namespace Assets.Scripts.Entities.Containers
{
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    using Assets.Scripts.Entities.Items;
    using Assets.Scripts.Entities.Components;
    using System.Collections;

    public class Inventory : IEnumerable<Item>
    {
        public static readonly int INVENTORY_CAPACITY = 5;
        private static readonly ILog log = LogManager.GetLogger(typeof(Inventory));

        private List<Item> items;
        private int coins;

        public Item this[int index]
        {
            get
            {
                return get(index);
            }
        }

        public Inventory()
        {
            items = new List<Item>();
            items.Add(ItemMap.INSTANCE["Basic Health Potion"]);
        }

        public int Count
        {
            get
            {
                return items.Count;
            }
        }

        public Item get(int i)
        {
            if (i < 0 || i >= items.Count())
            {
                return null;
            }
            return items[i];
        }

        /// <summary>
        /// Add items to inventory.
        /// </summary>
        /// <param name="newItem">The item to add to an inventory.</param>
        /// <returns>
        /// <list type="bullet"> 
        /// <item> true - The item was succesfully added to inventory.</item>
        /// <item> false - The item was not added due to having a full invetory.</item>
        /// </list>
        /// </returns>
        public bool add(Item newItem)
        {
            if (items.Count >= INVENTORY_CAPACITY)
            {
                return false;
            }

            items.Add(newItem);

            return true;
        }

        /// <summary>
        /// Removes item from invetory
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool remove(int i)
        {
            if (i < 0 || i >= items.Count)
            {
                return false;
            }

            items[i] = null;

            return true;
        }

        /// <summary>
        /// Checks to see if item is <see cref="Useable"/>, then executes its <c>use()</c> method.
        /// </summary>
        /// <param name="i"> The element of inventory that will be used</param>
        /// <param name="isConsumeable">is set if used item is consumeable</param>
        /// <returns>
        /// <list type="bullet"> 
        /// <item> true - The item was useable and its use fucntion was executed</item>
        /// <item> false - The item is not useable</item>
        /// </list>
        /// </returns>
        public bool use(int i, Combatant target, out bool isConsumeable)
        {
            Useable item = get(i) as Useable;
            isConsumeable = false;
            if (item == null)
            {
                return false;
            }

            item.use(target);

            item = get(i) as Consumable;
            if (item != null)
            {
                remove(i);
                isConsumeable = true;
            }

            return true;
        }

        public IEnumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator<Item> IEnumerable<Item>.GetEnumerator()
        {
            return (IEnumerator<Item>)GetEnumerator();
        }
    }
}
