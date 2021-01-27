
namespace Assets.Scripts.UI.Common
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Containers;
    using Assets.Scripts.GameStates;
    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.Util;
    using log4net;
    using Assets.Scripts.UI.Combat;

    /// <summary>
    /// UI component for individual Ability Buttons.  Ability buttons provide the visual elements of an <see cref="Ability"/>.
    /// Abilitybuttons are loaded into the combat UI as apart of the <see cref="CombatPanel"/>.
    /// </summary>
    public class AbilityButton : MonoBehaviour
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AbilityButton));

        /// <summary>
        /// Letter codees for alphabetic keystrokes used for keybinding.
        /// </summary>
        public static readonly KeyCode[] keyCodes = { KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T};

        /// <summary>
        /// Index of the selected ability
        /// </summary>
        public static int selectedAbilityIndex { get; set; }
        
        public int buttonCount;

        private static int keyIndex = 0;
        private static bool isStandardTooltip = false;

        //Componenet Fields
        public static Color selectedColor = Color.yellow;
        public static Color unselectedColor = Color.white;

        public Image imageBkg;
        public Text keyText;
        public Button button { get; private set; }
        public Text cooldownText { get; private set; }
        public Image icon { get; set; }
        private Ability abilityRef;

        private static CombatController turnController => StateManager.battleState.combatController;

        /// <summary>
        ///  Instantiates and returns the an AbilityButton instance.
        /// </summary>
        /// <param name="ability"> ability: The ability which will be visuallly represnted by this gameobject.</param>
        /// <returns>  An AbilityButton reflecting that of the passed in ability.</returns>
        public static AbilityButton create(in Ability ability)
        {
            Transform buttonTransfrom = Instantiate(GameAssets.INSTANCE.abilityButtonPrefab, Vector2.zero, Quaternion.identity);
            AbilityButton button = buttonTransfrom.GetComponent<AbilityButton>();
            button.setData(ability);
            keyIndex++;

            return button;
        }

        /// <summary>
        /// Initialize  game object components
        /// </summary>
        void Awake()
        {
            button = gameObject.transform.Find("button").GetComponent<Button>();
            keyText = gameObject.transform.Find("text_key").GetComponent<Text>();
            icon = button.GetComponent<Image>();
            cooldownText = gameObject.transform.Find("text_cooldown").GetComponent<Text>();
            imageBkg = gameObject.GetComponent<Image>();
            cooldownText.color = Color.yellow;
        }


        /// <summary>
        /// Sets the values of each GameObject that makes the AbilityButton.
        /// </summary>
        /// <param name="ability">The ability which will be  represnted by this gameobject.</param>
        private void setData(Ability ability)
        {
            buttonCount = keyIndex;
            keyText.text = keyCodes[buttonCount].ToString();
            abilityRef = ability;
            icon.sprite = ability == null ? AssetLoader.getSprite("lock") : ability.assetData.sprite;
            setEventHandlers();
        }

        /// <summary>
        /// Creates the event handlers for hovering and click events for the ability buttons.
        /// </summary>
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
        /// <summary>
        /// On mouse hover over ability icons, the tooltip for that ability is displayed.
        /// </summary>
        /// <remarks>
        /// if an ability is locked or not loaded, the called function will throw an ArgumentOutOfRangeException which indicates that a standard tooltip should be displayed.
        /// </remarks>
        /// <param name="ability">a value between 0-4 indicating which ability from the ability bar is being hovered over.</param>
        public void onAbilityHoverEnter(in Ability ability)
        {
            isStandardTooltip = ability == null;
            if (isStandardTooltip) Tooltip.show("Locked");
            else AbilityTooltip.show(ability);
        }

        /// <summary>
        ///     * Event handler for hiding the ability tooltips upon mouse exit.
        /// </summary>
        public void onAbilityHoverExit()
        {
            if (isStandardTooltip) Tooltip.hide();
            else AbilityTooltip.hide();
        }

        /// <summary>
        /// Event handler for click action in ability icon.
        /// <para>
        /// Determines the ability type of the ability that was clicked, and provides appropriate operations 
        /// for populating the targets dictionary or otherwise displaying 
        /// targeting information through setting the tint of targeteable sprites.
        /// </para>
        /// </summary>
        /// <param name="abilitySelection">The index of the ability that was clicked</param>
        public void onAbilityClicked(int abilitySelection)
        {
            if (abilitySelection >= Game.clientSidePlayer.playerClass.abilities.Count && abilitySelection != -1) return;
            if (Game.clientSidePlayer.playerClass.abilities[abilitySelection] == null) return;
            if (!turnController.isClientPlayerTurn) return;
            if (Game.clientSidePlayer.playerClass.abilities[abilitySelection].isOnCooldown) return;

            AbilityButtons.setSelected(abilitySelection);

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
                    .ForEach(monster =>
                    {
                        monster.combatSprite.sprite.color = Color.red;
                        turnController.targets.Add(monster);
                    });
                    break;

                case AbilityTypes.MULTI_HEAL:
                case AbilityTypes.MULTI_BUFF:
                    turnController.playerParty.asList() //Update Player sprite colors
                    .FindAll(player => player.isAlive())
                    .ForEach(player =>
                    {
                        player.combatSprite.sprite.color = Color.green;
                        turnController.targets.Add(player);
                    });
                    break;
            }
        }

        /// <summary>
        /// Adds selection ring around the selected ability.
        /// </summary>
        /// <param name="isSelected"></param>
        public void setSelected(bool isSelected)
        {
            if (isSelected)
            {
                imageBkg.color = selectedColor;
            }
            else
            {
                imageBkg.color = unselectedColor;
            }
        }

        private void OnDestroy()
        {
            keyIndex--;
        }

        void Update()
        {
            if (Input.GetKeyDown(keyCodes[buttonCount]))
            {
                onAbilityClicked(buttonCount);
            }
        }
    }
}