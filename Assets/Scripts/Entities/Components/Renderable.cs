/*---------------------------------------------------------------
                       RENDERABLE
 ---------------------------------------------------------------*/
/***************************************************************
 *  The Renderable type contains fields relating to any icons
    or sprite related data needed to render the relevant
    visual component to the player's UI.
**************************************************************/

namespace Assets.Scripts.Entities.Components
{
    using Assets.Scripts.Util;
    using UnityEngine;

    public class Renderable
    {
        /// <summary>
        /// The name of the asset that is used as the key in a key-value map that uniquely identifies an asset.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The sprite's location on disk (should be within solution project structure).
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// The underlying Unity <see cref="UnityEngine.Sprite"/> object. Accessing this member will instantiate the sprite if the instance
        /// holder is <c>null</c>.
        /// </summary>
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

        /// <summary>
        /// Instance holder for sprite
        /// </summary>
        private Sprite _sprite;

        /// <summary>
        /// Sets the renderable component's path and name.
        /// </summary>
        /// <param name="name">The name of the asset that is used as the key in a key-value map that uniquely identifies an asset. </param>
        /// <param name="path">The sprite's location on disk (should be within solution project structure).</param>
        public Renderable(string name, string path)
        {
            this.name = name;
            this.path = path;
        }

        public Renderable()
        {

        }
    }

}
