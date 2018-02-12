using UnityEngine;
using UnityEngine.UI;

namespace PokerCats
{
    public enum ChipDenominations
    {
        One = 1,
        Five = 5,
        TwentyFive = 25,
        Fifty = 50,
        OneHundred = 100,
        FiveHundred = 500,
        OneThousand = 1000
    };

    public class ChipWidget : MonoBehaviour
    {
        private ChipDenominations _denomination;
        public ChipDenominations Denomination
        {
            set
            {
                _denomination = value;
                Image = Resources.Load<Sprite>("Textures/Chips/" + ((int)value).ToString());
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
