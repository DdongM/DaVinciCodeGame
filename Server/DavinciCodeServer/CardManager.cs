using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavinciCodeServer
{
    internal class CardManager
    {
        private static Random rnd = new Random();

        public static List<Card> GenerateDeck()
        {
            List<Card> deck = new List<Card>();

            for (int i = 0; i <= 12; i++)
            {
                deck.Add(new Card("Black", i));
                deck.Add(new Card("White", i));
            }

            deck.Add(new Card("Black", 13)); 
            deck.Add(new Card("White", 13));

            return deck;
        }

        public static List<Card> DrawCards(List<Card> deck, int count, List<Card> currentHand)
        {
            List<Card> result = new List<Card>();
            Random rand = new Random();

            var available = deck
                .Where(c => !currentHand.Any(h => h.Color == c.Color && h.Number == c.Number))  // ✅ 중복 제거 조건
                .ToList();

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int index = rand.Next(available.Count);
                Card selected = available[index];
                result.Add(selected);
                deck.Remove(selected);
                available.RemoveAt(index);
            }

            return result;
        }

        public static List<Card> DrawCards(List<Card> deck, int count)
        {
            return DrawCards(deck, count, new List<Card>());
        }
    }
}
