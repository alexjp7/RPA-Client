using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Util
{
    class JsonParser<T>
    {
        public static string serialize(T obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public static T deserialize(string jsonString)
        {
            return JsonUtility.FromJson<T>(jsonString);
        }
    }
}
