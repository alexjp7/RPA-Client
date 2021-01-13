using Assets.Scripts.Entities.Items;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Entities.Containers
{
    class Inventory
    {
        public static readonly int INVENTORY_CAPACITY = 5;
        private static readonly ILog log = LogManager.GetLogger(typeof(Inventory));

        private List<Item> items = new List<Item>();
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

        /// <summary>
        /// Generates UI components for each item in an Adventurer's inventory.
        /// </summary>
        /// <param name="inventoryPanel">The UI container for inventory items.</param>
        public static void generateInventoryUI(in Transform inventoryPanel)
        {
            int childs = inventoryPanel.childCount;
            for (var i = childs - 1; i >= 0; i--)
            {
                GameObject.Destroy(inventoryPanel.GetChild(i).gameObject);
            }
        }
    }
}
