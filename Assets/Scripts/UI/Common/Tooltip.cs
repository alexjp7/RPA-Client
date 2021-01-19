/*---------------------------------------------------------------
                        TOOLTIP
 ---------------------------------------------------------------*/
/***************************************************************
* UI Component for containerizing and displaying generic tooltip
  information.
**************************************************************/
namespace Assets.Scripts.UI.Common
{
    using UnityEngine;
    using UnityEngine.UI;

    public class Tooltip : MonoBehaviour
    {
        public static Tooltip instance;
        [SerializeField]
        public Camera uiCamera;

        private static float xOffSet;
        private static float yOffset;
        private static float transparency;

        private Text tooltipText;
        private RectTransform tooltipTransform;

        /// <summary>
        ///  Called on Scene initialization - assigns local variables to components in the unity object hierachy.
        /// </summary>
        private void Awake()
        {
            instance = this;
            tooltipTransform = transform.Find("panel_tooltip").GetComponent<RectTransform>();
            tooltipText = transform.Find("tooltip_text").GetComponent<Text>();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Updates the rect transform based on the mouse position. 
        /// </summary>
        /// <remarks>
        /// provides a local offset to stop mouse-hover jitter caused by tooltip overlapping the ability skill icon region and cancling the tooltip display.
        /// </remarks>
        private void Update()
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
            localPoint.x += xOffSet;
            localPoint.y += yOffset;
            transform.localPosition = localPoint;
        }

        /// <summary>
        /// Sets the tooltip text to passed in string value.
        /// </summary>
        /// <param name="tooltipString">the text wanting to be displayed in tooltip.</param>
        private void showTooltip(string tooltipString)
        {
            gameObject.SetActive(true);
            Color toolTipColor = tooltipTransform.GetComponent<Image>().color;
            toolTipColor.a = transparency;
            tooltipTransform.GetComponent<Image>().color = toolTipColor;

            tooltipText.text = tooltipString;
        }

        /// <summary>
        ///  Sets the active state of the tooltip to false, forcing it to be invisible to the user.
        /// </summary>
        private void hideTooltip()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        ///  public interface method for showing the tooltip.
        /// </summary>
        /// <param name="tooltipString">the text wanting to be displayed in tooltip.</param>
        /// <param name="_xOffset">default value of 50</param>
        /// <param name="_yOffset">default value of 50</param>
        /// <param name="_transparency">default value of 255</param>
        public static void show(string tooltipString, float _xOffset = 50, float _yOffset = 50, float _transparency = 255)
        {
            xOffSet = _xOffset;
            yOffset = _yOffset;
            transparency = _transparency;

            instance.showTooltip(tooltipString);
        }

        /// <summary>
        /// public interface method for hiding the tooltip.
        /// </summary>
        public static void hide()
        {
            instance.hideTooltip();
        }
    }
}