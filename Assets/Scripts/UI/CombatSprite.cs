using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CombatSprite:  MonoBehaviour
    {
        //Party textures/text
        public GameObject panel { get; set; }
        public SpriteRenderer sprite { get; set; }
        public Text nameText { get; set; }
        public Text maxHealthValue { get; set; }
        public Text currentHealthValue { get; set; }
        public Image hpBar { get; set; }
        public GameObject turnIndicator { get; set; }
    }
}
