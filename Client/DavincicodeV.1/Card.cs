using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Card.cs (추상 클래스 기반)
namespace DavincicodeV._1
{
    public abstract class Card
    {
        public string Color { get; set; }
        public int Number { get; set; }

        protected Card(string color, int number)
        {
            Color = color;
            Number = number;
        }

        public abstract string Display();
    }

    public class StandardCard : Card
    {
        public StandardCard(string color, int number) : base(color, number) { }

        public override string Display()
        {
            return $"{(Color == "Black" ? "B" : "W")}-{(Number == 13 ? "J" : Number.ToString())}";
        }
    }
}