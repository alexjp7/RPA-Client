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

        public static InventoryPanel create(in Inventory inventory, RectTransform parent)
        {
            GameObject gameObject = new GameObject("inventory");
            InventoryPanel inventoryPanel = gameObject.AddComponent<InventoryPanel>();
            inventoryPanel.setData(inventory, parent);

            return inventoryPanel;
        }

        private void setData(Inventory inventory, RectTransform parent)
        {
            this.tabName = "Inventory";
            this.inventory = inventory;

            HorizontalLayoutGroup layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.spacing = 10;

            for (int i = 0; i < Game.clientSidePlayer.inventorySize; i++)
            {
                Transform inventorySlotTransform = Instantiate(GameAssets.INSTANCE.inventorySlotPrefab, Vector2.zero, Quaternion.identity);
                inventorySlotTransform.SetParent(gameObject.transform);
                setEventHandlers(inventorySlotTransform);
            }
        }

        private void setEventHandlers(Transform inventorySlotTransform)
        {
            inventorySlotTransform.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry mouseEnterEvent;
            EventTrigger.Entry mouseExitEvent;
            //EventTrigger.Entry mouseClickeEvent;

            //Initialize Party Sprite Events
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
           // mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            //mouseClickeEvent.eventID = EventTriggerType.PointerClick;

            mouseEnterEvent.callback.AddListener(evt => onAbilityHoverEnter());
            mouseExitEvent.callback.AddListener(evt => onAbilityHoverExit());
            //     mouseClickeEvent.callback.AddListener(evt => onSpriteClicked(combatantRef));

            inventorySlotTransform.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            inventorySlotTransform.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            //gameObject.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }

        public void onAbilityHoverEnter()
        {
            Tooltip.show("Empty");
        }

        /// <summary>
        /// Event handler for hiding the inventory slot tooltips upon mouse exit.
        /// </summary>
        public void onAbilityHoverExit()
        {
            Tooltip.hide();
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
