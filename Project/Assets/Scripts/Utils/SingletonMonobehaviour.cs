using UnityEngine;

namespace PokerCats
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<T>();
                }

                if (_instance == null)
                {
                    GameObject gameObj = new GameObject();
                    gameObj.name = typeof(T).ToString();
                    _instance = gameObj.AddComponent<T>();
                }

                return _instance;
            }
        }
    }
}