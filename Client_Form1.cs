
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

public partial class Form1 : Form
{
    TcpClient client;
    NetworkStream stream;
    Thread receiveThread;
    List<Card> myCards = new List<Card>();

    public Form1()
    {
        InitializeComponent();
    }

    private void buttonConnect_Click(object sender, EventArgs e)
    {
        client = new TcpClient("127.0.0.1", 5000);
        stream = client.GetStream();
        receiveThread = new Thread(ReceiveMessage);
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveMessage()
    {
        byte[] buffer = new byte[4096];
        while (true)
        {
            int len = stream.Read(buffer, 0, buffer.Length);
            string msg = Encoding.UTF8.GetString(buffer, 0, len);

            if (msg.StartsWith("#CARDS#"))
            {
                string json = msg.Substring(7);
                myCards = JsonSerializer.Deserialize<List<Card>>(json);
                Invoke(new Action(DisplayCards));
            }
            else if (msg == "#TURN#")
                buttonGuess.Enabled = true;
            else if (msg == "#WAIT#")
                buttonGuess.Enabled = false;
            else
                Log(msg);
        }
    }

    private void DisplayCards()
    {
        panelCards.Controls.Clear();
        for (int i = 0; i < myCards.Count; i++)
        {
            var card = myCards[i];
            Button btn = new Button { Width = 80, Height = 40, Tag = i };
            btn.Text = card.IsOpen ? $"{card.Color} {card.Number}" : "???";
            btn.BackColor = card.Color == "Black" ? Color.Gray : Color.White;
            btn.Click += CardButton_Click;
            panelCards.Controls.Add(btn);
        }
    }

    private void CardButton_Click(object sender, EventArgs e)
    {
        var btn = sender as Button;
        textBoxIndex.Text = btn.Tag.ToString();
    }

    private void buttonGuess_Click(object sender, EventArgs e)
    {
        string msg = $"#GUESS#{textBoxIndex.Text},{textBoxGuess.Text}";
        byte[] data = Encoding.UTF8.GetBytes(msg);
        stream.Write(data, 0, data.Length);
    }

    private void Log(string msg)
    {
        if (InvokeRequired)
            Invoke(new Action(() => listBoxChat.Items.Add(msg)));
        else
            listBoxChat.Items.Add(msg);
    }
}
