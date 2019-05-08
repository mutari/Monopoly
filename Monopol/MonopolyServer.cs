using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.IO;

namespace Monopol
{
    class MonopolyServer
    {
        private TcpListener server;
        private readonly int port;
 
        public MonopolyServer(int port)
        {
            this.port = port;
            server = new TcpListener(IPAddress.Any, port);

            StreamWriter writer = new StreamWriter(new FileStream("GameInfo.txt", FileMode.OpenOrCreate, FileAccess.Write));

            writer.Write(""); // rensar textfilen när man startar ett nytt spel

            writer.Dispose();

        }

        public void startaServer()
        {
            server.Start();
        }

        public async void asyncListener()
        {
            TcpClient client;

            try
            {
                client = await server.AcceptTcpClientAsync();

                Game.Players.Add(new Player(client));

                startaAsyncLäsning(client);
            }
            catch (Exception e) { Console.Write(e); return; }

            if (Game.Players.Count <= 4)
            {
                asyncListener();
            }
        }

        public async void asyncWrite(TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();

            byte[] utData = Encoding.Unicode.GetBytes(message);

            try
            {
                await stream.WriteAsync(utData, 0, utData.Length);

                //sparar medelandena till en spel fil där man kan gå tillbaka för att kolla vad som hent i alla sinna spel
                FileStream inStream = new FileStream("GameInfo.txt", FileMode.Open, FileAccess.Read);

                StreamReader reader = new StreamReader(inStream);

                string text = reader.ReadToEnd();

                reader.Dispose();

                FileStream outStream = new FileStream("GameInfo.txt", FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter writer = new StreamWriter(outStream);

                writer.Write(text + "\n" + new DateTime() + message);

                writer.Dispose();

            }
            catch (Exception error) { MessageBox.Show(error.ToString()); return; }
        }

        public void normalWrite(TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();

            byte[] utData = Encoding.Unicode.GetBytes(message);

            try
            {
                stream.Write(utData, 0, utData.Length);

                //sparar medelandena till en spel fil där man kan gå tillbaka för att kolla vad som hent i alla sinna spel
                FileStream inStream = new FileStream("GameInfo.txt", FileMode.Open, FileAccess.Read);

                StreamReader reader = new StreamReader(inStream);

                string text = reader.ReadToEnd();

                reader.Dispose();

                FileStream outStream = new FileStream("GameInfo.txt", FileMode.OpenOrCreate, FileAccess.Write);

                StreamWriter writer = new StreamWriter(outStream);

                writer.Write(text + "\n" + new DateTime() + message);

                writer.Dispose();

            }
            catch(Exception error) { MessageBox.Show(error.ToString()); return; }
        }

        public void writeAsyncToAllPlayers(string message)
        {
            for (int i = 0; i < Game.Players.Count; i++)
                asyncWrite(Game.Players.ElementAt(i).client, message);
        }

        public void writeToAllPlayers(string message)
        {
            for (int i = 0; i < Game.Players.Count; i++)
                normalWrite(Game.Players.ElementAt(i).client, message);
        }

        public async void startaAsyncLäsning(TcpClient klient)
        {
            try
            {
                NetworkStream stream = klient.GetStream();

                byte[] bytes = new byte[1028];
                string data = "";

                int inputData = await stream.ReadAsync(bytes, 0, bytes.Length);

                data = Encoding.Unicode.GetString(bytes, 0, inputData);

                parsData(data);
                //startaAsyncSkrivning(klient, respons); // respons

                startaAsyncLäsning(klient);

            }
            catch (Exception e) { Console.Write(e); return; }

            startaAsyncLäsning(klient);
        }

        private void parsData(string data)
        {
            string[] splitdata = data.Split(':');

            //data ifrpn serven och vad jag ska göra med det
            for (int i = 0; i < splitdata.Length; i++)
            {
                //tar bort alla blanka stringar i splitdata arrayen
                if (splitdata[i] == "") continue; //tar bort alla blanka stringar i splitdata arrayen
                if (splitdata[i] == "newPlayer")
                {
                    writeAsyncToAllPlayers(":uppdateConections:");
                    Game.Players.Last().name = splitdata[i + 1];
                    Game.AmountOfPlayersLeft++; // lägger till en spelare

                    string toSend = "";
                    for (int j = 0; j < Game.Players.Count; j++)
                    {
                        toSend += ":newPlayer:" + Game.Players.ElementAt(j).name + ":";
                    }

                    writeAsyncToAllPlayers(toSend);

                    i += 2;
                }
                else if (splitdata[i] == "logingOut")
                {
                    for(int j = 0; j < Game.Players.Count; j++)
                        normalWrite(Game.Players.ElementAt(j).client, ":closewindow:" + splitdata[i + 1] + ":");
                    server.Stop();
                    System.Environment.Exit(0);
                    i++;
                }
                else if (splitdata[i] == "dice")
                {
                    //börjare med att testa om spelaren är i fängelse
                    //Player p = getPlayerByName(splitdata[i + 3]);
                    Player p = Game.Players.ElementAt(Game.NowPlaying);
                    Game.Dice1 = int.Parse(splitdata[i + 1]);
                    Game.Dice2 = int.Parse(splitdata[i + 2]);
                    
                    if (!p.inPrison) // om inte i fängelse spela som normalt
                    {
                        if (Game.Dice1 == Game.Dice2)
                        {
                            p.amountOfDoubleDice += 1;
                            writeAsyncToAllPlayers(":Message:" + p.name + " slog lika:");
                        }
                        else
                            Game.Players.ElementAt(Game.NowPlaying).amountOfDoubleDice = 0;
                        if (p.amountOfDoubleDice > 2) // om man slagit lika 3 gånger i rad gå i fängelse
                        {
                            p.inPrison = true;
                            p.pocition = 10;

                            p.x = Game.Plates.ElementAt(p.pocition).x;
                            p.y = Game.Plates.ElementAt(p.pocition).y;

                            p.amountOfDoubleDice = 0;

                            writeAsyncToAllPlayers(":dice:" + Game.Dice1 + ":" + Game.Dice2 + ":" + p.x + ":" + p.y + ":" + p.name);
                            writeAsyncToAllPlayers(":Message:" + p.name + " skickad till fängele för att slå lika 3 gånger:");

                            //ger alla spelare tid att se vad den förgående splaren har gjort i deta fale bara flyttast till fengelset
                            Thread.Sleep(100);



                            nextPlayer();
                        } // om man inte går i fängelse spela normalt
                        else
                            play(p, Game.Dice1, Game.Dice2);
                    }
                    else
                    {
                        p.amountOfRoundsInPrison++;
                        if (Game.Dice1 == Game.Dice2)
                        {
                            p.amountOfRoundsInPrison = 0;
                            p.inPrison = false;
                            writeAsyncToAllPlayers(":Message:" + p.name + " slog lika och lämnade fängelset:");
                            play(p, Game.Dice1, Game.Dice2);
                        }
                        else if (p.amountOfEskipCards > 0)
                        {
                            p.amountOfEskipCards--;
                            p.inPrison = false;
                            writeAsyncToAllPlayers(":uppdateFrikort:" + p.amountOfEskipCards + ":" + p.name + ":");
                            writeAsyncToAllPlayers(":Message:" + p.name + " använde ett frikort för att lämna fängelset:");
                            p.amountOfRoundsInPrison = 0;
                            play(p, Game.Dice1, Game.Dice2);
                        }
                        else if (p.amountOfRoundsInPrison == 3)
                        {
                            p.money -= 50;
                            CheckMoney(p);
                            writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                            writeAsyncToAllPlayers(":Message:" + p.name + " tvungen att betala 50kr för att lämna fängelset:");
                            p.amountOfRoundsInPrison = 0;
                            p.inPrison = false;
                            play(p, Game.Dice1, Game.Dice2);
                        }
                        else
                        {
                            asyncWrite(p.client, ":payToGetOut:");
                            writeAsyncToAllPlayers(":dice:" + Game.Dice1 + ":" + Game.Dice2 + ":" + p.x + ":" + p.y + ":" + p.name);
                        }
                    }

                    i += 3;
                }
                else if (splitdata[i] == "bayHouse")
                {
                    int colorID = int.Parse(splitdata[i + 1]);

                    int PlateToAddHouseTo = 0; //id av den plattan som man ska addera ett hus till

                    House[] PlatesOfSameColor = new House[colorID == 1 || colorID == 8 ? 2 : 3];

                    //när man köper us måste man köpa dem hjempt på alla plattor och börjar på den första

                    int index = 0;
                    for (int j = 0; j < Game.Plates.Count; j++)
                    {
                        if (Game.Plates.ElementAt(j) is House)
                        {
                            if ((Game.Plates.ElementAt(j) as House).colorID == colorID)
                            {
                                PlatesOfSameColor[index] = (Game.Plates.ElementAt(j) as House);
                                index++;
                            }
                        }
                    }

                    //hämtar den plattan emd minsta natal hus
                    int min = PlatesOfSameColor[0].amountOfHouses; // första plattan
                    //gemför antal hus med alla andra plattor
                    for (int j = 0; j < PlatesOfSameColor.Length; j++)
                    {
                        if (PlatesOfSameColor[j].amountOfHouses < min) min = PlatesOfSameColor[j].amountOfHouses;
                    }

                    //lopar i genom frö att hitta det första plattan med den minsta antal hus(om flera plattor har samma antal hus)
                    for (int j = 0; j < PlatesOfSameColor.Length; j++)
                    {
                        if (PlatesOfSameColor[j].amountOfHouses == min)
                        {
                            PlateToAddHouseTo = j;
                            break;
                        }
                    }

                    //köper själva huset
                    if (PlatesOfSameColor[PlateToAddHouseTo].housePrise <= PlatesOfSameColor[PlateToAddHouseTo].Owner.money)
                    {
                        if (PlatesOfSameColor[PlateToAddHouseTo].amountOfHouses < 5)
                        {
                            PlatesOfSameColor[PlateToAddHouseTo].amountOfHouses++;
                            PlatesOfSameColor[PlateToAddHouseTo].Owner.money -= PlatesOfSameColor[PlateToAddHouseTo].housePrise;
                            writeAsyncToAllPlayers(":BayAHouse:" + PlatesOfSameColor[PlateToAddHouseTo].colorID + ":" + PlateToAddHouseTo + ":" + PlatesOfSameColor[PlateToAddHouseTo].amountOfHouses + ":");
                            CheckMoney(PlatesOfSameColor[PlateToAddHouseTo].Owner);
                            writeAsyncToAllPlayers(":uppdateMoney:" + PlatesOfSameColor[PlateToAddHouseTo].Owner.money + ":" + PlatesOfSameColor[PlateToAddHouseTo].Owner.name + ":");
                            writeAsyncToAllPlayers(":Message:" + PlatesOfSameColor[PlateToAddHouseTo].Owner.name + " köpte ett hus för " + PlatesOfSameColor[PlateToAddHouseTo].housePrise + "kr:");
                        }
                        else
                            writeAsyncToAllPlayers(":Message:" + PlatesOfSameColor[PlateToAddHouseTo].Owner.name + " försökte köpa ett hus men äger redan alla hus på den gatan:");
                    }
                    else// om splearen tin har råd till att köpa huset
                        writeAsyncToAllPlayers(":Message:" + PlatesOfSameColor[PlateToAddHouseTo].Owner.name + " försökte köpa ett hus men har inte tilrängligt med pengar:");

                }
                else if (splitdata[i] == "yesOutOfJail")
                {
                    Player p = getPlayerByName(splitdata[i + 1]);
                    p.inPrison = false;
                    p.money -= 50;
                    CheckMoney(p);
                    writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                    writeAsyncToAllPlayers(":Message:" + p.name + " betalade 50kr för att lämna fängelset:");
                    p.amountOfRoundsInPrison = 0;
                    play(p, Game.Dice1, Game.Dice2);
                }
                else if (splitdata[i] == "noOutOfJail")
                {
                    nextPlayer();
                }
                //köpa hus
                else if (splitdata[i] == "yesBayHouse")
                {
                    Player p = getPlayerByName(splitdata[i + 1]);
                    House h = (Game.Plates.ElementAt(p.pocition) as House);
                    p.money -= h.prise;
                    h.Owner = p;
                    p.amountOfPlatesOfEveryColor[h.colorID - 1]++;
                    if ((h.colorID == 1 || h.colorID == 8) && p.amountOfPlatesOfEveryColor[h.colorID - 1] == 2) asyncWrite(p.client, ":fullColor:" + h.colorID + ":"); //de två första och de två sista har endast två ruter för att bli full ferg
                    else if (p.amountOfPlatesOfEveryColor[h.colorID - 1] == 3) asyncWrite(p.client, ":fullColor:" + h.colorID + ":"); // 

                    CheckMoney(p);
                    writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                    writeAsyncToAllPlayers(":Message:" + p.name + " köpte " + h.Name + ":");
                    nextPlayer();
                }
                else if (splitdata[i] == "noBayHouse")
                {
                    nextPlayer();
                    //om jag har till lägger jag till action men nuvarande lir det bara nästa spelare
                }

                //köpa statligt werk
                else if (splitdata[i] == "yesBaySW")
                {
                    Player p = getPlayerByName(splitdata[i + 1]);
                    p.amountOfStateHouses++;
                    p.money -= (Game.Plates.ElementAt(p.pocition) as StateWork).prise;
                    (Game.Plates.ElementAt(p.pocition) as StateWork).Owner = p;
                    CheckMoney(p);
                    writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                    writeAsyncToAllPlayers(":Message:" + p.name + " köpte ett statligt verk:");
                    nextPlayer();
                }
                else if (splitdata[i] == "noBaySW")
                {
                    nextPlayer();
                    //om jag har till lägger jag till action men nuvarande lir det bara nästa spelare
                }

                //köpa tågstation
                else if (splitdata[i] == "yesBayTrainS")
                {
                    Player p = getPlayerByName(splitdata[i + 1]);
                    p.money -= (Game.Plates.ElementAt(p.pocition) as TrainStation).prise;
                    p.amountOfTrainStations++;
                    (Game.Plates.ElementAt(p.pocition) as TrainStation).Owner = p;
                    CheckMoney(p);
                    writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                    writeAsyncToAllPlayers(":Message:" + p.name + " köpte en tågstation:");
                    nextPlayer();
                }
                else if (splitdata[i] == "noBayTrainS")
                {
                    nextPlayer();
                    //om jag har till lägger jag till action men nuvarande lir det bara nästa spelare
                }
            } 
        }

        private void nextPlayer()
        {
            if (!(Game.Players.ElementAt(Game.NowPlaying).amountOfDoubleDice > 0)) //testar om så att spelaren inte har slagit några lika täningar
            {
                // om splearen slagit lika så kommer nowPlaying inte att ändras
                Game.NowPlaying++;
                if (Game.NowPlaying >= Game.Players.Count) Game.NowPlaying = 0;
                for (int i = 0; i < Game.Players.Count; i++)
                {
                    //hoppar över alla splerae som har förlorat och inte spelar längre
                    if (!Game.Players.ElementAt(Game.NowPlaying).Playing)
                    { 
                        nextPlayer();
                        break;
                    }
                }
            }
            writeAsyncToAllPlayers(":nowPlaying:" + Game.Players.ElementAt(Game.NowPlaying).name + ":");
            writeAsyncToAllPlayers(":Message:now playing " + Game.Players.ElementAt(Game.NowPlaying).name + ":");

        }

        private Player getPlayerByName(String name)
        {
            for (int i = 0; i < Game.Players.Count; i++)
                if (Game.Players.ElementAt(i).name == name) return Game.Players.ElementAt(i);
            return null;
        }

        private void play(Player p, int dice1, int dice2)
        {
            int newPlayerPos;
            if (dice2 == -1) //om man vill bestäma den ny positionen exakt sätter man dice2 till -1 och dice1 till den ny positionen
                newPlayerPos = dice1;
            else
                newPlayerPos = Game.Players.ElementAt(Game.NowPlaying).pocition + dice1 + dice2;

            //beräknar vart spelaren ska flytta 
            if (Game.Plates.Count < newPlayerPos) {//om spelaren korsar gå
                newPlayerPos -= Game.Plates.Count;
                p.money += 200;
                CheckMoney(p);
                writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                writeAsyncToAllPlayers(":Message:" + p.name + " fick 200kr för att gå över gå:");
            }
            p.pocition = newPlayerPos;
            p.x = Game.Plates.ElementAt(newPlayerPos).x;
            p.y = Game.Plates.ElementAt(newPlayerPos).y;
            int antal = 0; // antal spleare på samma platta
            //bestämmer positionen för varje spelare om det står flera på samma platta
            foreach (Player player in Game.Players)
                if (player != p)
                    if (p.x == player.x && p.y == player.y)
                    {
                        antal++;
                        if (antal == 1) p.x = Game.Plates.ElementAt(newPlayerPos).x + 20;
                        else if (antal == 2) p.x = Game.Plates.ElementAt(newPlayerPos).x - 20;
                        else if (antal == 3) p.x = Game.Plates.ElementAt(newPlayerPos).x + 40;
                    }

            writeAsyncToAllPlayers(":Message:" + p.name + " gick till platta " + newPlayerPos + ":");
            
            //skickar antalet som visas på terningarna till alla speare
            writeToAllPlayers(":dice:" + Game.Dice1 + ":" + Game.Dice2 + ":" + p.x + ":" + p.y + ":" + p.name);
            writeAsyncToAllPlayers(":Message:" + p.name + " slog " + Game.Dice1 + "," + Game.Dice2 + ":");

            //pausear för att låta alla spelare säkert får se vad som hänt
            Thread.Sleep(1000);

            //hämta plattan som man kommer fram till
            Plate standingOn = Game.Plates.ElementAt(newPlayerPos);

            //husplatta
            if (standingOn.Typ == 1)
            { // hus platta
                House plate = (standingOn as House);

                //vad som händer om man landar på en husplatta

                if (plate.Owner == null)
                {
                    if (p.money >= plate.prise)
                    {
                        // säger till clienten att fråga användaren om hen vill köpa tomten
                        normalWrite(p.client, ":köpaYES/NO:" + plate.prise + ":");
                        return;
                    }
                    else
                    {
                        //om spelaren som hamlar på rutan inte har tilääkligt med pengar går riuat nautomatiskt till aktion 
                        //(fixar om jag har tid brukar inte köra med det hema)
                        writeAsyncToAllPlayers(":Message:" + p.name + " har inte tillräkligt med pengar för att köpa plattan:");
                        nextPlayer();
                        return;
                    }
                }
                else if (plate.Owner != p)
                {
                    writeAsyncToAllPlayers(":Message:" + p.name + " måste betala " + plate.ToPay() + " till " + plate.Owner.name + " :");
                    pay(p, plate.Owner, plate.ToPay());
                }
                nextPlayer();
                return;
            }

            //kort
            else if (standingOn.Typ == 2)
            {
                // dra kort

                Card card = Game.DrawACard();
                
                writeAsyncToAllPlayers(":showChansMessage:" + card.Text + ":" + p.name + ":");
                writeAsyncToAllPlayers(":Message:" + p.name + " hamlade på chans:");
                writeAsyncToAllPlayers(":Message:" + p.name + " text på kortet " + card.Text + ":");

                if (card.ID == 1) // då får
                {
                    int money = card.Value; //vad du får

                    Thread.Sleep(1000);

                    p.money += money;
                    CheckMoney(p);
                    writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                    nextPlayer();
                }
                else if (card.ID == 2) //du ger
                {
                    int money = card.Value; //vad du får

                    Thread.Sleep(1000);

                    p.money -= money;
                    CheckMoney(p);
                    writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                    nextPlayer();
                }
                else if (card.ID == 3) // gå i fängelse
                {
                    p.amountOfRoundsInPrison++;
                    p.inPrison = true;

                    play(p, 10, -1);
                }
                else if (card.ID == 4) // ut ur fängelse
                {
                    p.amountOfEskipCards++;

                    Thread.Sleep(1000);

                    writeAsyncToAllPlayers(":uppdateAuteOfJail:" + p.amountOfEskipCards + ":" + p.name + ":");
                    nextPlayer();
                }
                else if (card.ID == 5) //ny pos
                {
                    int pos = card.Value; //value är lika med den positionen du ska flytta till

                    Thread.Sleep(1000);

                    play(p, pos, -1);
                }
                else if (card.ID == 6) // du ska flytta x steg fram
                {
                    int pos = card.Value; //de antal steg du ska fyltta

                    Thread.Sleep(1000);

                    play(p, pos, 0);
                }
                else if (card.ID == 7) // få av alla
                {
                    int money = card.Value; //pengana som alla ska ge till dig

                    for (int j = 0; j < Game.Players.Count; j++)
                    {
                        if(Game.Players.ElementAt(j) != p)
                        {
                            pay(Game.Players.ElementAt(j), p, money);
                        }
                    }

                    nextPlayer();
                }
                else if (card.ID == 8) // ge alla
                {
                    int money = card.Value; //pengana som alla ska ge till dig

                    for (int j = 0; j < Game.Players.Count; j++)
                    {
                        if (Game.Players.ElementAt(j) != p)
                        {
                            pay(p, Game.Players.ElementAt(j), money);
                        }
                    }

                    nextPlayer();
                }

                return;
            }

            //skuld
            else if (standingOn.Typ == 3)
            {
                writeAsyncToAllPlayers(":Message:" + p.name + " förlorade 100kr:");
                p.money -= standingOn.gre;
                CheckMoney(p);
                writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                nextPlayer();
            }

            //tågstation
            else if (standingOn.Typ == 4)
            {
                TrainStation plate = (standingOn as TrainStation);

                //vad som händer om man landar på en husplatta

                if (plate.Owner == null)
                {
                    if (p.money >= plate.prise)
                    {
                        // säger till clienten att fråga användaren om hen vill köpa tomten
                        normalWrite(p.client, ":köpaYES/NOTrain:" + plate.prise + ":");
                        return;
                    }
                    else
                    {
                        //om spelaren som hamlar på rutan inte har tilääkligt med pengar går riuat nautomatiskt till aktion 
                        //(fixar om jag har tid brukar inte köra med det hema)
                        writeAsyncToAllPlayers(":Message:" + p.name + " har inte tillräkligt med pengar för att köpa plattan:");
                        nextPlayer();
                        return;
                    }
                }
                else if (plate.Owner != p)
                {
                    pay(p, plate.Owner, plate.ToPay());
                    nextPlayer();
                    return;
                }

            }

            //bara på besök
            else if (standingOn.Typ == 5)
            {
                writeAsyncToAllPlayers(":Message:" + p.name + " hamlade på bara på besök:");
                nextPlayer();
            }

            //statlig verk
            else if (standingOn.Typ == 6)
            {
                StateWork plate = (standingOn as StateWork);

                //vad som händer om man landar på en husplatta

                if (plate.Owner == null)
                {
                    if (p.money >= plate.prise)
                    {
                        // säger till clienten att fråga användaren om hen vill köpa tomten
                        normalWrite(p.client, ":köpaYES/NOWork:" + plate.prise + ":");
                        return;
                    }
                    else
                    {
                        //om spelaren som hamlar på rutan inte har tilääkligt med pengar går riuat nautomatiskt till aktion 
                        //(fixar om jag har tid brukar inte köra med det hema)
                        writeAsyncToAllPlayers(":Message:" + p.name + " har inte tillräkligt med pengar för att köpa plattan:");
                        nextPlayer();
                        return;
                    }
                }
                else if (plate.Owner != p)
                {
                    pay(p, plate.Owner, plate.ToPay());
                    nextPlayer();
                    return;
                }
            }

            //fri pakering
            else if (standingOn.Typ == 7)
            {
                writeAsyncToAllPlayers(":Message:" + p.name + " hamlade på fri pakering:");
                nextPlayer();
            }
            
            //gå i fängelse
            else if (standingOn.Typ == 8)
            {
                writeAsyncToAllPlayers(":Message:" + p.name + " gick i fängelse:");
                p.inPrison = true;
                p.amountOfRoundsInPrison++;
                play(p, 10, -1);
                nextPlayer();
            }
            
            //gå
            else if (standingOn.Typ == 9)
            {
                writeAsyncToAllPlayers(":Message:" + p.name + " hittade 400kr:");
                p.money += 200; // sätter in 200 för att man landade på gå och sätter in två hundra för att man gått ett helt varv
                CheckMoney(p);
                writeAsyncToAllPlayers(":uppdateMoney:" + p.money + ":" + p.name + ":");
                nextPlayer();
            }
        }

        /*
         * det är p1 är som tapar pengar och p2 som känar pengar om p2 är null förlorar endast p1 penar ingen känar pengar  
         */ 
        private void pay(Player p1, Player p2, int amount)
        {
            //om spelare två är null betyder detta att man betalar pengar till staten(pengarna försviner i från spleare 1 bara)
            if (p2 == null)
            {
                p1.money -= amount;
                CheckMoney(p1);
                writeAsyncToAllPlayers(":uppdateMoney:" + p1.money + ":" + p1.name + ":");
                writeAsyncToAllPlayers(":Message:" + p1.name + " betalade" + amount + " till brädan:");
            }
            else
            {
                p1.money -= amount;
                p2.money += amount;
                CheckMoney(p1);
                CheckMoney(p2);
                writeAsyncToAllPlayers(":uppdateMoney:" + p1.money + ":" + p1.name + ":");
                writeAsyncToAllPlayers(":uppdateMoney:" + p2.money + ":" + p2.name + ":");
                writeAsyncToAllPlayers(":Message:" + p1.name + " betalade " + amount + " till " + p2.name + ":");
            }
        }

        /*
         * testar om spelaren har pengar kvar eller om hen har förlorat
         */ 
        private void CheckMoney(Player p)
        {
            //testar om spelaren har förlorat alla sina pengar
            if (p.money <= 0)
            {
                p.Playing = false;
                Game.AmountOfPlayersLeft--;
                writeAsyncToAllPlayers(":Message:" + p.name + " förlorade alla sinna pengar och är därför ute ur spelet:");
                writeAsyncToAllPlayers(":Lost:" + p.name + ":");
            }
            if (Game.AmountOfPlayersLeft == 1)
            {
                for (int i = 0; i < Game.Players.Count; i++)
                    if (Game.Players.ElementAt(i).Playing)
                    {
                        writeAsyncToAllPlayers(":Message:" + Game.Players.ElementAt(i).name + " van denna rundan:");
                        writeAsyncToAllPlayers(":Winner:" + Game.Players.ElementAt(i).name + ":");
                    }
            }
        }

    }
}
