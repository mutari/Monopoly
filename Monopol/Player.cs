using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Monopol
{
    class Player
    {
        public int pocition = 0; // värdet på den rutan man befiner sig på
        public int money = 1500; // hur mycket pengar du har 1500 är mängden
        public int amountOfEskipCards = 0; // antal kort frikort spelaren har i från fängelset
        public int amountOfStateHouses = 0;
        public int amountOfTrainStations = 0;
        public int amountOfDoubleDice = 0;
        public string name = "{name}"; //  namnet på spelaren
        public bool inPrison = false;  // om du är i fängelse
        public int amountOfRoundsInPrison = 0;// hur länge du vart i fängelse
        public int[] amountOfPlatesOfEveryColor = new int[8];
        public int x; // spelarens kordinater
        public int y;
        public TcpClient client = null;
        public bool playing = true;

        public Player() { }

        public Player(TcpClient c)
        {
            this.client = c;
        }
    }
}
