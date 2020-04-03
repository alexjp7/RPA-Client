using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : MonoBehaviour
{
    private Text damageText;
    
    private float dissapearTime;
    private Color textColor;
    private float moveSpeed;
    
        public static DamagePopup create(Vector3 spritePosition, int damageDealt)
        {
            Vector2 localPoint = spritePosition;
            localPoint.y += 100;

            Transform damagePopupTransform = Instantiate(GameAssets.instance.damagePopupPrefab, localPoint, Quaternion.identity);
            DamagePopup damagePopup = damagePopupTransform.GetComponent<DamagePopup>();
            damagePopup.appear(damageDealt);
  
            return damagePopup;
        }
   
    private void Awake()
    {
        damageText = transform.GetComponent<Text>();
        moveSpeed = 30f;
        dissapearTime = 1f;
    }

    public void appear(int damageDealt)
    {
        damageText.text = damageDealt.ToString();
        textColor = damageText.color;
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
            damageText.color = textColor;

            if(textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }

    }
}
