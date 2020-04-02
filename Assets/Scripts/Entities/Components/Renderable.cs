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
        public string iconPath { get; set;}


        public Texture2D icon { get; set; }

        //if Model is not set, load it.
        public Sprite model
        {
            get
            {
                return AssetLoader.getAsset(spriteName, spritePath);
            }
        }


    }

}
