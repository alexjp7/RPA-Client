/*---------------------------------------------------------------
                       TURN-CHEVRON
 ---------------------------------------------------------------*/
/***************************************************************
* The script attached to the turnchevron prefab,
  used to manipulate the turn indcator instance.
**************************************************************/

using UnityEngine;

public class TurnChevron : MonoBehaviour
{
    private SpriteRenderer chevronSprite;
    private static Transform instance;

    /***************************************************************
    * The position of the chevron.
    **************************************************************/
    private static Transform chevronTransform
    {
        get
        {
            if (instance == null) instance = Instantiate(GameAssets.instance.turnChevron, Vector3.zero, Quaternion.identity);
            return instance;
        }
    }

    /***************************************************************
    * Updates the position of the chevron to reflect the next
     combatants turn.

     @param  - position: The new position that the chevron
     transform will be updated to.
    **************************************************************/
    public static void setPosition(Vector3 position)
    {
        Vector3 localPoint = position;
        localPoint.y += 100;
        chevronTransform.position = localPoint;
    }
}
