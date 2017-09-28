using Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Script
{
    class Util
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T inner = default(T);
        }

        public static T FromJson<T>(string json)
        {
            string newJson = "{ \"inner\": " + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.inner;
        }
    }
}
