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
        private static bool isInitialLoad = true;
        private static GameState activeState;

        public static void loadStaticAssets(GameState gameState)
        {
            if (gameState == activeState) return;
            dumpPreviousResources();
            activeState = gameState;
            loadGeneralAssets();

            switch (gameState)
            {
                case GameState.MAIN_MENU:

                    break;

                case GameState.CHARACTER_CREATION:
                    loadCharacterCreationAssets();
                    break;

                case GameState.BATTLE_STATE:
                    loadBattleStateAssets();
                    break;

                default:
                    Debug.LogError($"Unexpecte Gamestate argument {gameState.ToString()} provided during asset loading.  {new System.Diagnostics.StackFrame().ToString()}");
                    break;
            }
        }

        /***************************************************************
        * Removes unused assets from the sprite map as the game
          state changes
        **************************************************************/
        private static void dumpPreviousResources()
        {
            List<string> assetsRemoving = new List<string>();

            foreach(var asset in spriteMap)
            {
                if(asset.Key.StartsWith(activeState.ToString()))
                {
                    assetsRemoving.Add(asset.Key);
                }
            }

            foreach(var asset in assetsRemoving )
            {
                spriteMap.Remove(asset);
            }
        }

        /***************************************************************
        * Loads general assets used over multiple states
        **************************************************************/
        private static void loadGeneralAssets()
        {
            if (isInitialLoad)
            {
                Sprite[] generalAssets = Resources.LoadAll<Sprite>("textures/icon_textures/general");

                foreach (Sprite asset in generalAssets)
                {
                    if (!spriteMap.ContainsKey(asset.name))
                    {
                        spriteMap.Add(asset.name, asset);
                    }
                }
            }
        }

        /***************************************************************
        * Loads character creation assets
        **************************************************************/
        private static void loadCharacterCreationAssets()
        {
            Sprite[] classIcons = Resources.LoadAll<Sprite>("textures/icon_textures/class_icons");

            foreach (Sprite asset in classIcons)
            {
                string stateIdentifiedKey = activeState.ToString() + asset.name;
                if (!spriteMap.ContainsKey(stateIdentifiedKey) )
                {
                    spriteMap.Add(stateIdentifiedKey, asset);
                }
            }
        }

        /***************************************************************
        * Loads battle state assets
        **************************************************************/
        private static void loadBattleStateAssets()
        {
            Sprite[] statusEffectIcons = Resources.LoadAll<Sprite>("textures/icon_textures/status_effects");

            foreach(Sprite iconSprite in statusEffectIcons)
            {
                string stateIdentifiedKey = activeState.ToString() + iconSprite.name;
                if (!spriteMap.ContainsKey(stateIdentifiedKey))
                {
                    spriteMap.Add(stateIdentifiedKey, iconSprite);
                }
            }
        }


        /***************************************************************
        @param - key: The asset to be loaded/returned
        @paran - path: the location on disk where that asset is

        @return - The sprite model for the key passed in.
        **************************************************************/
        public static Sprite getSprite(string key, string path = "", bool isStateSpecific = false)
        {
            string stateIdentifiedKey = isStateSpecific ? activeState.ToString() + key: key;
            if (!spriteMap.ContainsKey(stateIdentifiedKey))
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if(sprite == null)
                {
                    Debug.LogError($"AssetNotFound - Loading asset '{key}' at path 'Assets/Resources/{path}' failed. Chcek in Assets/Resources directory for missing or missmatched asset path {new System.Diagnostics.StackFrame().ToString()}") ;
                }

                spriteMap.Add(key, sprite);
            }

            return spriteMap[stateIdentifiedKey];
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
