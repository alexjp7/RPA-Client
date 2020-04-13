/*---------------------------------------------------------------
                    ASSET-LOADER
 ---------------------------------------------------------------*/
/***************************************************************
* The Assetloader class provides the functionality for 
  loading and storing re-usable assets/sprite models.
**************************************************************/

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace Assets.Scripts.Util
{
    public class AssetLoader
    {
        private static Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();


        public static void loadStaticAssets(GameState gameState)
        {
            Dictionary<string, string> staticAssets = new Dictionary<string, string>()
            {
                {"lock", "textures/icon_textures/general/lock" }
            };

            switch (gameState)
            {
                case GameState.MAIN_MENU:

                    break;

                case GameState.CHARACTER_CREATION:

                    break;

                case GameState.BATTLE_STATE:
                    loadBattleStateAssets();
                    break;

                default:
                    Debug.LogError($"Unexpecte Gamestate argument {gameState.ToString()} provided during asset loading.  {new System.Diagnostics.StackFrame().ToString()}");
                    break;

            }

            foreach (var asset in staticAssets)
            {
                if(!spriteMap.ContainsKey(asset.Key))
                {
                    spriteMap.Add(asset.Key, Resources.Load<Sprite>(asset.Value));
                }
            }
        }

        private static void loadBattleStateAssets()
        {
            Sprite[] statusEffectIcons = Resources.LoadAll<Sprite>("textures/icon_textures/status_effects");

            foreach(Sprite iconSprite in statusEffectIcons)
            {
                if (!spriteMap.ContainsKey(iconSprite.name))
                {
                    spriteMap.Add(iconSprite.name, iconSprite);
                }
            }
        }


        /***************************************************************
        @param - key: The asset to be loaded/returned
        @paran - path: the location on disk where that asset is

        @return - The sprite model for the key passed in.
        **************************************************************/
        public static Sprite getSprite(string key, string path = "")
        {
            if (!spriteMap.ContainsKey(key))
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if(sprite == null)
                {
                    Debug.LogError($"AssetNotFound- Loading asset '{key}' at path 'Assets/Resources/{path}' failed. Chcek in Assets/Resources directory for missing or missmatched asset path {new System.Diagnostics.StackFrame().ToString()}") ;
                }

                spriteMap.Add(key, sprite);
            }

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
