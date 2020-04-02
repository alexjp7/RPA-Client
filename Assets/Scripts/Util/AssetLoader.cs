using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Util
{
    public class AssetLoader
    {
        private static Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();

        public static Sprite getAsset(string key, string path = "")
        {
            if (!spriteMap.ContainsKey(key))
            {
                Debug.Log(key);
                spriteMap.Add(key, Resources.Load<Sprite>(path));
            }

            return spriteMap[key];
        }

        public static void destroyAsset(string key)
        {
            if (spriteMap.ContainsKey(key)) spriteMap.Remove(key);
        }
    } 
}
