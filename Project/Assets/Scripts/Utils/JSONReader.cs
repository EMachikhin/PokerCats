using UnityEngine;
using System.Collections;
using System.IO;
using LitJson;

namespace PokerCats
{
    public class JSONReader : SingletonMonoBehaviour<JSONReader>
    {
        private string _jsonString;
        private JsonData _configData;

        public void PrepareConfig()
        {
            StartCoroutine(LoadConfig());
        }

        private IEnumerator LoadConfig()
        {
            string jsonConfigPath = Path.Combine(Application.streamingAssetsPath, Path.Combine("JSON", "Config.json"));

            // Android
            if (jsonConfigPath.Contains("://"))
            {
                WWW www = new WWW(jsonConfigPath);
                yield return www;
                _jsonString = www.text;
            }
            else
            {
                _jsonString = File.ReadAllText(jsonConfigPath);
            }

            ReadConfig();
            ParseConfig();
        }

        private void ReadConfig()
        {
            _configData = JsonMapper.ToObject(_jsonString);
        }

        private void ParseConfig()
        {
            JsonData preflopRanges = _configData["PreflopRanges"];
            for (int rangeIndex = 0; rangeIndex < preflopRanges.Count; rangeIndex++) {
                ParsePreflopRange(preflopRanges[rangeIndex]);
            }
        }

        private void ParsePreflopRange(JsonData preflopRange)
        {
            string positionString = preflopRange["Position"].ToString();
            Position position = Utils.GetPositionByString(positionString);

            JsonData openRaiseRange = preflopRange["OpenRaise"];
            ParsePreflopOpenRaiseRange(position, openRaiseRange);

            JsonData coldCallRange = preflopRange["ColdCall"];
            ParsePreflopColdCallRange(position, coldCallRange);

            JsonData threeBetRange = preflopRange["3Bet"];
            ParsePreflop3BetRange(position, threeBetRange);

            JsonData call3BetRange = preflopRange["Call3Bet"];
            ParsePreflop3BetCallingRange(position, call3BetRange);

            JsonData fourBetRange = preflopRange["4Bet"];
            ParsePreflop4BetRange(position, fourBetRange);
        }

        private void ParsePreflopOpenRaiseRange(Position position, JsonData openRaiseRange)
        {
            for (int handIndex = 0; handIndex < openRaiseRange.Count; handIndex++) {
                string handString = openRaiseRange[handIndex].ToString();
                HoleCards hand = Utils.GetHandByString(handString);
                PlayerAI.Instance.AddHandToPreflopOpenRaiseRange(position, hand);
            }
        }

        private void ParsePreflopColdCallRange(Position position, JsonData coldCallRange)
        {
            for (int vsPositionIndex = 0; vsPositionIndex < coldCallRange.Count; vsPositionIndex++) {
                string vsPositionString = coldCallRange[vsPositionIndex]["VsPosition"].ToString();
                Position vsPosition = Utils.GetPositionByString(vsPositionString);

                JsonData callingRange = coldCallRange[vsPositionIndex]["Range"];

                for (int handIndex = 0; handIndex < callingRange.Count; handIndex++) {
                    string handString = callingRange[handIndex].ToString();
                    HoleCards hand = Utils.GetHandByString(handString);
                    PlayerAI.Instance.AddHandToPreflopColdCallRange(position, vsPosition, hand);
                }
            }
        }

        private void ParsePreflop3BetRange(Position position, JsonData threeBetRange)
        {
            for (int vsPositionIndex = 0; vsPositionIndex < threeBetRange.Count; vsPositionIndex++) {
                string vsPositionString = threeBetRange[vsPositionIndex]["VsPosition"].ToString();
                Position vsPosition = Utils.GetPositionByString(vsPositionString);

                JsonData threeBettingRange = threeBetRange[vsPositionIndex]["Range"];

                for (int handIndex = 0; handIndex < threeBettingRange.Count; handIndex++) {
                    string handString = threeBettingRange[handIndex].ToString();
                    HoleCards hand = Utils.GetHandByString(handString);
                    PlayerAI.Instance.AddHandToPreflop3BetRange(position, vsPosition, hand);
                }
            }
        }

        private void ParsePreflop3BetCallingRange(Position position, JsonData threeBetCallRange)
        {
            for (int vsPositionIndex = 0; vsPositionIndex < threeBetCallRange.Count; vsPositionIndex++) {
                string vsPositionString = threeBetCallRange[vsPositionIndex]["VsPosition"].ToString();
                Position vsPosition = Utils.GetPositionByString(vsPositionString);

                JsonData threeBetCallingRange = threeBetCallRange[vsPositionIndex]["Range"];

                for (int handIndex = 0; handIndex < threeBetCallingRange.Count; handIndex++) {
                    string handString = threeBetCallingRange[handIndex].ToString();
                    HoleCards hand = Utils.GetHandByString(handString);
                    PlayerAI.Instance.AddHandToPreflop3BetCallingRange(position, vsPosition, hand);
                }
            }
        }

        private void ParsePreflop4BetRange(Position position, JsonData fourBetRange)
        {
            for (int vsPositionIndex = 0; vsPositionIndex < fourBetRange.Count; vsPositionIndex++) {
                string vsPositionString = fourBetRange[vsPositionIndex]["VsPosition"].ToString();
                Position vsPosition = Utils.GetPositionByString(vsPositionString);

                JsonData fourBettingRange = fourBetRange[vsPositionIndex]["Range"];

                for (int handIndex = 0; handIndex < fourBettingRange.Count; handIndex++) {
                    string handString = fourBettingRange[handIndex].ToString();
                    HoleCards hand = Utils.GetHandByString(handString);
                    PlayerAI.Instance.AddHandToPreflop4BetRange(position, vsPosition, hand);
                }
            }
        }
    }
}
