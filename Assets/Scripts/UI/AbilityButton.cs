using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    public Image icon;
    public Button button;
    public Text cooldownText;

    /***************************************************************
    * instantiates and returns the an AbilityButton instance.

    @param - ability: The ability which will be visuallly
    represnted by this gameobject.

    @return - An AbilityButton reflecting that of the passed in
    ability.
    **************************************************************/
    public static AbilityButton create(in Ability ability)
    {
        Transform buttonTransfrom = Instantiate(GameAssets.INSTANCE.abilityButtonPrefab, Vector2.zero, Quaternion.identity);
        AbilityButton button = buttonTransfrom.GetComponent<AbilityButton>();
        button.setData(in ability);
        return button;
    }

    //Initialize Components
    void Awake()
    {
        button = gameObject.transform.Find("button").GetComponent<Button>();
        icon = button.GetComponent<Image>();
        cooldownText = gameObject.transform.Find("text_cooldown").GetComponent<Text>();
    }

    /***************************************************************
    * Sets the values of each GameObject that makes the AbilityButton.

    @param - ability: The ability which will be visuallly
    represnted by this gameobject.
    **************************************************************/
    private void setData(in Ability ability)
    {
        icon.sprite = ability == null ? AssetLoader.getSprite("lock") :ability.assetData.sprite;
        setEventHandlers(ability);
    }

    /***************************************************************
    * Creates the event handlers for hovering and click events
      for the ability buttons.
    **************************************************************/
    private void setEventHandlers(Ability ability)
    {
        EventTrigger.Entry mouseEnterEvent;
        EventTrigger.Entry mouseExitEvent;
        EventTrigger.Entry mouseClickeEvent;

        mouseEnterEvent = new EventTrigger.Entry();
        mouseExitEvent = new EventTrigger.Entry();
        mouseClickeEvent = new EventTrigger.Entry();

        mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
        mouseExitEvent.eventID = EventTriggerType.PointerExit;
        mouseClickeEvent.eventID = EventTriggerType.PointerClick;

        mouseEnterEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onAbilityHoverEnter(ability));
        mouseExitEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onAbilityHoverExit());
        mouseClickeEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onAbilityClicked(ability));

        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
    }
}
