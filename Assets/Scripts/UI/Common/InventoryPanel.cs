namespace Assets.Scripts.UI.Common
{
    using UnityEngine;
    using Assets.Scripts.Entities.Containers;
    using UnityEngine.UI;
    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.Util;
    using UnityEngine.EventSystems;

    class InventoryPanel : TabComponent
    {
        private Inventory inventory;
        private static readonly string ITEM_IMAGE = "image_item";

        /// <summary>
        /// Static instantiation method for creating an InventoryPanel
        /// </summary>
        /// <param name="inventory">The player's inventory that will be rendered</param>
        /// <param name="parent">Parent Transform to be rendered</param>
        /// <returns></returns>
        public static InventoryPanel create(in Inventory inventory, RectTransform parent)
        {
            GameObject gameObject = new GameObject("inventory");
            InventoryPanel inventoryPanel = gameObject.AddComponent<InventoryPanel>();
            inventoryPanel.setData(inventory, parent);

            return inventoryPanel;
        }

        /// <summary>
        /// Initializes data of the inventory panel
        /// </summary>
        private void setData(Inventory inventory, RectTransform parent)
        {
            this.tabName = "Inventory";
            this.inventory = inventory;
            inventory = new Inventory();


            HorizontalLayoutGroup layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.spacing = 10;

            for (int i = 0; i < Game.clientSidePlayer.inventorySize; i++)
            {
                Transform inventorySlotTransform = Instantiate(GameAssets.INSTANCE.inventorySlotPrefab, Vector2.zero, Quaternion.identity);
                inventorySlotTransform.SetParent(gameObject.transform);


                setEventHandlers(inventorySlotTransform, i);
            }

            drawInventory();

        }

        /// <summary>
        /// Draws inventory based on current state of inventory slots.
        /// </summary>
        private void drawInventory()
        {
            for (int i = 0; i < inventory.Count; i++)
            {
                Transform slot = transform.GetChild(i);
                Image icon = slot.Find(ITEM_IMAGE).GetComponent<Image>();

                if (inventory[i] != null)
                {
                    icon.sprite = inventory?[i]?.iconSprite?.sprite;
                }
                else
                {
                    icon.gameObject.SetActive(false);
                }
            }
        }

        private void setEventHandlers(Transform inventorySlotTransform, int i)
        {
            // Create local capture of variable to ensure callback methods are set with appropriate index.
            int j = i;
            inventorySlotTransform.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry mouseEnterEvent;
            EventTrigger.Entry mouseExitEvent;
            EventTrigger.Entry mouseClickeEvent;

            //Initialize Party Sprite Events
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
            mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            mouseClickeEvent.eventID = EventTriggerType.PointerClick;

            mouseEnterEvent.callback.AddListener(evt => onInventorySlothoverEnter(j));
            mouseExitEvent.callback.AddListener(evt => onInventorySlothoverExit());
            mouseClickeEvent.callback.AddListener(evt => onSlotClicked(j));

            inventorySlotTransform.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            inventorySlotTransform.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            inventorySlotTransform.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }

        /// <summary>
        /// Shows the item tooltip.
        /// </summary>
        /// <param name="slotIndex"></param>
        public void onInventorySlothoverEnter(int slotIndex)
        {
            Tooltip.show(slotIndex >= inventory.Count || string.IsNullOrEmpty(inventory[slotIndex]?.description) ? "Empty" : inventory[slotIndex]?.description);
        }

        /// <summary>
        /// Event handler for hiding the inventory slot tooltips upon mouse exit.
        /// </summary>
        public void onInventorySlothoverExit()
        {
            Tooltip.hide();
        }

        /// <summary>
        /// Executes slots use effect if it has one.
        /// </summary>
        /// <param name="slotIndex">Item slot selected</param>
        private void onSlotClicked(int slotIndex)
        {
            if (slotIndex < -1 || slotIndex >= inventory.Count) return;

            bool isConsumeable;
            inventory.use(slotIndex, Game.clientSidePlayer.playerClass, out isConsumeable);

            if (isConsumeable)
            {
                drawInventory();
            }
        }
    }
}
