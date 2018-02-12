using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;

namespace PokerCats
{
    public class MenuController : MonoBehaviour
    {
        void Start()
        {

        }

        void Update()
        {

        }

        public void OnNewGameClicked()
        {
            SceneManager.LoadScene("Game");
        }

        public void OnSettingsClicked()
        {

        }

        public void OnExitClicked()
        {
            Application.Quit();
        }
    }
}
