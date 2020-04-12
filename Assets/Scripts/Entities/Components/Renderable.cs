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
        public string name { get; set; }
        public string path { get; set; }

        private Sprite _sprite;
        //Loaded sprite from file, if not found returns null.
        public Sprite sprite
        {
            get
            {
                if(_sprite == null)
                {
                    _sprite = AssetLoader.getSprite(name, path);
                }

                return _sprite;
            }
            private set
            {
                sprite = value;
            }
        }

        public Renderable(string _name, string _path)
        {
            name = _name;
            path = _path;
        }

        public Renderable()
        {

        }
    }

}
