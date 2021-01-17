namespace Assets.Scripts.UI.Common
{
    using log4net;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    using Assets.Scripts.Util;
    using UnityEngine.UI;

    /// <summary>
    /// Provides a mechanism for the TabbedPanel to have a seperate UI component to control 
    /// the state of TabbedPanel
    /// </summary>
    class TabbedPanelControl : MonoBehaviour
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TabbedPanelControl));

        /// <summary>
        /// The tabbed panel of which will be controled
        /// </summary>
        public MultiTabPanel panel;

        /// <summary>
        /// Creates a new TabedPanelControl component that acts as the control unit for tabed panel.
        /// </summary>
        /// <param name="panel"></param>
        /// <returns></returns>
        public static void create(in MultiTabPanel panel, RectTransform parent)
        {
            GameObject gameObject = new GameObject("combat_panel_control");
            TabbedPanelControl control = gameObject.AddComponent<TabbedPanelControl>();

            control.setData(panel, parent);
        }

        private void setData(MultiTabPanel panel, RectTransform parent)
        {
            this.panel = panel;

            for (int i = 0; i < panel.count; i++)
            {
                int j = i;
                string tabName = panel[i].tabName;

                if (string.IsNullOrEmpty(tabName))
                {
                    ArgumentException exception = new ArgumentException($"Argument [tabName] is null or empty, this tab will not be displayed for {panel[i]}. Ensure a tab name is provided.");
                    log.Error(exception.Message);
                    throw exception;
                }

                Transform controlElement = Instantiate(GameAssets.INSTANCE.genericButtonPrefab, Vector2.zero, Quaternion.identity);


                Button button = controlElement.GetComponent<Button>();
                Text text = controlElement.transform.Find("text").GetComponent<Text>();
                button.transform.SetParent(parent);

                RectTransform rectTransform = button.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, 0);
                rectTransform.localScale = new Vector3(1, 1, 1);


                text.text = tabName;
                button.onClick.AddListener(() => onClick(j));
            }   
        }

        /// <summary>
        /// On click event listener 
        /// </summary>
        /// <param name="tabIndex"></param>
        public void onClick(int tabIndex)
        {
            Debug.Log(tabIndex);
            panel.setActive(tabIndex);
        }
    }
}
