
namespace Assets.Scripts.Entities.Containers
{
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    using Assets.Scripts.Entities.Items;

    class Inventory
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


        public Item get(int i)
        {
            if (i >= items.Count())
            {
                ArgumentOutOfRangeException exception = new ArgumentOutOfRangeException($"Index [{i}] exceeds container [Inventory] size of [{items.Count}]");
                log.Error(exception.Message);
                throw exception;
            }
            return items[i];
        }


        public Inventory()
        {
            items = new List<Item>();
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
        /// Checks to see if item is <see cref="Useable"/>, then executes its <c>use()</c> method.
        /// </summary>
        /// <param name="i"> The element of inventory that will be used</param>
        /// <returns>
        /// <list type="bullet"> 
        /// <item> true - The item was useable and its use fucntion was executed</item>
        /// <item> false - The item is not useable</item>
        /// </list>
        /// </returns>
        public bool use(int i)
        {
            Useable item = get(i) as Useable;
            if (item != null)
            {
                return false;
            }

            item.use();


            return true;
        }
    }
}
