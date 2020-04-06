/*---------------------------------------------------------------
                       TURN-CHEVRON
 ---------------------------------------------------------------*/
/***************************************************************
* The script attached to the turnchevron prefab,
  used to manipulate the turn indcator instance.
**************************************************************/

using Assets.Scripts.Util;
using UnityEngine;

public class TurnChevron : MonoBehaviour
{
    private static int turnCount = 0;
    private static SpriteRenderer chevronSprite;
    private static Transform instance;


    /***************************************************************
    * The position of the chevron.
    **************************************************************/
    private static Transform chevronTransform
    {
        get
        {
            if (instance == null) instance = Instantiate(GameAssets.INSTANCE.turnChevron, Vector3.zero, Quaternion.identity);
            return instance;
        }
    }

    void Awake()
    {
        chevronSprite = gameObject.GetComponent<SpriteRenderer>();
    }
    /***************************************************************
    * Updates the position of the chevron to reflect the next
     combatants turn.

     @param  - position: The new position that the chevron
     transform will be updated to.
    **************************************************************/
    public static void setPosition(Transform position)
    {
        turnCount++;
        Vector3 localPoint = position.position;
        localPoint.y += 100;
        chevronTransform.SetParent(position.transform);
        chevronTransform.position = localPoint;

        if(turnCount % 2 == 0)
        {
            chevronSprite.color = Color.red;
        }
        else
        {
            chevronSprite.color = Color.green;
        }

    }
}
