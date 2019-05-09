using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Drawing;
using System.Net.Sockets;

namespace Monopol
{
    class MonopolyClient
    {
        TcpClient client;
        IPAddress ipAddress;
        int port;
        NetworkStream stream;
        private string myName;
        private MonopolyWindow form; // clienten ska kunna göra ändringar på spelarens form
        private List<string> names = new List<string>();

        public MonopolyClient(int port, string ipaddress, MonopolyWindow form)
        {
            this.port = port;
            this.form = form;
            ipAddress = IPAddress.Parse(ipaddress);
            client = new TcpClient();
            client.NoDelay = true;
            Game.GameRunning = true;
        }

        public async void startaAsyncKlient()
        {
            try
            {
                await client.ConnectAsync(ipAddress, port);
                stream = client.GetStream();
                myName = "{name}" + ((IPEndPoint)client.Client.LocalEndPoint).Port.ToString();
                startaAsyncSkrivning("newPlayer:" + myName);
                form.Text = "Monopoly: " + ((IPEndPoint)client.Client.LocalEndPoint).Port;
            }
            catch (Exception error) { MessageBox.Show(error.ToString()); return; }

            startAsyncMotagning();
        }

        public async void startaAsyncSkrivning(string message)
        {
            //varje gång man skriver något till servern läggs namnet(clienten som skickars namn) på i slutate av den data man skickar
            message += ":" + myName + ":";

            byte[] utData = Encoding.Unicode.GetBytes(message);

            try
            {
                await stream.WriteAsync(utData, 0, utData.Length);
            }
            catch (Exception error) { MessageBox.Show(error.ToString()); return; }

        }

        public async void startAsyncMotagning()
        {
            try
            {
                byte[] bytes = new byte[1028];
                string data = "";

                int inputData = await stream.ReadAsync(bytes, 0, bytes.Length);

                data = Encoding.Unicode.GetString(bytes, 0, inputData);

                parsData(data);

            }
            catch (Exception e) { Console.Write(e); return; }

            startAsyncMotagning();
        }

        public void close()
        {
            client.Close();
        }

        private void parsData(string data)
        {
            string[] splitdata = data.Split(':');

            for (int i = 0; i < splitdata.Length; i++)
            {
                //tar bort alla blanka stringar i splitdata arrayen
                if (splitdata[i] == "") continue;
                if (splitdata[i] == "uppdateConections")
                {
                    form.tbxPlayers.Text = "";
                    names = new List<string>();
                }
                else if (splitdata[i] == "newPlayer")
                {
                    names.Add(splitdata[i + 1]);
                    form.tbxPlayers.AppendText(splitdata[i + 1] + "\n");
                    i++;
                }
                else if (splitdata[i] == "closewindow")
                {
                    MessageBox.Show(splitdata[i + 1] + " left the game (spelet kommer att stngas ner för alla)");
                    Game.GameRunning = false; //säger till att spelet inte ska köras mera
                    form.Close(); //stänger ner skärmen
                    System.Environment.Exit(0);
                    i++;
                }
                else if (splitdata[i] == "startGame")
                {
                    form.panelGameLobby.Visible = false;
                    form.lblPnamn1.Text = names.ElementAt(0); form.ImgPlayer1.Visible = true;
                    if (names.Count > 1)
                    {
                        form.lblPnamn2.Text = names.ElementAt(1);
                        form.ImgPlayer2.Visible = true;
                    }
                    else form.lblPnamn3.Text = "null";
                    if (names.Count > 2)
                    {
                        form.ImgPlayer3.Visible = true;
                        form.lblPnamn3.Text = names.ElementAt(2);
                    }
                    else form.lblPnamn3.Text = "null";
                    if (names.Count > 3)
                    {
                        form.ImgPlayer4.Visible = true;
                        form.lblPnamn4.Text = names.ElementAt(3);
                    }
                    else form.lblPnamn4.Text = "null";


                }
                else if (splitdata[i] == "nowPlaying")
                {
                    String nextPlayer = splitdata[i + 1];

                    form.panelDieces.Visible = false;
                    form.lblDice1Res.Visible = false;
                    form.lblDice2Res.Visible = false;
                    form.lblPnamn1.ForeColor = Color.Black;
                    form.lblPnamn2.ForeColor = Color.Black;
                    form.lblPnamn3.ForeColor = Color.Black;
                    form.lblPnamn4.ForeColor = Color.Black;

                    if (myName == nextPlayer)
                    {
                        form.panelDieces.Visible = true;
                    }

                    if (names.ElementAt(0) == nextPlayer) form.lblPnamn1.ForeColor = Color.Green;
                    if (names.Count > 1 && names.ElementAt(1) == nextPlayer) form.lblPnamn2.ForeColor = Color.Green;
                    if (names.Count > 2 && names.ElementAt(2) == nextPlayer) form.lblPnamn3.ForeColor = Color.Green;
                    if (names.Count > 3 && names.ElementAt(3) == nextPlayer) form.lblPnamn4.ForeColor = Color.Green;
                }
                else if (splitdata[i] == "uppdateMoney")
                {
                    string money = splitdata[i + 1];

                    string name = splitdata[i + 2];

                    if (names.ElementAt(0) == name) form.lblPmoney1.Text = "Money: " + money;
                    if (names.Count > 1 && names.ElementAt(1) == name) form.lblPmoney2.Text = "Pengar: " + money;
                    if (names.Count > 2 && names.ElementAt(2) == name) form.lblPmoney3.Text = "Pengar: " + money;
                    if (names.Count > 3 && names.ElementAt(3) == name) form.lblPmoney4.Text = "Pengar: " + money;

                    i += 2;
                }
                else if (splitdata[i] == "showChansMessage")
                {
                    form.panelChansMessage.Visible = true;
                    form.lblText.Text = splitdata[i + 1];
                    form.lblCardOwner.Text = "Owner of this card is:" + splitdata[i + 2];

                    form.panelChansMessage.Visible = false;

                    i += 2;
                }
                else if (splitdata[i] == "uppdateAuteOfJail")
                {
                    string frikort = splitdata[i + 1];

                    string name = splitdata[i + 2];

                    if (names.ElementAt(0) == name) form.lblPfrikort1.Text = "Frikort: " + frikort;
                    if (names.Count > 1 && names.ElementAt(1) == name) form.lblPfrikort2.Text = "Frikort: " + frikort;
                    if (names.Count > 2 && names.ElementAt(2) == name) form.lblPfrikort3.Text = "Frikort: " + frikort;
                    if (names.Count > 3 && names.ElementAt(3) == name) form.lblPfrikort4.Text = "Frikort: " + frikort;

                    i += 2;
                }
                else if (splitdata[i] == "BayAHouse")
                {
                    int colorID = int.Parse(splitdata[i + 1]);
                    int id = int.Parse(splitdata[i + 2]); //id är den plattans som man är på id den färgen colorID = 5 id = 0 då är det den försat plattan i färggrup 5
                    int amountOfHouse = int.Parse(splitdata[i + 3]);

                    if (amountOfHouse != 5)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if ((form.HousePanels[colorID - 1, id].Tag as List<PictureBox>).ElementAt(j).Visible == false)
                            {
                                (form.HousePanels[colorID - 1, id].Tag as List<PictureBox>).ElementAt(j).Visible = true;
                                break;
                            }
                        }
                    }
                    else
                    { // om amountOfHouse är lika med 5 har man köpt ett hotel
                        for (int j = 0; j < 4; j++)
                        {
                            (form.HousePanels[colorID - 1, id].Tag as List<PictureBox>).ElementAt(j).BackColor = Color.Red;
                        }
                    }

                }
                else if (splitdata[i] == "dice")
                {
                    form.panelDieces.Visible = false;
                    form.lblDice1Res.Visible = true;
                    form.lblDice2Res.Visible = true;

                    int numberOnDice1 = int.Parse(splitdata[i + 1]);
                    int numberOnDice2 = int.Parse(splitdata[i + 2]);
                    int x = int.Parse(splitdata[i + 3]);
                    int y = int.Parse(splitdata[i + 4]);
                    string name = splitdata[i + 5];

                    form.lblDice1Res.Text = numberOnDice1.ToString();
                    form.lblDice2Res.Text = numberOnDice2.ToString();

                    if (names.ElementAt(0) == name) form.ImgPlayer1.Location = new System.Drawing.Point(x, y);
                    if (names.Count > 1 && names.ElementAt(1) == name) form.ImgPlayer2.Location = new System.Drawing.Point(x, y);
                    if (names.Count > 2 && names.ElementAt(2) == name) form.ImgPlayer3.Location = new System.Drawing.Point(x, y);
                    if (names.Count > 3 && names.ElementAt(3) == name) form.ImgPlayer4.Location = new System.Drawing.Point(x, y);

                    i += 5;
                }
                else if (splitdata[i] == "fullColor")
                {
                    int colorID = int.Parse(splitdata[i + 1]);

                    if (colorID == 1) form.btnBayHouse1.Enabled = true;
                    if (colorID == 2) form.btnBayHouse2.Enabled = true;
                    if (colorID == 3) form.btnBayHouse3.Enabled = true;
                    if (colorID == 4) form.btnBayHouse4.Enabled = true;
                    if (colorID == 5) form.btnBayHouse5.Enabled = true;
                    if (colorID == 6) form.btnBayHouse6.Enabled = true;
                    if (colorID == 7) form.btnBayHouse7.Enabled = true;
                    if (colorID == 8) form.btnBayHouse8.Enabled = true;
                    i += 1;
                }
                else if (splitdata[i] == "payToGetOut")
                {
                    form.panelDieces.Visible = false;

                    form.panelMessageJail.Visible = true;
                }
                else if (splitdata[i] == "köpaYES/NO")
                {
                    form.panelBayHouse.Visible = true;
                    form.MessageToShow = 0;

                    form.lblPrise.Text = "vill du köpa huset för " + splitdata[i + 1] + "kr";

                    i += 1;

                }
                else if (splitdata[i] == "köpaYES/NOWork")
                {
                    form.panelBayHouse.Visible = true;
                    form.MessageToShow = 1;

                    form.lblPrise.Text = "vill du köpa ett statligtverk för " + splitdata[i + 1] + "kr";

                    i += 1;

                }
                else if (splitdata[i] == "köpaYES/NOTrain")
                {
                    form.panelBayHouse.Visible = true;
                    form.MessageToShow = 2;

                    form.lblPrise.Text = "vill du köpa en tågstation för " + splitdata[i + 1] + "kr";

                    i += 1;

                }
                else if (splitdata[i] == "Message")
                {
                    string message = splitdata[i + 1];

                    string infoToDisplay = form.rtbInfo.Text;

                    infoToDisplay = message + "\n" + infoToDisplay;

                    form.rtbInfo.Text = infoToDisplay;

                }
                else if (splitdata[i] == "Winner")
                {
                    string name = splitdata[i + 1];
                    MessageBox.Show("vinnaren är " + name);
                    //stännger av spelet när någon har vunnit
                    startaAsyncSkrivning(":logingOut:");
                }
                else if (splitdata[i] == "Lost")
                {
                    string name = splitdata[i + 1];

                    if (names.ElementAt(0) == name) form.ImgPlayer1.Visible = false;
                    if (names.Count > 1 && names.ElementAt(1) == name) form.ImgPlayer2.Visible = false;
                    if (names.Count > 2 && names.ElementAt(2) == name) form.ImgPlayer3.Visible = false;
                    if (names.Count > 3 && names.ElementAt(3) == name) form.ImgPlayer4.Visible = false;

                }
            }

        }

    }

}
