namespace Assets.Scripts.UI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using Assets.Scripts.Util;
    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.UI.Common;

    /// <summary>
    /// UI component that represents the status effects that a combatant is being affected by.
    /// The Ui component consists of a list of status effect icons, which have an on-hover tooltip.
    /// </summary>
    public class BuffBar : MonoBehaviour
    {
        /// <summary>
        /// Populates a combatant's buff bar with condition icons matching the conditions in its condition list.
        /// </summary>
        /// <param name="conditions">The list of conditions that will be populated onto the buffbar of the selected combatant.</param>
        public void updateConditions(in Dictionary<int, Condition> conditions)
        {
            int childs = transform.childCount;
            for (var i = childs - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            if (conditions.Count > 0)
            {
                foreach (var condition in conditions)
                {
                    int conditionId = condition.Key;
                    string conditionName = ((StatusEffect)conditionId).ToString();
                    GameObject newCondition = new GameObject(conditionName);
                    Image conditionIcon = newCondition.AddComponent<Image>();
                    conditionIcon.sprite = AssetLoader.getSprite(conditionName, null, true);
                    setEventHandlers(newCondition, conditionId, condition.Value.potency, condition.Value.turnsRemaining);
                    newCondition.gameObject.transform.SetParent(transform);
                }
            }
        }

        /// <summary>
        /// Sets the mouse enter/exit event listeners for each condition icon when its created.
        /// </summary>
        /// <param name="newCondition">Game object which contains the condition icon</param>
        /// <param name="conditionId">The effect ID as denoted in <see cref="EffectProcessor>"/></param>
        /// <param name="conditionPotency">The potency of the effect</param>
        /// <param name="turnsRemaining">How many turns remaining the status effect will be applied for.</param>
        private void setEventHandlers(GameObject newCondition, int conditionId, int conditionPotency, int turnsRemaining)
        {
            EventTrigger.Entry mouseEnterEvent;
            EventTrigger.Entry mouseExitEvent;

            //Initialize Party Sprite Events
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;

            mouseEnterEvent.callback.AddListener(evt => Tooltip.show(EffectProcessor.getEffectLabel(conditionId, conditionPotency), -50, -50, 0.9f));
            mouseExitEvent.callback.AddListener(evt => Tooltip.hide());

            newCondition.AddComponent<EventTrigger>();

            newCondition.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            newCondition.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
        }
    }
}
