
namespace Assets.Scripts.UI.Combat
{
    using UnityEngine;

    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.UI.Common;

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

            AbilityButtons abilityButtons = AbilityButtons.create(Game.clientSidePlayer.playerClass.abilities, transform.GetComponent<RectTransform>());
            AbilityButtons abilityButtons2 = AbilityButtons.create(Game.clientSidePlayer.playerClass.abilities, transform.GetComponent<RectTransform>());

            tabs.Add(abilityButtons);
            tabs.Add(abilityButtons2);
            init();
        }
    }
}
