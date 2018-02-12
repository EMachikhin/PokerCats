using UnityEngine;
using UnityEngine.UI;


namespace PokerCats
{
    public class DeckWidget : MonoBehaviour
    {
        public Sprite Image
        {
            set
            {
                this.gameObject.GetComponent<Image>().sprite = value;
            }
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}
