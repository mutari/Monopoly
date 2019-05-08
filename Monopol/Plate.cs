using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Monopol
{
    class Plate
    {
        protected string name = "";
        //typ detta beskriver vad det är för platta om den inte har någon child class
        protected int typ = -1; // husplatat = 1, chance = 2, inkomstskatt = 3, tågstation = 4, bara på besök = 5, statligtverk = 6, fri pakering = 7, gi i fängelse = 8, gå = 9 
        public int gre = 0; // gre är en variabel där man kan spara vilket värde man vill

        protected Player owner = null;

        //var figuren ska tå på plattan
        public int x, y;

        public virtual int Event(Player client)
        {
            return 0;
        }

        public virtual int ToPay()
        {
            return 0;
        }

        public Player Owner
        {
            get { return owner; }
            set { owner = value; }
        }

        public int Typ
        {
            get { return typ; }
            set { typ = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public override string ToString()
        {
            return "Plate: " + name;
        }

    }

    class Plot : Plate
    {
        public int mortageValue;
        public int undoMortageValue;
        public int prise; //vad det kostar att köpa plattan

        public override int Event(Player player)
        {
            return 0;
        }

        public override int ToPay()
        {
            return 0;
        }
    }

    class House : Plot
    {
        public int[] rent = new int[7];
        public int housePrise = 0;
        public int amountOfHouses = 0;
        public int colorID; //vilken färg grup plattan tilhör

        public bool buyHouse(Player player)
        {
            return false;
        }

        public override int Event(Player player)
        {
            return 0;
        }

        public override int ToPay() 
        {
            int toPay = 0;
            //Player kommer inte att behövas

            //ownern sparar all data om hur många av samma farg samt hus
            //beror på hur mycket hus man har samt om man har full color

            if (((colorID == 1 || colorID == 8) && owner.amountOfPlatesOfEveryColor[colorID - 1] == 2) || //testar om spelaren äger de två första eller de trvå sista
                (owner.amountOfPlatesOfEveryColor[colorID] == 3)) // testar om spelaren äger alla plator av samma färg
                toPay = rent[1 + amountOfHouses]; // om man äger full ferg betalar du pris 2 i rent arrayen om du har 1 hus betalar du pris 3 i rent arrayen
            else
                toPay = rent[0]; //om man bara äger plattan betalar man normal pris

            return toPay;
        }

        public override string ToString()
        {
            return "prise:" + prise + "   rent1: " + rent[0] + "   rent2: " + rent[1] + "   rent3: " + rent[2] + "   rent4: " + rent[3]
                + "   rent5: " + rent[4] + "   rent6: " + rent[5] + "   rent7: " + rent[6] + "    name: " + name;
        }

    }

    class TrainStation : Plot
    {
        public int[] rent = new int[4];

        public override int Event(Player player)
        {
            return base.Event(player);
        }

        public override int ToPay()
        {
            int toPay = 0;

            toPay = rent[owner.amountOfTrainStations-1];

            return toPay;
        }

        public override string ToString()
        {
            return "prise:" + prise + "   rent1: " + rent[0] + "   rent2: " + rent[1] + "   rent3: " + rent[2] + "   rent4: " + rent[3]
                +  "    name: " + name;
        }
    }

    class StateWork : Plot
    {
        public int[] multiplayer = new int[2];

        public override int ToPay()
        {
            int toPay = 0;

            Random r = new Random();
            
            toPay = owner.amountOfStateHouses == 1 ? (r.Next(12) + 1) * 4 : (owner.amountOfStateHouses == 2 ? (r.Next(12)+1) * 10 : 0);

            return toPay;
        }

        public override int Event(Player player)
        {
            return base.Event(player);
        }

        public override string ToString()
        {
            return base.ToString();
        }

    }
}