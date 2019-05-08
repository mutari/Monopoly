using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monopol
{
    class Card
    {
        //1 = du får 2 = du ger 3 = gå i fängelse 4 = ut ur fängelse 5 = ny pos 6 = gå tillbaka x steg 7 = få av alla 8 = ge alla 

        private string text;
        private int id;

        private int value; //pengar man känar eller förlorar för det specefika kortet

        public Card(string text, int id)
        {
            this.text = text;
            this.id = id;
        }

        public int ID
        {
            get { return id; }
        }

        public string Text
        {
            get { return text; }
        }

        public int Value
        {
            get { return value; }
            set { this.value = value; }
        }

    }
}
