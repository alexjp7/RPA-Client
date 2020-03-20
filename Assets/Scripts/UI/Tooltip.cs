using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{

    public static Tooltip instance;

    [SerializeField]
    public Camera uiCamera;

    private Text tooltipText;
    private RectTransform tooltipTransform;

    private void Awake()
    {
        instance = this;
        tooltipTransform = transform.Find("panel_tooltip").GetComponent<RectTransform>();
        tooltipText = transform.Find("tooltip_text").GetComponent<Text>();
        gameObject.SetActive(false);
    }


    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), Input.mousePosition, uiCamera, out localPoint);
        localPoint.x += 50;
        localPoint.y += 50;
        transform.localPosition = localPoint;
    }
    private void showTooltip(string tooltipString)
    {
        gameObject.SetActive(true);
        tooltipText.text = tooltipString;
    }

    private void hideTooltip()
    {
        gameObject.SetActive(false);
    }

    public static void show(string tooltipString)
    {
        instance.showTooltip(tooltipString);
    }

    public static void hide()
    {
        instance.hideTooltip();
    }


}
