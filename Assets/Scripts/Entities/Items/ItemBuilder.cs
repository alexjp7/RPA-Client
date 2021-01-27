namespace Assets.Scripts.Entities.Items
{
    using log4net;
    using SimpleJSON;

    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Components;

    /// <summary>
    /// Builder class to extract item data from json file.
    /// </summary>
    class ItemBuilder
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ItemBuilder));

        // Read in values common to all items
        private int _id;
        private string _name;
        private string _description;
        private int _buyValue;
        private int _sellValue;

        //Propertys belonging to Item sub-type
        internal Condition _condition;

        private bool isUseable;
        private bool isConsumeable;

        private JSONNode jsonNode;

        /// <summary>
        /// Sets the JSON node that will be used to parse out item attributes.
        /// </summary>
        /// <param name="jsonNode">JSON element which contains Item attributes to be pasred</param>
        public ItemBuilder json(JSONNode jsonNode)
        {
            this.jsonNode = jsonNode;
            isUseable = jsonNode["is_useable"].AsBool;
            isConsumeable = jsonNode["is_consumeable"].AsBool;

            return this;
        }

        /// <summary>
        /// Extracts the item's ID
        /// </summary>
        public ItemBuilder id()
        {
            _id = jsonNode["id"].AsInt;
            return this;
        }

        /// <summary>
        /// Extracts the item's name
        /// </summary>
        public ItemBuilder name()
        {
            _name = jsonNode["name"];
            return this;
        }

        /// <summary>
        /// Extracts the item's coin values
        /// </summary>
        public ItemBuilder value()
        {
            JSONArray coinValues = jsonNode["coin_value"].AsArray;
            _buyValue= coinValues[0].AsInt;
            _sellValue = coinValues[1].AsInt;

            return this;
        }

        /// <summary>
        /// Extracts the item's description
        /// </summary>
        public ItemBuilder description()
        {
            _description = jsonNode["description"];
            return this;
        }

        /// <summary>
        /// Extracts consumeable items condition.
        /// </summary>
        public ItemBuilder condition()
        {
            if (isConsumeable)
            {
                int effectId = jsonNode["use_effect"].AsInt;
                int potency = jsonNode["effect_potency"].AsInt;
                int turnsApplied = jsonNode["turns_applied"].AsInt;

                _condition = new Condition(effectId, potency, turnsApplied);
            }
            return this;
        }

        /// <summary>
        /// Creates the final Item object with its members set to what was extracted from JSON. 
        /// </summary>
        public Item build()
        {
            Item item = ItemFactory.create(isConsumeable, isUseable);
            item.id = _id;
            item.name = _name;
            item.description = _description;
            item.iconSprite = new Renderable(_name, Item.ICON_PATH+"/"+ _name);
            item.buyValue = _buyValue;
            item.sellValue = _sellValue;

            item.build(this);

            return item;
        }
    }
}
