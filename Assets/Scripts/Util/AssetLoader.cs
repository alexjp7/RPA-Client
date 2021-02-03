

/*---------------------------------------------------------------
                    ASSET-LOADER
 ---------------------------------------------------------------*/
/***************************************************************
* The Assetloader class provides the functionality for 
  loading and storing re-usable assets/sprite models.
**************************************************************/
namespace Assets.Scripts.Util
{
    using log4net;
    using UnityEngine;
    using System.Collections.Generic;

    public class AssetLoader
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AssetLoader));

        /// <summary>
        /// Internal reference to static sprite data. Caches sprites to prevent disk loads.
        /// </summary>
        private static Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();

        /// <summary>
        /// Prevents double loading of common assets.
        /// </summary>
        private static bool isInitialLoad = true;

        /// <summary>
        /// Reference to current game state to append to <see cref="spriteMap"/> keys to group assets into game states.
        /// </summary>
        private static GameState activeState;

        private static string COMMON_ASSET_PATH = "textures/icon_textures/general";
        private static string CLASS_ICON_PATH = "textures/icon_textures/class_icons";
        private static string STATUS_ICON_PATH = "textures/icon_textures/status_effects";

        /// <summary>
        /// Loads assets relevant for a particular game state.
        /// </summary>
        /// <param name="gameState"> Gamestate that is currently in-play for the client-side player.</param>
        public static void loadStaticAssets(GameState gameState)
        {
            if (gameState == activeState) return;
            dumpPreviousResources();
            activeState = gameState;

            if (isInitialLoad)
            {
                batchLoadAssets(COMMON_ASSET_PATH);
            }

            string statePrefix = activeState.ToString();

            switch (gameState)
            {
                case GameState.MAIN_MENU:
                    break;

                case GameState.CHARACTER_CREATION:
                    batchLoadAssets(CLASS_ICON_PATH, statePrefix);

                    break;

                case GameState.BATTLE_STATE:
                    batchLoadAssets(STATUS_ICON_PATH, statePrefix);
                    break;

                default:
                    Debug.LogError($"UnexpecteD Gamestate argument {gameState} provided during asset loading.  {new System.Diagnostics.StackFrame()}");
                    break;
            }
        }

        /// <summary>
        /// Loads all assets from the provided path, and loads them into the <see cref="spriteMap"/>
        /// </summary>
        /// <param name="path">The path to load assets form</param>
        /// <param name="keyPrefix">An identifer to group assets by. Useful to perform group loads/dumps of prefixed assets.</param>
        public static void batchLoadAssets(string path, string keyPrefix = "")
        {
            log.Debug($"Batch-Loading: from '{path}'");

            if(isDuplicateLoad(path))
            {
                log.Info($"Loading cancelled, sprites have already been processed.");
            }

            Sprite[] spirtesLoaded = Resources.LoadAll<Sprite>(path);

            foreach (Sprite asset in spirtesLoaded)
            {
                log.Debug($"Loading: [{asset.name}] from '{path}'");

                string stateIdentifiedKey = keyPrefix + asset.name;

                if (!spriteMap.ContainsKey(stateIdentifiedKey))
                {
                    spriteMap.Add(stateIdentifiedKey, asset);
                }
            }
        }

        /// <summary>
        /// Checks to see if batch loaded assets have already been loaded.
        /// </summary>
        /// <param name="path">An asset data path that is a candidate for batch loading</param>
        private static bool isDuplicateLoad(string path)
        {
            string[] tokens = path.Split('/');
            if (tokens.Length > 1)
            {
                string lastToken = tokens[tokens.Length - 1];
                if (lastToken.EndsWith("parts"))
                {
                    foreach (var spriteName in spriteMap.Keys)
                    {
                        string entityName = lastToken.Split('.')[0];
                        if (spriteName.StartsWith($"{entityName}."))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }      
        
        /// <summary>
        /// Removes unused assets from the sprite map as the game state changes 
        /// </summary>
        private static void dumpPreviousResources()
        {
            List<string> assetsRemoving = new List<string>();

            foreach (var asset in spriteMap)
            {
                if (asset.Key.StartsWith(activeState.ToString()))
                {
                    assetsRemoving.Add(asset.Key);
                }
            }

            foreach (var asset in assetsRemoving)
            {
                spriteMap.Remove(asset);
            }
        }

        /// <summary>
        /// Returns the sprite from <see cref="spriteMap>">SpriteMap</see>. If the sprite doesn't exist in the map this function will attempt to load it from disk.
        /// </summary>
        /// <param name="key">The asset to be loaded/returned</param>
        /// <param name="path">the location on disk where that asset is on disk</param>
        /// <param name="isStateSpecific"></param>
        /// <returns>The sprite model for the key passed in. Returns <c> null</c> if no asset can be found. </returns>
        public static Sprite getSprite(string key, string path = "", bool isStateSpecific = false)
        {
            string stateIdentifiedKey = isStateSpecific ? activeState.ToString() + key : key;

            if (!spriteMap.ContainsKey(stateIdentifiedKey))
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                log.Debug($"Loading: [{key}] from '{path}'");

                if (sprite != null)
                {
                    spriteMap.Add(key, sprite);
                }
                else
                {
                    log.Error($"AssetNotFound - Loading asset '{key}' at path 'Assets/Resources/{path}' failed. Chcek in Assets/Resources directory for missing or missmatched asset path {new System.Diagnostics.StackFrame()}");
                }
            }

            return spriteMap?[stateIdentifiedKey];
        }

        /// <summary>
        /// Destroys the asset, removing it from memory.
        /// </summary>
        /// <param name="key">The asset to be loaded/returned</param>
        public static void destroyAsset(string key)
        {
            if (spriteMap.ContainsKey(key)) spriteMap.Remove(key);
        }
    }
}
