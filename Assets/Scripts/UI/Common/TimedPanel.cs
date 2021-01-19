namespace Assets.Scripts.UI.Common
{
    using UnityEngine;
    using UnityEngine.UI;

    using Assets.Scripts.Util;

    /// <summary>
    /// Callback function to be called when timedpanel countdown expires.
    /// </summary>
    public delegate void TimerCallBack();

    /// <summary>
    /// Panel component which intrinsically destroys itself after the set time. After timeout, a callback can be optionally executed.
    /// </summary>
    public class TimedPanel : MonoBehaviour
    {
        private TimerCallBack callBack;
        private Text panelHeading;
        private Text paneldescription;
        private Text countdownText;

        public float countdownFrom { get; private set; }

        /// <summary>
        /// Instantiates a gameobject to have the timedpanel prefab.
        /// </summary>
        /// <param name="countdownFrom">value in seconds to countdown from, after which an optional callback can be invoked.</param>
        /// <param name="callBack">The callback method to execute prior to destrucntion</param>
        /// <param name="heading">Text value for panel heading to dispkay</param>
        /// <param name="description">Text value for the panel's description to display</param>
        /// <returns></returns>
        public static TimedPanel create(float countdownFrom, TimerCallBack callBack, string heading, string description)
        {
            Transform panelTransform = Instantiate(GameAssets.INSTANCE.timedPanelPrefab, Vector2.zero, Quaternion.identity);
            TimedPanel panel = panelTransform.GetComponent<TimedPanel>();
            panel.setData(countdownFrom, callBack, heading, description);
            return panel;
        }

        public void Awake()
        {
            panelHeading = transform.Find("text_heading").GetComponent<Text>();
            countdownText = transform.Find("text_countdown").GetComponent<Text>();
            paneldescription = transform.Find("text_description").GetComponent<Text>();
        }

        ///
        private void setData(float countdownFrom, TimerCallBack callBack, string heading, string description = "")
        {
            this.countdownFrom = countdownFrom;
            this.callBack = callBack;

            countdownText.text = countdownFrom.ToString();
            panelHeading.text = heading;
            paneldescription.text = description;
        }
        /// <summary>
        /// Incremently decreases the value of <see cref="countdownFrom"/> until it reaches 0. After the timedpanel is destroyed.
        /// </summary>
        public void Update()
        {
            countdownFrom -= Time.deltaTime;
            countdownText.text = ((int)countdownFrom).ToString();

            if (countdownFrom < 0)
            {
                callBack?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
