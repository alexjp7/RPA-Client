using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.UI;

public class FloatingPopup : MonoBehaviour
{
    private static int popupCount = 0;
    private Text popupText;
    private float dissapearTime;
    private Color textColor;
    private float moveSpeed;

        public static FloatingPopup create(Vector3 spritePosition, string text, in Color color)
        {
            Vector2 localPoint = spritePosition;
            localPoint.y += 100 + (popupCount * 25);

            Transform damagePopupTransform = Instantiate(GameAssets.INSTANCE.damagePopupPrefab, localPoint, Quaternion.identity);
            FloatingPopup popup = damagePopupTransform.GetComponent<FloatingPopup>();
            popup.appear(text, in color);
  
            return popup;
        }
   
    private void Awake()
    {
        popupText = transform.GetComponent<Text>();
        moveSpeed = 30f;
        dissapearTime = 1f;
    }

    public void appear(string text, in Color color)
    {
        popupCount++;
        popupText.color = color;
        popupText.text = text;
        textColor = color;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0, moveSpeed) * Time.deltaTime;
        dissapearTime -= Time.deltaTime;
        if(dissapearTime < 0)
        {   
            float dissapearSpeed = 3f;
            textColor.a -= dissapearSpeed * Time.deltaTime;
            popupText.color = textColor;

            if(textColor.a < 0)
            {
                Destroy(gameObject);
                popupCount--;
            }
        }

    }
}
