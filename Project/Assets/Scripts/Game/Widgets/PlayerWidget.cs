using UnityEngine;
using UnityEngine.UI;

namespace PokerCats
{
    public enum ChipAlignment
    {
        Invalid = -1,
        Left,
        Right,
        Count
    };

	public class PlayerWidget : MonoBehaviour
	{
        private Player _playerLink;
        private bool _isSeatEmpty = true;
        public bool IsSeatEmpty { get { return _isSeatEmpty; } }

        public ChipAlignment ChipAlignment;
        public Image PlayerAvatar;
        public Text SeatHintText;

        public Player PlayerLink
        {
            get { return _playerLink; }
            set { _playerLink = value; }
        }

        public void OnSeatClicked()
        {
            if (_isSeatEmpty) {
                SitHere();
            }
        }

        private void SitHere()
        {
            _isSeatEmpty = false;

            if (SeatHintText != null) {
                SeatHintText.gameObject.SetActive(false);
            }

            if (PlayerAvatar != null) {
                PlayerAvatar.gameObject.SetActive(true);
            }
        }
	}
}
