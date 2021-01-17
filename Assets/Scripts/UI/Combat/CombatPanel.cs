namespace Assets.Scripts.UI.Combat
{
    using UnityEngine;

    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.UI.Common;
    using Assets.Scripts.Entities.Containers;
    using UnityEngine.UI;

    /// <summary>
    /// The UI container class for displaying the combat panel including Inventory, ability buttons and player info.
    /// </summary>
    class CombatPanel : MultiTabPanel
    {
        public static CombatPanel create(RectTransform parent)
        {
            GameObject gameObject = new GameObject("combat_tab");
            CombatPanel combatPanel = gameObject.AddComponent<CombatPanel>();
            combatPanel.setData(parent);

            return combatPanel;
        }


        /// <summary>
        /// Sets default tabe properties. Additionally adds combat panel tabs (Abilities, inventory, player info).
        /// </summary>
        /// <param name="parent"> Transform that this panel will be attached to</param>
        protected override void setData(RectTransform parent)
        {
            base.setData(parent);

            RectTransform rectTransform = transform.GetComponent<RectTransform>();

            AbilityButtons abilities = AbilityButtons.create(Game.clientSidePlayer.playerClass.abilities);
            InventoryPanel inventory = InventoryPanel.create(new Inventory(), rectTransform);

            tabs.Add(abilities);
            tabs.Add(inventory);

            init();
        }

        /// <summary>
        /// <inheritdoc/>
        /// <para>
        /// Sets each tab to centered within the combat panel, and inherit the combat panels width/height.
        /// </para>
        /// </summary>
        protected override void setTransformDimensions()
        {
            RectTransform parentTransform = gameObject.GetComponent<RectTransform>();
            foreach (TabComponent tab in tabs)
            {
                RectTransform tabTransform = tab.gameObject.GetComponent<RectTransform>();
                tabTransform = tabTransform ? tabTransform : tab.gameObject.AddComponent<RectTransform>();

                tabTransform.SetParent(this.transform);
                tabTransform.anchoredPosition = new Vector2(0, 0);

                tabTransform.sizeDelta = new Vector2(parentTransform.rect.width, parentTransform.rect.height);
                tabTransform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
