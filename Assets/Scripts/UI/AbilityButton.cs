using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.GameStates;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    public static readonly KeyCode[] keyCodes = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y };
    public static int selectedAbilityIndex { get; set; }
    private static int keyIndex = 0;
    private static bool isStandardTooltip = false;

    public int buttonCount;

    //Componenet Fields
    public Text keyText;
    public Button button { get; private set; }
    public Text cooldownText { get; private set; }
    public Image icon { get; set; }
    private Ability abilityRef;

    private static TurnController turnController => StateManager.battleState.turnController;

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
        button.setData(ability);
        keyIndex++;

        return button;
    }

    //Initialize Components
    void Awake()
    {
        button = gameObject.transform.Find("button").GetComponent<Button>();
        keyText = gameObject.transform.Find("text_key").GetComponent<Text>();
        icon = button.GetComponent<Image>();
        cooldownText = gameObject.transform.Find("text_cooldown").GetComponent<Text>();
        cooldownText.color = Color.yellow;
    }

    /***************************************************************
    * Sets the values of each GameObject that makes the AbilityButton.

    @param - ability: The ability which will be visuallly
    represnted by this gameobject.
    **************************************************************/
    private void setData(Ability ability)
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

        mouseEnterEvent.callback.AddListener(evt => onAbilityHoverEnter(abilityRef));
        mouseExitEvent.callback.AddListener(evt => onAbilityHoverExit());
        mouseClickeEvent.callback.AddListener(evt => onAbilityClicked(buttonCount));

        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
        gameObject.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
    }

    /*---------------------------------------------------------------
                        CLIENT SIDE EVENT-HANDLERS
     ---------------------------------------------------------------*/
    /***************************************************************
                        ABILITY-HANDLERS
    *************************************************************** 
    * On mouse hover over ability icons, the tooltip for that ability
      is displayed.

    * if an ability is locked or not loaded, the called function 
      will throw an ArgumentOutOfRangeException which indicates
      that a standard tooltip should be displayed.

    @param - skillIndex: a value between 0-4 indicating which
    ability from the ability bar is being hovered over.
    **************************************************************/
    public void onAbilityHoverEnter(in Ability ability)
    {
        isStandardTooltip = ability == null;
        if (isStandardTooltip) Tooltip.show("Locked");
        else AbilityTooltip.show(ability);
    }

    /***************************************************************
    * Event handler for hiding the ability tooltips upon mouse exit.
    **************************************************************/
    public void onAbilityHoverExit()
    {
        if (isStandardTooltip) Tooltip.hide();
        else AbilityTooltip.hide();
    }

    /***************************************************************
    * Event handler for click action in ability icon.
    
    * Determines the ability type of the ability that was clicked
      and provides appropriate operations for populating the targets
      dictionary or otherwise displaying targeting information
      through setting the tint of targeteable sprites.
          
    @param - ability: The ability that was clicked
    **************************************************************/
    public void onAbilityClicked(int abilitySelection)
    {
        if (Game.clientSidePlayer.playerClass.abilities[abilitySelection] == null) return;
        if (!turnController.isClientPlayerTurn) return; 
        if (Game.clientSidePlayer.playerClass.abilities[abilitySelection].isOnCooldown) return;

        //Reset TargetsS
        turnController.resetTargets();
        selectedAbilityIndex = abilitySelection;

        switch ((AbilityTypes)Game.clientSidePlayer.playerClass.abilities[abilitySelection].typeIds[0])
        {
            //NOTE: Single Target abilities defer targeting to manual combatant selection
            //Self Target
            case AbilityTypes.SELF_HEAL:
            case AbilityTypes.SELF_BUFF:
                turnController.targets.Add(Game.clientSidePlayer.playerClass);
                Game.clientSidePlayer.playerClass.combatSprite.sprite.color = Color.green;
                break;

            //Multi Target 
            case AbilityTypes.MULTI_DAMAGE:
            case AbilityTypes.MULTI_DEBUFF:
                turnController.monsterParty.asList()//Update monster sprite colors 
                .FindAll(monster => monster.isAlive())
                .ForEach(monster => {
                    monster.combatSprite.sprite.color = Color.red;
                    turnController.targets.Add(monster);
                });
                break;

            case AbilityTypes.MULTI_HEAL:
            case AbilityTypes.MULTI_BUFF:
                turnController.playerParty.asList() //Update Player sprite colors
                .FindAll(player => player.isAlive())
                .ForEach(player => {
                    player.combatSprite.sprite.color = Color.green;
                    turnController.targets.Add(player);
                });
                break;
        }
    }


    private void OnDestroy()
    {
        keyIndex--;
    }

    void Update()
    {
        if(Input.GetKeyDown(keyCodes[buttonCount]))
        {
            onAbilityClicked(buttonCount);
        }
    }
}
