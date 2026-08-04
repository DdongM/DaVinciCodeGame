using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DavincicodeV._1
{
    internal interface IPlayer
    {
        string Name { get; set; }
        List<Card> Cards { get; set; }

        void ReceiveCards(List<Card> cards);
        void SortCards();
    }
}
