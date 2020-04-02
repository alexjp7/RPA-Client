using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _instance;

    public static GameAssets instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Instantiate(Resources.Load<GameAssets>("prefabs/GameAssets"));
            }

            return _instance;
        }
    }

    public Transform damagePopupPrefab;
    public Transform turnChevron;
    public Transform combatSpritePrefab;
}
