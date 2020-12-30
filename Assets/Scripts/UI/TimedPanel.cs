

namespace Assets.Scripts.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.UI;
    using Assets.Scripts.Util;

    public delegate void TimerCallBack();

    public class TimedPanel : MonoBehaviour
    {
        private TimerCallBack callBack;
        private Text panelHeading;
        private Text paneldescription;
        private Text countdownText;

        public float countdownFrom { get; private set; }

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

        private void setData(float countdownFrom, TimerCallBack callBack ,string heading, string description = "")
        {
            this.countdownFrom = countdownFrom;
            this.callBack = callBack;

            countdownText.text =  countdownFrom.ToString();
            panelHeading.text = heading;
            paneldescription.text = description;
        }

        public void Update()
        {
            countdownFrom -= Time.deltaTime;
            countdownText.text = ((int)countdownFrom).ToString();

            if (countdownFrom < 0)
            {
                callBack();
                Destroy(gameObject);
            }
        }
    }
}
