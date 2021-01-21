namespace Assets.Scripts.Entities.Items
{
    using System.Collections.Generic;
    using System.IO;
    using log4net;
    using SimpleJSON;
    
    /// <summary>
    /// Container class for holding static references to game items.
    /// </summary>
    class ItemMap
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ItemMap));

        public Dictionary<int, Item> items;

        /// <summary>
        /// Singleton accessor
        /// </summary>
        public static ItemMap INSTANCE
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ItemMap();
                }

                return _instance;
            }
        }
        /// <summary>
        /// Instance holder.
        /// </summary>
        private static ItemMap _instance;

        public Item this[int itemId]
        {
            get
            {
                return items[itemId];

            }
        }

        public ItemMap()
        {
            items = new Dictionary<int, Item>();
            loadItems();
        }

        /// <summary>
        /// Loads item data from disk.
        /// </summary>
        public void loadItems()
        {
            JSONNode json = null;

            try
            {
                string rawItemData = File.ReadAllText(Item.ITEM_PATH);
                json = JSON.Parse(rawItemData)["items"];
            }
            catch (IOException e)
            {
                log.Error(e.Message);
                throw e;
            }

            buildItems(json);

        }

        /// <summary>
        /// Refernces <see cref="ItemBuilder"/> to construct each item from the passed in JSON.
        /// </summary>
        /// <param name="json">JSON collection of items.</param>
        private void buildItems(JSONNode json)
        {
            ItemBuilder builder = new ItemBuilder();

            foreach (var itemNode in json)
            {   
                Item item = builder.json(itemNode)
                            .id()
                            .name()
                            .description()
                            .value()
                            .condition()
                            .build();

                items.Add(item.id,item);
            }
        }
    }
}
