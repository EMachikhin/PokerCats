using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerCats
{
    public class Pot
    {
        private int _potSize;
        private List<Player> _playersInPot = new List<Player>();

        public Pot(int size = 0)
        {
            _potSize = size;
        }

        public int Size
        {
            get { return _potSize; }
            set { _potSize = value; }
        }

        public List<Player> PlayersInPot
        {
            get { return _playersInPot; }
        }

        public bool IsPlayerInPot(Player player)
        {
            return _playersInPot.Contains(player);
        }
    }
}
