/*---------------------------------------------------------------
                         ABILITY-TOOLTIP
 ---------------------------------------------------------------*/
/***************************************************************
* UI Component for containerizing and displaying ability tooltip
information on mouse-hover.
**************************************************************/
namespace Assets.Scripts.Util
{
    using UnityEngine;
    using UnityEngine.UI;

    using Assets.Scripts.Entities.Combat;

    /// <summary>
    /// Tooltip that is shown when hovering over an ability button.
    /// </summary>
    public class AbilityTooltip : MonoBehaviour
    {
        public static AbilityTooltip instance;

        [SerializeField] public Camera uiCamera;

        private Text abilityName;
        private Text abilityType;
        private Text abilityCooldown;
        private Text abilityDescription;

        private RectTransform tooltipTransform;

        /// <summary>
        ///  Called on Scene initialization - assigns local variables to components in the unity object hierachy.
        /// </summary>
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

        /// <summary>
        /// Updates the rect transform based on the mouse position. Provides a local offset to stop mouse-hover
        /// jitter caused by tooltip overlapping the ability skill icon region and cancling the tooltip display.
        /// </summary>
        private void Update()
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
            localPoint.x += 220;
            localPoint.y += 220;
            transform.localPosition = localPoint;
        }

        /// <summary>
        /// Sets the ability fields to the tooltips text fields
        /// </summary>
        /// <param name="ability">ability: The ability icon hovered over by player.</param>
        private void showTooltip(in Ability ability)
        {
            abilityName.text = ability.name;
            abilityType.text = constructTypeString(ability.typeIds);
            abilityCooldown.text = constructCooldownText(ability.cooldown);
            abilityDescription.text = ability.tooltip;
            gameObject.SetActive(true);
        }

        /// <summary>
        ///  Helper method to construct a more user friendly cooldown description
        /// </summary>
        /// <param name="cooldown">The value for the amount of turns an ability takes to recover before it is usable again.</param>
        /// <returns>The completed string of "[x] turn/s"</returns>
        private string constructCooldownText(int cooldown)
        {
            string result = cooldown.ToString();
            return result += cooldown > 1 ? " Turns" : " Turn";
        }

        /// <summary>
        /// Helper method to construct a comma delimited ability type string to include in the tooltip text field.
        /// </summary>
        /// <param name="ids"> The list of ability type IDs that an ability has.</param>
        /// <returns>Concatented ability type labels</returns>
        private string constructTypeString(int[] ids)
        {
            string result = "";
            string[] typeStrings = AbilityUtils.getAbilityTypeLabel(ids);
            for(int i = 0; i < typeStrings.Length; i++)
            {
                result += typeStrings[i] + " ";
            }
            return result;
        }

        /// <summary>
        ///  Sets the active state of the tooltip to false, forcing it  to be invisible to the user.
        /// </summary>
        private void hideTooltip()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// public interface method for showing the ability tooltip.
        /// </summary>
        /// <param name="ability">ability: The ability icon hovered over by player.</param>
        public static void show(in Ability ability)
        {
            instance.showTooltip(ability);
        }

        /// <summary>
        /// public interface method for hiding the ability tooltip.
        /// </summary>
        public static void hide()
        {
            instance.hideTooltip();
        }
    }
}
