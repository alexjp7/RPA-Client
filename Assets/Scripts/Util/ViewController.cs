using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViewController : MonoBehaviour
{
    public void changeScene(int scene)
    {
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    public static void notify(string message)
    {
        GameObject newCanvas = new GameObject("canvas");
        Canvas c = newCanvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        newCanvas.AddComponent <CanvasScaler> ();
        GameObject panel = new GameObject("panel");

        panel.AddComponent<CanvasRenderer>();
        panel.AddComponent<RectTransform>();
        Image image = panel.AddComponent<Image>();
        Text text = panel.AddComponent<Text>();
        text.text = message;
        image.color = Color.red;

        panel.transform.SetParent(newCanvas.transform, false);
    }






}
