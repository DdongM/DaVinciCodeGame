using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavinciCodeServer
{
    internal class Card
    {
        private string color;

        public string Color
        {
            get { return color; }
            set { color = value; }
        }

        private int number;

        public int Number
        {
            get { return number; }
            set { number = value; }
        }

        public Card(string color, int number)
        {
            this.color = color;
            this.number = number;
        }

        public string Display()
        {
            return $"{(Color == "Black" ? "B" : "W")}-{(Number == 13 ? "J" : Number.ToString())}";
        }
    

    public override bool Equals(object obj)
        {
            if (obj is Card other)
                return this.Color == other.Color && this.Number == other.Number;
            return false;
        }
        public override int GetHashCode()
        {
            return Color.GetHashCode() ^ Number.GetHashCode();
        }
    }
}
