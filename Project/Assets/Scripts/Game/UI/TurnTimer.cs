using UnityEngine;
using System.Collections;

namespace PokerCats
{
    public class TurnTimer : MonoBehaviour
    {
        public Transform ClewTransform;
        public RectTransform ThreadTransform;

        private bool _isTimerTicking;
        private float _timeSpentForTurn;
        private Vector2 _initialTimerPosition;
        private Vector2 _initialThreadOffsetMax;

        private const float TIMER_POSITION_DELTA = 50f;
        private const float CLUE_OFFSET_MAX_DELTA = -80f;

        public Vector2 TimerPosition
        {
            get { return this.gameObject.transform.localPosition; }
            private set { this.gameObject.transform.localPosition = value; }
        }

	    void Start()
        {
            _initialTimerPosition = TimerPosition;
            _initialThreadOffsetMax = ThreadTransform.offsetMax;
	    }

        public void StartTimer()
        {
            SetTimerVisible(true);
            _timeSpentForTurn = 0;
            _isTimerTicking = true;
            StartCoroutine(UpdateTimerUI());
        }

        public void StopTimer()
        {
            _isTimerTicking = false;
        }

        private IEnumerator UpdateTimerUI()
        {
            if (ClewTransform == null || ThreadTransform == null) {
                yield break;
            }

            Vector2 timerTransformFinalPosition = _initialTimerPosition;
            timerTransformFinalPosition.x += TIMER_POSITION_DELTA;

            Vector2 threadTransformFinalOffsetMax = _initialThreadOffsetMax;
            threadTransformFinalOffsetMax.x += CLUE_OFFSET_MAX_DELTA;

            while (_timeSpentForTurn < Defines.SECONDS_FOR_TURN && _isTimerTicking) {
                _timeSpentForTurn += Time.deltaTime;
                float timeSpentPercentage = _timeSpentForTurn / Defines.SECONDS_FOR_TURN;
                TimerPosition = Vector2.Lerp(_initialTimerPosition, timerTransformFinalPosition, timeSpentPercentage);
                ClewTransform.rotation = Quaternion.Euler(0, 0, -_timeSpentForTurn * 100);
                ThreadTransform.offsetMax = Vector2.Lerp(_initialThreadOffsetMax, threadTransformFinalOffsetMax, timeSpentPercentage);
                yield return null;
            }

            if (_isTimerTicking) {
                GameController.Instance.EndPlayerTurnOnTimeout();
            }

            ResetTimerState();
        }

        private void ResetTimerState()
        {
            TimerPosition = _initialTimerPosition;
            ThreadTransform.offsetMax = _initialThreadOffsetMax;
            SetTimerVisible(false);
        }

        private void SetTimerVisible(bool show)
        {
            ClewTransform.gameObject.SetActive(show);
            ThreadTransform.gameObject.SetActive(show);
        }
    }
}
