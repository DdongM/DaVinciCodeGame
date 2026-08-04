using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavincicodeV._1
{
    internal class Player : IPlayer
    {
		private string name;

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		private List<Card> cards;

		public List<Card> Cards
		{
			get { return cards; }
			set { cards = value; }
		}

		public Player(string name)
		{
			this.name = name;
			this.cards = new List<Card>();
		}

		public void ReceiveCards(List<Card> cards)
		{
			this.cards = cards;
		}

		public void SortCards()
		{
			cards.Sort((a, b) => a.Number.CompareTo(b.Number));
		}


	}
}
