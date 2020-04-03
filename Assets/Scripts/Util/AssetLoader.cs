/*---------------------------------------------------------------
                    ASSET-LOADER
 ---------------------------------------------------------------*/
/***************************************************************
* The Assetloader class provides the functionality for 
  loading and storing re-usable assets/sprite models.
**************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public class AssetLoader
    {
        private static Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();


        public static void loadStaticAssets()
        {
            Dictionary<string, string> staticAssets = new Dictionary<string, string>()
            {
                {"lock", "textures/icon_textures/ability_icons/lock" }
            };

            foreach (var asset in staticAssets) spriteMap.Add(asset.Key, Resources.Load<Sprite>(asset.Value));
        }

        /***************************************************************
        @param - key: The asset to be loaded/returned
        @paran - path: the location on disk where that asset is

        @return - The sprite model for the key passed in.
        **************************************************************/
        public static Sprite getSprite(string key, string path = "")
        {
            if (!spriteMap.ContainsKey(key)) spriteMap.Add(key, Resources.Load<Sprite>(path));

            return spriteMap[key];
        }

        /***************************************************************
         @param - key: The asset to be loaded/returned
         @paran - path: the location on disk where that asset is

        @return - The sprite model for the key passed in.
        **************************************************************/
        public static void destroyAsset(string key)
        {
            if (spriteMap.ContainsKey(key)) spriteMap.Remove(key);
        }
    } 
}
