using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace PokerCats
{
    public class RandomNumber
    {
        private static readonly RNGCryptoServiceProvider _seed = new RNGCryptoServiceProvider();
        public static int GetRandomNumber(int maxValue)
        {
            byte[] randomNumber = new byte[1];

            _seed.GetBytes(randomNumber);

            double asciiValue = Convert.ToDouble(randomNumber[0]);

            double multiplier = Math.Max(0, (asciiValue / 255d) - 0.00000000001d);

            int range = maxValue + 1;

            double randomValue = Math.Floor(multiplier * range);

            return (int)(randomValue);
        }

    }
}
