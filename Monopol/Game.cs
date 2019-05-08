using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace Monopol
{
    class Game
    {
        public static List<Player> Players = new List<Player>(); // aktiva spelare
        public static int AmountOfPlayersLeft = 0; //antal spelare kar 
        public static int MaxPlayer = 3; // 3 clienter och 1 server spelare
        public static int Dice1 = 0, Dice2 = 0;

        public static List<Plate> Plates = new List<Plate>(); // en lista med alla plattor i spelet
        public static List<Card> Cards = new List<Card>(); 

        public static int NowPlaying = 0; // vilken spelar som spelar just nu

        public static bool GameRunning = false; // om spelet är i gång

        public static void LoadCards()
        {
            //laddar in all korten
            string[] data = File.ReadAllLines("CardInfo.conf");

            string str = string.Join(" ", data.ToArray());

            EXEConfigCards(Parser(str));

            //blandar korten
            Random r = new Random();

            int n = Cards.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                Card temp = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = temp;
            }
        }

        public static Card DrawACard()
        {
            Card c = Cards.ElementAt(0);

            Cards.Remove(c);//tar bort kortet ur listan
            Cards.Insert(Cards.Count, c); //lägger till kortet sist i listan

            return c;
        }

        public static void LoadPlates()
        {
            //laddar in alla spelplattor och all den information i från en fil på server datorn
            string[] data = File.ReadAllLines("PlateInfo.conf");
            
            string str = string.Join(" ", data.ToArray());

            EXEConfigPlates(Parser(str));
        }

        private static void EXEConfigPlates(List<string> tokens)
        {
            Plate temp = new Plate();
            int i = 0;
            string type = "";
            while(i < tokens.Count)
            {
                if (tokens.ElementAt(i) == "SP")
                {
                    i++;
                    type = tokens.ElementAt(i).Substring(7); // när jag hämtar elementet har det värde STRING:x där x är det jag vill ha jag använder substring för att klippa bort den biten av stringen
                    if (type == "husplatta")
                    {
                        i++;
                        temp = new House();
                        temp.Typ = 1;
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "Chans")
                    {
                        i++;
                        temp = new Plate();
                        temp.Typ = 2;
                        temp.Name = "CHANS";
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "Inkomstskatt")
                    {
                        i++;
                        temp = new Plate();
                        temp.Typ = 3;
                        temp.Name = "INKOMSTSKATT";
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "Tåg station")
                    {
                        i++;
                        temp = new TrainStation();
                        temp.Typ = 4;
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "BaraPåBesök")
                    {
                        i++;
                        temp = new Plate();
                        temp.Typ = 5;
                        temp.Name = "Bara på besök";
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "StatligtVerk")
                    {
                        i++;
                        temp = new StateWork();
                        temp.Typ = 6;
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "FRIparkering")
                    {
                        i++;
                        temp = new Plate();
                        temp.Typ = 7;
                        temp.Name = "FRI PARKERING";
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "GåIFängelse")
                    {
                        i++;
                        temp = new Plate();
                        temp.Typ = 8;
                        temp.Name = "GÅ I FÄNGELSE";
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (type == "gå")
                    {
                        i++;
                        temp = new Plate();
                        temp.Typ = 9;
                        temp.Name = "GÅ";
                        temp.x = int.Parse(tokens.ElementAt(i).Substring(7));
                        temp.y = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                }
                else if (tokens.ElementAt(i) == "EP")
                {
                    Plates.Add(temp);
                    i++;
                }
                else if (type == "husplatta")
                {
                    if (tokens.ElementAt(i) == "SPRISE")
                    {
                        i++;
                        int index = 0; // SPRISE är starten på en aray med priser, använder en while lop för att gå igenom arayen tills EPRISE dyker upp
                        while (true)
                        {
                            string t = tokens.ElementAt(i + index);
                            if (t != "EPRISE")
                            {
                                (temp as House).rent[index] = int.Parse(tokens.ElementAt(i + index).Substring(7));
                                index++;
                            }
                            else
                            {
                                i++;//ökar med 1 då EPRISE har hittats
                                i += index; //ökar med index då alla värden i arrayen är hittade
                                break;
                            }
                        }
                    }
                    else if (tokens.ElementAt(i) == "COLOR")
                    {
                        (temp as House).colorID = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "HOUSE")
                    {
                        (temp as House).housePrise = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "MORT")
                    {
                        (temp as House).mortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "UMORT")
                    {
                        (temp as House).undoMortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "NAME")
                    {
                        (temp as House).Name = tokens.ElementAt(i + 1).Substring(7);
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "PRISE")
                    {
                        (temp as House).prise = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                }
                else if (type == "Inkomstskatt")
                {
                    if (tokens.ElementAt(i) == "PRISE")
                    {
                        temp.gre = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                }
                else if (type == "Tåg station")
                {
                    if (tokens.ElementAt(i) == "SPRISE")
                    {
                        i++;
                        int index = 0; // SPRISE är starten på en aray med priser, använder en while lop för att gå igenom arayen tills EPRISE dyker upp
                        while (true)
                        {
                            string t = tokens.ElementAt(i + index);
                            if (t != "EPRISE")
                            {
                                (temp as TrainStation).rent[index] = int.Parse(tokens.ElementAt(i + index).Substring(7));
                                index++;
                            }
                            else
                            {
                                i++;//ökar med 1 då EPRISE har hittats
                                i += index; //ökar med index då alla värden i arrayen är hittade
                                break;
                            }
                        }
                    }
                    else if (tokens.ElementAt(i) == "MORT")
                    {
                        (temp as TrainStation).mortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "UMORT")
                    {
                        (temp as TrainStation).undoMortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "NAME")
                    {
                        (temp as TrainStation).Name = tokens.ElementAt(i + 1).Substring(7);
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "PRISE")
                    {
                        (temp as TrainStation).prise = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                }
                else if (type == "husplatta")
                {
                    if (tokens.ElementAt(i) == "SPRISE")
                    {
                        i++;
                        int index = 0; // SPRISE är starten på en aray med priser, använder en while lop för att gå igenom arayen tills EPRISE dyker upp
                        while (true)
                        {
                            string t = tokens.ElementAt(i + index);
                            if (t != "EPRISE")
                            {
                                (temp as House).rent[index] = int.Parse(tokens.ElementAt(i + index).Substring(7));
                                index++;
                            }
                            else
                            {
                                i++;//ökar med 1 då EPRISE har hittats
                                i += index; //ökar med index då alla värden i arrayen är hittade
                                break;
                            }
                        }
                    }
                    else if (tokens.ElementAt(i) == "COLOR")
                    {
                        (temp as House).colorID = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "HOUSE")
                    {
                        (temp as House).housePrise = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "MORT")
                    {
                        (temp as House).mortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "UMORT")
                    {
                        (temp as House).undoMortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "NAME")
                    {
                        (temp as House).Name = tokens.ElementAt(i + 1).Substring(7);
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "PRISE")
                    {
                        (temp as House).prise = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                }
                else if (type == "Inkomstskatt")
                {
                    if (tokens.ElementAt(i) == "PRISE")
                    {
                        temp.gre = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                }
                else if (type == "gå")
                {
                    if (tokens.ElementAt(i) == "PRISE")
                    {
                        temp.gre = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                }
                else if (type == "StatligtVerk")
                {
                    if (tokens.ElementAt(i) == "SPRISE")
                    {
                        i++;
                        int index = 0; // SPRISE är starten på en aray med priser, använder en while lop för att gå igenom arayen tills EPRISE dyker upp
                        while (true)
                        {
                            string t = tokens.ElementAt(i + index);
                            if (t != "EPRISE")
                            {
                                (temp as StateWork).multiplayer[index] = int.Parse(tokens.ElementAt(i + index).Substring(7));
                                index++;
                            }
                            else
                            {
                                i++;//ökar med 1 då EPRISE har hittats
                                i += index; //ökar med index då alla värden i arrayen är hittade
                                break;
                            }
                        }
                    }
                    else if (tokens.ElementAt(i) == "MORT")
                    {
                        (temp as StateWork).mortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "UMORT")
                    {
                        (temp as StateWork).undoMortageValue = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "NAME")
                    {
                        (temp as StateWork).Name = tokens.ElementAt(i + 1).Substring(7);
                        i += 2;
                    }
                    else if (tokens.ElementAt(i) == "PRISE")
                    {
                        (temp as StateWork).prise = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                        i += 2;
                    }
            
                }
            }
        }

        //väldigt simpel parser som bara kan hitta keywords samt strängar (man bestämmer om det är ett intvärde senare med en TryPars funktion)
        private static List<string> Parser(string data)
        {
            //compilarear config filen till plattorna
            List<string> tokens = new List<string>();
            char[] dataList = data.ToCharArray();
            
            bool isString = false;
            string str = "";

            string delTok = "";

            int i = 0;
            while(i < data.Length) 
            {
                //testar om texten är en string
                delTok += dataList[i];
                if (isString && delTok != "\"")
                {
                    str += delTok;
                    delTok = "";
                }
                else if (delTok == "\"")
                {
                    if (!isString)
                        isString = true;
                    else
                    {
                        isString = false;
                        tokens.Add("STRING:" + str);
                        str = "";
                    }
                    delTok = "";
                }
                //letar efter bestämda ord för plates
                else if (delTok == "StartPlate")
                {
                    delTok = "";
                    tokens.Add("SP");
                }
                else if (delTok == "startPrisList")
                {
                    tokens.Add("SPRISE");
                    delTok = "";
                }
                else if (delTok == "prise")
                {
                    tokens.Add("PRISE");
                    delTok = "";
                }
                else if (delTok == "colorID")
                {
                    delTok = "";
                    tokens.Add("COLOR");
                }
                else if (delTok == "endPrisList")
                {
                    delTok = "";
                    tokens.Add("EPRISE");
                }
                else if (delTok == "house")
                {
                    delTok = "";
                    tokens.Add("HOUSE");
                }
                else if (delTok == "mortage")
                {
                    delTok = "";
                    tokens.Add("MORT");
                }
                else if (delTok == "undoMortage")
                {
                    delTok = "";
                    tokens.Add("UMORT");
                }
                else if (delTok == "name")
                {
                    delTok = "";
                    tokens.Add("NAME");
                }
                else if (delTok == "EndPlate")
                {
                    delTok = "";
                    tokens.Add("EP");
                }

                //letar efter bestämda ord för cards
                else if (delTok == "NewCard")
                {
                    delTok = "";
                    tokens.Add("NC");
                }


                //rensar bort teken som inte har någon riktig betydelse mening
                else if (delTok == ":") delTok = "";
                else if (delTok == " " || delTok == "\t" || delTok == "\n") delTok = "";
                i++;
            }

            return tokens;
        }

        public static void EXEConfigCards(List<string> tokens)
        {
            int i = 0;
            while (i < tokens.Count)
            {
                if (tokens.ElementAt(i) == "NC")
                {
                    int id = int.Parse(tokens.ElementAt(i + 1).Substring(7));
                    string text = tokens.ElementAt(i + 2).Substring(7);
                    int value = int.Parse(tokens.ElementAt(i + 3).Substring(7));

                    Card c = new Card(text, id);

                    if (value != 0) c.Value = value;

                    Cards.Add(c);
                    i += 4;
                }
            }
        }

        //för att spelar listen inte ska inehålla null object
        public static void QuickLoadPlayers()
        {
            Players.Add(new Player());
            Players.Add(new Player());
            Players.Add(new Player());
            Players.Add(new Player());
        }

    }
}
