
using Assets.Scripts.Player.Abilities;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Util
{
    public class AbilityTooltip : MonoBehaviour
    {
        public static AbilityTooltip instance;

        [SerializeField]
        public Camera uiCamera;

        private Text abilityName;
        private Text abilityType;
        private Text abilityCooldown;
        private Text abilityDescription;

        private RectTransform tooltipTransform;

        private void Awake()
        {
            instance = this;
            tooltipTransform = transform.Find("panel_ability_tooltip").GetComponent<RectTransform>();
            abilityName = transform.Find("label_name").GetComponent<Text>();
            abilityType = transform.Find("text_type").GetComponent<Text>();
            abilityCooldown = transform.Find("text_cooldown").GetComponent<Text>();
            abilityDescription = transform.Find("text_description").GetComponent<Text>();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
            localPoint.x += 150;
            localPoint.y += 150;
            transform.localPosition = localPoint;
        }
        private void showTooltip(in Ability ability)
        {
            abilityName.text = ability.name;
            abilityType.text = constructTypeString(ability.typeIds);
            abilityCooldown.text = constructCooldownText(ability.cooldown);
            abilityDescription.text = ability.tooltip;
            gameObject.SetActive(true);
        }

        private string constructCooldownText(int cooldown)
        {
            string result = cooldown.ToString();
            return result += cooldown > 1 ? " Turns" : " Turn";
        }

        //Helper method to create cumma delimited type string for tooltip
        private string constructTypeString(int[] ids)
        {
            string result = "";
            string[] typeStrings = AbilityFactory.getAbilityType(ids);
            for(int i = 0; i < typeStrings.Length; i++)
            {
                result += typeStrings[i] + " ";
            }
            return result;
        }

        private void hideTooltip()
        {
            gameObject.SetActive(false);
        }

        public static void show(in Ability ability)
        {
            instance.showTooltip(ability);
        }

        public static void hide()
        {
            instance.hideTooltip();
        }



    }
}
