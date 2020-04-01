using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnChevron : MonoBehaviour
{
    private SpriteRenderer chevronSprite;
    private static Transform instance;
    private static Transform chevronTransform
    {
        get
        {
            if (instance == null) instance = Instantiate(GameAssets.instance.turnChevron, Vector3.zero, Quaternion.identity);
            return instance;
        }
    }

    public static void setPosition(Vector3 position)
    {
        Vector3 localPoint = position;
        localPoint.y += 100;
        chevronTransform.position = localPoint;
    }
}
