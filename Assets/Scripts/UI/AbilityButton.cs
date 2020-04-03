using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    public Image icon;
    public Button button;
    public Text cooldownText;
    public int abilityIndex = 0;
    public static int index = 0;

    public static AbilityButton create(in Ability ability)
    {
        Transform buttonTransfrom = Instantiate(GameAssets.instance.abilityButtonPrefab, Vector2.zero, Quaternion.identity);
        AbilityButton button = buttonTransfrom.GetComponent<AbilityButton>();
        button.setData(in ability);
        index++;
        return button;
    }

    private void setData(in Ability ability)
    {
        if(ability != null)
        {
            icon.sprite = ability.assetData.sprite;
            abilityIndex = index;
        }
        else
        {
            icon.sprite = AssetLoader.getSprite("lock");
        }
    }

    void Awake()
    {
        button = gameObject.transform.Find("button").GetComponent<Button>();
        icon = button.GetComponent<Image>();
        cooldownText = gameObject.transform.Find("text_cooldown").GetComponent<Text>();
    }
}
