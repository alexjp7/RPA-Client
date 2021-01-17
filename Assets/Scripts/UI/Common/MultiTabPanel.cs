namespace Assets.Scripts.UI.Common
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Generic component for tabbed panels. Handles general logic.
    /// </summary>
    class MultiTabPanel : MonoBehaviour
    {
        public TabComponent currentTab => tabs[currentIndex];

        public List<TabComponent> tabs { get; private set; }
        public int count => tabs.Count;
        private int currentIndex = 0;

        public TabComponent this[int index]
        {
            get
            {
                return tabs[index];
            }
        }

        public MultiTabPanel()
        {
            tabs = new List<TabComponent>();
        }

        /// <summary>
        /// Sets component level data for MultiTabPanels to be positioned center within its parent and the tab being same size as parent.
        /// </summary>
        /// <remarks>
        /// Override this and call <see cref="setData(RectTransform)">base.setData()</see> to invoke these default properties.
        /// </remarks>
        /// <param name="parent"> Parent element that this panel will be attached to</param>
        protected virtual void setData(RectTransform parent)
        {
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            gameObject.transform.SetParent(parent);

            rectTransform.anchoredPosition = new Vector2(0, 0);
            rectTransform.sizeDelta = new Vector2(parent.rect.width, parent.rect.height);
            rectTransform.localScale = new Vector3(1, 1, 1);
        }

        /// <summary>
        /// Initializes the default behaviours and transform's dimensations of a MultiTabPanel.
        /// <list type="bullet">
        /// <item> All tabs are added to the panel</item>
        /// <item> The default(first) tab is set to show, while all other tabs are hidden.</item>
        /// </list>
        /// </summary>
        protected void init()
        {
            tabs.ForEach(tab =>
            {
                tab.gameObject.SetActive(false);
                tab.transform.SetParent(this.transform);
            });

            tabs[0].gameObject.SetActive(true);
            setTransformDimensions();
        }

        /// <summary>
        /// Override to provide a specific implementation
        /// </summary>
        protected virtual void setTransformDimensions()
        {

        }

        /// <summary>
        /// Sets the selected tab to visible while disabling the previously selected
        /// </summary>
        /// <param name="tabIndex"></param>
        public void setActive(int tabIndex)
        {
            if (tabIndex == currentIndex || tabIndex == -1 || tabIndex >= tabs.Count)
            {
                return;
            }

            tabs[currentIndex].gameObject.SetActive(false);
            tabs[tabIndex].gameObject.SetActive(true);

            currentIndex = tabIndex;
        }
    }
}
