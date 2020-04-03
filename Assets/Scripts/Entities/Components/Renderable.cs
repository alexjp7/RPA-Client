/*---------------------------------------------------------------
                       RENDERABLE
 ---------------------------------------------------------------*/
/***************************************************************
 *  The Renderable type contains fields relating to any icons
    or sprite related data needed to render the relevant
    visual component to the player's UI.
**************************************************************/

using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.Entities.Components
{
    public class Renderable
    {
        public string spriteName { get; set; }
        public string spritePath { get; set; }

        private Sprite _sprite;
        public Sprite sprite
        {
            get
            {
                return _sprite == null ? _sprite = AssetLoader.getSprite(spriteName, spritePath) : _sprite;
            }
            private set
            {
                sprite = value;
            }
        }

    }

}
