/*---------------------------------------------------------------
                       RENDERABLE
 ---------------------------------------------------------------*/
/***************************************************************
 *  The Renderable type contains fields relating to any icons
    or sprite related data needed to render the relevant
    visual component to the player's UI.
**************************************************************/

using UnityEngine;

namespace Assets.Scripts.Entities.Components
{
    public class Renderable
    {
        public Texture2D icon { get; set; }
        public Sprite model { get; set; }
    }
}
