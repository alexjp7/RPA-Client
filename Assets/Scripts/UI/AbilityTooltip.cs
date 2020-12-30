/*---------------------------------------------------------------
                         ABILITY-TOOLTIP
 ---------------------------------------------------------------*/
/***************************************************************
* UI Component for containerizing and displaying ability tooltip
  information on mouse-hover.
**************************************************************/

using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Entities.Abilities;

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

        /***************************************************************
        * Called on Scene initialization - assigns local variables
          to components in the unity object hierachy.
        **************************************************************/
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

        /***************************************************************
        * Updates the rect transform based on the mouse position.
        * provides a local offset to stop mouse-hover jitter caused
          by tooltip overlapping the ability skill icon region and 
          cancling the tooltip display.
        **************************************************************/
        private void Update()
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
            localPoint.x += 220;
            localPoint.y += 220;
            transform.localPosition = localPoint;
        }

        /***************************************************************
        * Sets the ability fields to the tooltips text fields.
        @in-parm - ability: The ability icon hovered over by player.
        **************************************************************/
        private void showTooltip(in Ability ability)
        {
            abilityName.text = ability.name;
            abilityType.text = constructTypeString(ability.typeIds);
            abilityCooldown.text = constructCooldownText(ability.cooldown);
            abilityDescription.text = ability.tooltip;
            gameObject.SetActive(true);
        }

        /***************************************************************
        * Helper method to construct a more user friendly cooldown
          description
        @param - cooldown: The value for the amount of turns an ability
        takes to recover before it is usable again.
        **************************************************************/
        private string constructCooldownText(int cooldown)
        {
            string result = cooldown.ToString();
            return result += cooldown > 1 ? " Turns" : " Turn";
        }

        /***************************************************************
        * Helper method to construct a comma delimited ability type string
          to include in the tooltip text field.

        @param - ids: The list of ability type IDs that an ability has.
        **************************************************************/
        private string constructTypeString(int[] ids)
        {
            string result = "";
            string[] typeStrings = AbilityFactory.getAbilityTypeLabel(ids);
            for(int i = 0; i < typeStrings.Length; i++)
            {
                result += typeStrings[i] + " ";
            }
            return result;
        }

        /***************************************************************
        * Sets the active state of the tooltip to false, forcing it
          to be invisible to the user.
        **************************************************************/
        private void hideTooltip()
        {
            gameObject.SetActive(false);
        }

        /***************************************************************
        * public interface method for showing the ability tooltip .
        @in-parm - ability: The ability icon hovered over by player.
        **************************************************************/
        public static void show(in Ability ability)
        {
            instance.showTooltip(ability);
        }

        /***************************************************************
        * public interface method for hiding the ability tooltip.
        **************************************************************/
        public static void hide()
        {
            instance.hideTooltip();
        }
    }
}
