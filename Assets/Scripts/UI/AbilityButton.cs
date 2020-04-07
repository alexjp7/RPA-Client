using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    public static readonly KeyCode[] keyCodes = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y};
    private static int keyIndex = 0;

    public int buttonCount;
    public bool isPressed;
    public Text keyText;
    public Button button { get; private set; }
    public Text cooldownText { get; private set; }
    public Image icon { get; set; }
    private Ability abilityRef;

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
        keyIndex++;
        return button;
    }

    //Initialize Components
    void Awake()
    {
        isPressed = false;
        button = gameObject.transform.Find("button").GetComponent<Button>();
        keyText = gameObject.transform.Find("text_key").GetComponent<Text>();
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
        buttonCount = keyIndex;
        keyText.text = keyCodes[buttonCount].ToString();
        abilityRef = ability;
        icon.sprite = ability == null ? AssetLoader.getSprite("lock") :ability.assetData.sprite;
        setEventHandlers();
    }

    /***************************************************************
    * Creates the event handlers for hovering and click events
      for the ability buttons.
    **************************************************************/
    private void setEventHandlers()
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

        mouseEnterEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onAbilityHoverEnter(abilityRef));
        mouseExitEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onAbilityHoverExit());
        mouseClickeEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onAbilityClicked(abilityRef));

        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
    }

    private void OnDestroy()
    {
        keyIndex--;
    }

    void Update()
    {
        if(Input.GetKeyDown(keyCodes[buttonCount]))
        {
            ViewController.INSTANCE.battleState.onAbilityClicked(abilityRef);
        }
    }
}
