using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerCats
{
    public class Defines
    {
        // Game rules
        public static readonly int DECK_CARDS_COUNT = 52;
        public static readonly int HOLE_CARDS_COUNT = 2;
        public static readonly int FLOP_CARDS_COUNT = 3;
        public static readonly int CARDS_COUNT_ON_TURN = 4;
        public static readonly int CARDS_COUNT_ON_RIVER = 5;
        public static readonly int CARDS_IN_HAND_COUNT = 5;

        // Animation delays
        public static readonly float DELAY_IN_BLIND_POSTING = 0.5f;
        public static readonly float DELAY_BETWEEN_DEALING_CARDS = 0.25f;
        public static readonly float CARD_DEALING_TIME = 0.5f;
        public static readonly float DELAY_BETWEEN_DEALING_STREETS = 0.5f;
        public static readonly float SHOW_CARDS_TIME = 2.0f;

        // Table settings
        public static readonly int MAX_CHIPS_IN_ONE_STACK_IN_BETS = 5;
        public static readonly int MAX_STACKS_IN_ONE_ROW_IN_BETS = 5;

        // Timers
        public static readonly float SECONDS_FOR_TURN = 30.0f;
        public static readonly float AI_TURN_TIME = 1.0f;
    }
}
