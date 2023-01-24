using Microsoft.VisualBasic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CinsAptClient
{
    public partial class Form1 : Form
    {
        private Socket client;
        private byte[] data = new byte[1024];
        private int size = 1024;
        private String daire;
        public Form1()
        {
            InitializeComponent();
            this.AcceptButton = button1;

        }
        //connect button
        private void button2_Click(object sender, EventArgs e)
        {
            daire = this.comboBox1.SelectedItem.ToString();
            this.button2.Enabled = false;
            Socket newsock = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9050);
            newsock.BeginConnect(iep, new AsyncCallback(Connected), newsock);

        }

        void Connected(IAsyncResult iar)
        {
            client = (Socket)iar.AsyncState;
            client.EndConnect(iar);
            this.listBox1.Items.Add("Cins Apartman toplantı sistemine hoş geldiniz.");
            client.BeginReceive(data, 0, size, SocketFlags.None,
            new AsyncCallback(ReceiveData), client);

        }

        void ReceiveData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int recv = remote.EndReceive(iar);
            string stringData = Encoding.Unicode.GetString(data, 0, recv);
            string firstWord = stringData.Split(' ')[0];
            //categorize if recieved message is currency message, card info message, wheather info message or chat message.
            if (firstWord == "+!#para#!+")
            {
                label3.Text = stringData.Split(' ')[1];//dolar
                label9.Text = stringData.Split(' ')[2];//gbp
                label4.Text = stringData.Split(' ')[3];//euro

            }
            else if (firstWord == "+!#kart#!+")
            {
                String[] cardinfo = stringData.Split(" ");
                cardinfo[0] = "";
                String info = string.Join(" ", cardinfo);
                this.listBox2.Items.Add(info);
            }
            else if (firstWord == "+!#hava#!+")
            {
                String[] weatherinfo = stringData.Split(" ");
                weatherinfo[0] = "";
                String info = string.Join(" ", weatherinfo);
                label6.Text = info;
            }
            else//if normal chat message
            {
                this.listBox1.Items.Add(stringData);
            }
            //call BeginRecieve again(any time a new message recieved) to make the client listen any more new messages.
            remote.BeginReceive(data, 0, size, SocketFlags.None,
            new AsyncCallback(ReceiveData), remote);
        }

        //send button
        private void button1_Click(object sender, EventArgs e)
        {
            String msg = this.textBox1.Text;
            msg = "(" + this.daire + ")" + ":" + msg;
            byte[] message = Encoding.Unicode.GetBytes(msg);
            this.textBox1.Clear();
            client.BeginSend(message, 0, message.Length, SocketFlags.None,
            new AsyncCallback(SendData), client);
        }
        void SendData(IAsyncResult iar)
        {
            Socket remote = (Socket)iar.AsyncState;
            int sent = remote.EndSend(iar);

        }
        //to preselect flat no 1 when the form loaded.
        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBox1.SelectedIndex = 0;
        }

        //to send messages with "Enter" key
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1.PerformClick();
            }
        }


        //form closing events handling
        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                client.Close();
            }

            if (e.CloseReason == CloseReason.WindowsShutDown)
            {
                client.Close();
            }

        }
    }
}