

namespace Assets.Scripts.UI
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using Assets.Scripts.Util;
    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.UI.Common;

    public class BuffBar : MonoBehaviour
    {

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
