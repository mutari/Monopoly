using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

namespace Monopol
{
    public partial class MonopolyWindow : Form
    {
        MonopolyServer server;
        MonopolyClient client;

        private bool InfoDisplayOpen = false;
        public int MessageToShow = 0; //denna bestämemr om man ska vissa en message ruta för att köpa hus, statligt verk eller tågstation
        public Panel[,] HousePanels = new Panel[8,3];

        public MonopolyWindow()
        {
            InitializeComponent();

            //ritar alla hus paneler
            //hus panelerna visar vilka hus man köpt/äger
            for (int i = 0; i < 8; i++)
            {
                createNewHouseMark(3 + (46 * 0), 3 + (47 * i), i, 0);
                createNewHouseMark(3 + (46 * 1), 3 + (47 * i), i, 1);
                createNewHouseMark(3 + (46 * 2), 3 + (47 * i), i, 2);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            server = new MonopolyServer(8080);
            server.startaServer();
            server.asyncListener();
            panelServerMeny.Visible = false;
            panelGameLobby.Visible = true;

            //coplar upp till serverna via en client som körs via samma program
            client = new MonopolyClient(8080, "127.0.0.1", this);
            client.startaAsyncKlient();

            //ladda in alla plattorna i från filen PlattInfo.conf och spara dem på servern
            Game.LoadPlates();

            //ladda in all kort
            Game.LoadCards();
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            //startar en client
            client = new MonopolyClient(8080, "127.0.0.1", this);
            client.startaAsyncKlient();
            panelServerMeny.Visible = false;
            panelGameLobby.Visible = true;
            btnGameStart.Enabled = false;
        }

        //server side
        private void btnGameStart_Click(object sender, EventArgs e)
        {
            if (Game.Players.Count > 1 && Game.Players.Count <= 4) // man måste vara mer än 1 spelare för att starta
            {
                server.writeAsyncToAllPlayers(":startGame:");
                startGame();
            }
            else
                MessageBox.Show("minst 2 och max 4 spelare");
        }

        //client side
        private void btnThrowDices_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            
            int number1 = r.Next(6) + 1;
            int number2 = r.Next(6) + 1;

            /* för att testa, ger en möjligheten att välja tärning själv
            string value = "";
            InputBox("title", "num1", ref value);
            int number1 = int.Parse(value);
            InputBox("title", "num2", ref value);
            int number2 = int.Parse(value);
            */

            client.startaAsyncSkrivning(":dice:" + number1 + ":" + number2);
            //client.startaAsyncSkrivning(":dice:" + 1 + ":" + 0);

        }

        //server side
        private void startGame()
        {
            Random r = new Random();
            int startPlayer = r.Next(Game.Players.Count);
            Game.NowPlaying = startPlayer;
            server.writeAsyncToAllPlayers(":nowPlaying:" + Game.Players.ElementAt(Game.NowPlaying).name + ":");
        }

        //skapar alla paneler för att displaya antalet hus man köpt
        private void createNewHouseMark(int x, int y, int SaveX, int SaveY)
        {
            System.Windows.Forms.Panel PanelHouse;
            System.Windows.Forms.PictureBox ImgHouse1;
            System.Windows.Forms.PictureBox ImgHouse2;
            System.Windows.Forms.PictureBox ImgHouse3;
            System.Windows.Forms.PictureBox ImgHouse4;

            PanelHouse = new System.Windows.Forms.Panel();
            ImgHouse1 = new System.Windows.Forms.PictureBox();
            ImgHouse2 = new System.Windows.Forms.PictureBox();
            ImgHouse3 = new System.Windows.Forms.PictureBox();
            ImgHouse4 = new System.Windows.Forms.PictureBox();
            // 
            // PanelHouse
            // 
            PanelHouse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            PanelHouse.Location = new System.Drawing.Point(x, y);//3, 47
            PanelHouse.Name = "PanelHouse";
            PanelHouse.Size = new System.Drawing.Size(39, 22);
            // 
            // ImgHouse
            // 
            ImgHouse1.BackColor = System.Drawing.Color.LimeGreen;
            ImgHouse1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            ImgHouse1.Location = new System.Drawing.Point(2, 2);
            ImgHouse1.Name = "ImgHouse";
            ImgHouse1.Size = new System.Drawing.Size(10, 18);
            ImgHouse1.TabIndex = 23;
            ImgHouse1.TabStop = false;
            ImgHouse1.Visible = false;

            ImgHouse2.BackColor = System.Drawing.Color.LimeGreen;
            ImgHouse2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            ImgHouse2.Location = new System.Drawing.Point(10, 2);
            ImgHouse2.Name = "ImgHouse";
            ImgHouse2.Size = new System.Drawing.Size(10, 18);
            ImgHouse2.TabIndex = 23;
            ImgHouse2.TabStop = false;
            ImgHouse2.Visible = false;

            ImgHouse3.BackColor = System.Drawing.Color.LimeGreen;
            ImgHouse3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            ImgHouse3.Location = new System.Drawing.Point(18, 2);
            ImgHouse3.Name = "ImgHouse";
            ImgHouse3.Size = new System.Drawing.Size(10, 18);
            ImgHouse3.TabIndex = 23;
            ImgHouse3.TabStop = false;
            ImgHouse3.Visible = false;

            ImgHouse4.BackColor = System.Drawing.Color.LimeGreen;
            ImgHouse4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            ImgHouse4.Location = new System.Drawing.Point(26, 2);
            ImgHouse4.Name = "ImgHouse";
            ImgHouse4.Size = new System.Drawing.Size(10, 18);
            ImgHouse4.TabIndex = 23;
            ImgHouse4.TabStop = false;
            ImgHouse4.Visible = false;

            PanelHouse.Controls.Add(ImgHouse1);
            PanelHouse.Controls.Add(ImgHouse2);
            PanelHouse.Controls.Add(ImgHouse3);
            PanelHouse.Controls.Add(ImgHouse4);
            PanelHouse.Tag = new List<PictureBox> {ImgHouse1, ImgHouse2, ImgHouse3, ImgHouse4}; 

            HousePanels[SaveX,SaveY] = PanelHouse;
            this.PanelHouseBayer.Controls.Add(PanelHouse);

        }
        
        //all down client side
        private void btnYES_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":yesOutOfJail");
            panelMessageJail.Visible = false;
        }

        private void btnNO_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":noOutOfJail:");
            panelMessageJail.Visible = false;
        }

        private void btnBayHouseYes_Click(object sender, EventArgs e)
        {
            if (MessageToShow == 0)
                client.startaAsyncSkrivning(":yesBayHouse");
            else if (MessageToShow == 1)
                client.startaAsyncSkrivning(":yesBaySW");
            else if (MessageToShow == 2)
                client.startaAsyncSkrivning(":yesBayTrainS");
            panelBayHouse.Visible = false;
        }

        private void btnBayHouseNo_Click(object sender, EventArgs e)
        {
            if(MessageToShow == 0)
                client.startaAsyncSkrivning(":noBayHouse:");
            else if(MessageToShow == 1)
                client.startaAsyncSkrivning(":noBaySW:");
            else if(MessageToShow == 2)
                client.startaAsyncSkrivning(":noBayTrainS:");
            panelBayHouse.Visible = false;
        }

        private void btnOpenInfoDisplay_Click(object sender, EventArgs e)
        {
            if (InfoDisplayOpen)
            {
                rtbInfo.Size = new System.Drawing.Size(rtbInfo.Size.Width, 19);
                btnOpenInfoDisplay.Text = "\\/";
                InfoDisplayOpen = false;
            }
            else
            {
                rtbInfo.Size = new System.Drawing.Size(rtbInfo.Size.Width, 200);
                btnOpenInfoDisplay.Text = "/\\";
                InfoDisplayOpen = true;
            }
        }

        private void btnBayHouse1_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:1:");
        }

        private void btnBayHouse2_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:2:");
        }

        private void btnBayHouse3_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:3:");
        }

        private void btnBayHouse4_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:4:");
        }

        private void btnBayHouse5_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:5:");
        }

        private void btnBayHouse6_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:6:");
        }

        private void btnBayHouse7_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:7:");
        }

        private void btnBayHouse8_Click(object sender, EventArgs e)
        {
            client.startaAsyncSkrivning(":bayHouse:8:");
        }

        private void MonopolyWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Game.GameRunning)
            {
                e.Cancel = true;
                client.startaAsyncSkrivning(":logingOut:");
            }
            else
            {
                try
                {
                    client.close();
                }
                catch (Exception) { } //om clienten inte är skapad redan fångar jag upp errort
            }
        }

        //basic input dialog
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

    }
}
