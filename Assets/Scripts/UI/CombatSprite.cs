/*---------------------------------------------------------------
                        COMBAT-SPRITE
 ---------------------------------------------------------------*/
/***************************************************************
* Combat Sprite contains all the UI components relevant
  for displaying player and monster sprites, hp and nameplates.

 * This script is attached to the CombaSprite prefab.
**************************************************************/
using Assets.Scripts.Entities.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CombatSprite : MonoBehaviour
    {
        public SpriteRenderer sprite { get; private set;}
        public Text displayName { get; private set; }
        public Text maxHealthValue { get; private set; }
        public Text currentHealthValue { get; private set; }
        public Image healthBar { get; set; }
        public bool isMonster { get; private set; }
        public bool isTargetable { get; set; }
        public bool hasValidTarget { get; set; }

        /***************************************************************
        * instantiates and returns the a CombatSpriute instance.
        @param - position: Set to Vector2.zero.
        @param - combatant: The monster or player that is to be displayed

        @return - A combat sprite reflecting that of the passed in
        combatant.
        **************************************************************/
        public static CombatSprite create(Vector2 position, in Combatable combatant)
        {
            Transform spriteTransform = Instantiate(GameAssets.instance.combatSpritePrefab, position, Quaternion.identity);
            CombatSprite cbSprite = spriteTransform.GetComponent<CombatSprite>();
            cbSprite.setData(combatant);
            return cbSprite;
        }

        /***************************************************************
        * Sets the values of each GameObject that makes up the CombatSprite.
        
        @param - combatant: The monster or player that is to be displayed

        @return - A combat sprite reflecting that of the passed in
        combatant.
        **************************************************************/
        private void setData(in Combatable combatant)
        {
            displayName.text = combatant.name;
            sprite.sprite = combatant.assetData.sprite;
            currentHealthValue.text =  ( (int) combatant.healthProperties.currentHealth).ToString();
            maxHealthValue.text = "/" +  (int )combatant.healthProperties.maxHealth;

            if (combatant.type == CombatantType.PLAYER) isMonster = false;
            else if (combatant.type == CombatantType.MONSTER) isMonster = true;
        }

        void Awake()
        {
            displayName = gameObject.transform.Find("text_player_name").GetComponent<Text>();
            sprite = gameObject.transform.Find("sprite_player").GetComponent<SpriteRenderer>();
            healthBar = gameObject.transform.Find("image_hp_bar").GetComponent<Image>();
            maxHealthValue = gameObject.transform.Find("text_max_health").GetComponent<Text>();
            currentHealthValue = gameObject.transform.Find("text_current_health").GetComponent<Text>();
        }

        private void onMouseHoverEnter()
        {
           
        }
    }
}  