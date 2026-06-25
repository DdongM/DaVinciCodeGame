
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

public partial class Form1 : Form
{
    TcpListener server;
    TcpClient client1, client2;
    List<Card> deck = new List<Card>();
    List<Card> p1Cards, p2Cards;

    public Form1()
    {
        InitializeComponent();
    }

    private void buttonStartServer_Click(object sender, EventArgs e)
    {
        server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Thread acceptThread = new Thread(AcceptClients);
        acceptThread.Start();
    }

    private void AcceptClients()
    {
        client1 = server.AcceptTcpClient();
        client2 = server.AcceptTcpClient();
        InitDeck();
        DealCards();
        Thread t1 = new Thread(() => HandleClient(client1, p2Cards, client2));
        Thread t2 = new Thread(() => HandleClient(client2, p1Cards, client1));
        t1.Start(); t2.Start();
    }

    private void InitDeck()
    {
        deck.Clear();
        for (int i = 0; i <= 11; i++)
        {
            deck.Add(new Card(i, "Black"));
            deck.Add(new Card(i, "White"));
        }
        deck = deck.OrderBy(x => Guid.NewGuid()).ToList();
    }

    private void DealCards()
    {
        p1Cards = deck.Take(6).ToList();
        p2Cards = deck.Skip(6).Take(6).ToList();
        Send(client1.GetStream(), "#CARDS#" + JsonSerializer.Serialize(p1Cards));
        Send(client2.GetStream(), "#CARDS#" + JsonSerializer.Serialize(p2Cards));
        Send(client1.GetStream(), "#TURN#");
        Send(client2.GetStream(), "#WAIT#");
    }

    private void HandleClient(TcpClient client, List<Card> opponentCards, TcpClient opponent)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        while (true)
        {
            int len = stream.Read(buffer, 0, buffer.Length);
            string msg = Encoding.UTF8.GetString(buffer, 0, len);
            if (msg.StartsWith("#GUESS#"))
            {
                string[] parts = msg.Substring(7).Split(',');
                int index = int.Parse(parts[0]);
                int guess = int.Parse(parts[1]);

                var target = opponentCards[index];
                if (target.Number == guess)
                {
                    target.IsOpen = true;
                    Send(stream, $"정답! 카드 {index} 공개");
                    Send(opponent.GetStream(), $"상대가 카드 {index} 맞춤");

                    if (opponentCards.All(c => c.IsOpen))
                    {
                        Send(stream, "🎉 당신이 승리했습니다!");
                        Send(opponent.GetStream(), "😢 당신이 패배했습니다.");
                        break;
                    }

                    Send(stream, "#TURN#");
                    Send(opponent.GetStream(), "#WAIT#");
                }
                else
                {
                    Send(stream, "틀렸습니다! 턴 넘김");
                    Send(opponent.GetStream(), "상대가 틀렸습니다. 당신의 턴입니다.");
                    Send(stream, "#WAIT#");
                    Send(opponent.GetStream(), "#TURN#");
                }
            }
        }
    }

    private void Send(NetworkStream stream, string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        stream.Write(data, 0, data.Length);
    }
}
