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

        public static CombatSprite create(Vector2 position, in Combatable combatant)
        {
            Transform spriteTransform = Instantiate(GameAssets.instance.combatSpritePrefab, position, Quaternion.identity);
            CombatSprite cbSprite = spriteTransform.GetComponent<CombatSprite>();
            cbSprite.setData(combatant);
            return cbSprite;
        }

        private void setData(in Combatable combatant)
        {
            displayName.text = combatant.name;
            sprite.sprite = combatant.assetData.model;
            currentHealthValue.text =  ( (int) combatant.healthProperties.currentHealth).ToString();
            maxHealthValue.text = "/" +  (int )combatant.healthProperties.maxHealth;
        }

        void Awake()
        {
            displayName = gameObject.transform.Find("text_player_name").GetComponent<Text>();
            sprite = gameObject.transform.Find("sprite_player").GetComponent<SpriteRenderer>();
            healthBar = gameObject.transform.Find("image_hp_bar").GetComponent<Image>();
            maxHealthValue = gameObject.transform.Find("text_max_health").GetComponent<Text>();
            currentHealthValue = gameObject.transform.Find("text_current_health").GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}  