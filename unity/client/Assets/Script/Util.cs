using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Script
{
    public class Util
    {
        public static void LoadSplashScene()
        {
            SceneManager.LoadScene("SplashScene", LoadSceneMode.Single);
        }

        public static void LoadGameScene()
        {
            SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
        }

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
