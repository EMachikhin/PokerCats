using UnityEngine;
using UnityEngine.UI;

namespace PokerCats
{
    public class CardWidget : MonoBehaviour
    {
        private Card _cardLink;

        public Card CardLink
        {
            get { return _cardLink; }
            set
            {
                _cardLink = value;
                Image = Resources.Load<Sprite>("Textures/Cards/" + value.GetTextInfo());
            }
        }

        public Sprite Image
        {
            set
            {
                this.gameObject.GetComponent<Image>().sprite = value;
            }
        }

        public float ImageWidth
        {
            get
            {
                return this.gameObject.GetComponent<RectTransform>().rect.width;
            }
        }

        public float ImageHeight
        {
            get
            {
                return this.gameObject.GetComponent<RectTransform>().rect.height;
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
