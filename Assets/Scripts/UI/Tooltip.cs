/*---------------------------------------------------------------
                        TOOLTIP
 ---------------------------------------------------------------*/
/***************************************************************
* UI Component for containerizing and displaying generic tooltip
  information.
**************************************************************/

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

    /***************************************************************
    * Called on Scene initialization - assigns local variables
      to components in the unity object hierachy.
    **************************************************************/
    private void Awake()
    {
        instance = this;
        tooltipTransform = transform.Find("panel_tooltip").GetComponent<RectTransform>();
        tooltipText = transform.Find("tooltip_text").GetComponent<Text>();
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
        localPoint.x += xOffSet;
        localPoint.y += yOffset;
        transform.localPosition = localPoint;
    }

    /***************************************************************
    * Sets the tooltip text to passed in string value.
    @parm - tooltipString: the text wanting to be displayed in tooltip.
    **************************************************************/
    private void showTooltip(string tooltipString)
    {
        gameObject.SetActive(true);
        Color toolTipColor = tooltipTransform.GetComponent<Image>().color;
        toolTipColor.a = transparency;
        tooltipTransform.GetComponent<Image>().color = toolTipColor;

        tooltipText.text = tooltipString;
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
    * public interface method for showing the tooltip.
    @parm - tooltipString: the text wanting to be displayed in tooltip.
    **************************************************************/
    public static void show(string tooltipString, float _xOffset = 50, float _yOffset = 50, float _transparency = 255)
    {
        xOffSet = _xOffset;
        yOffset = _yOffset;
        transparency = _transparency;

        instance.showTooltip(tooltipString);
    }

    /***************************************************************
    * public interface method for hiding the tooltip.
    **************************************************************/
    public static void hide()
    {
        instance.hideTooltip();
    }
}
