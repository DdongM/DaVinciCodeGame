using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DavincicodeV._1
{
    public class OpponentCardSlot
    {
        public Card Card { get; set; }
        public bool Revealed { get; set; }
    }
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private List<Card> myCards = new List<Card>();
        private List<OpponentCardSlot> opponentSlots = new List<OpponentCardSlot>();
        private bool isMyTurn = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnConnectClient_Click(object sender, EventArgs e)
        {
            try
            {
                client = new TcpClient();
                client.Connect("172.20.10.2", 9999);
                stream = client.GetStream();

                AppendLog("서버에 연결되었습니다.");

                isMyTurn = false;
                btnGuess.Enabled = false;

                receiveThread = new Thread(ReceiveData);
                receiveThread.IsBackground = true;
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                AppendLog("연결 실패: " + ex.Message);
            }

        }

        private void ReceiveData()
        {
            try
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                while (true)
                {
                    string msg = reader.ReadLine();
                    AppendLog("수신 메시지: " + msg);

                    if (msg.StartsWith("MYCARD:"))
                    {
                        string data = msg.Substring("MYCARD:".Length);
                        var cards = ParseCards(data);
                        myCards = cards;
                        myCards.Sort((a, b) => a.Number.CompareTo(b.Number));

                        Invoke(new Action(() =>
                        {
                            DisplayCards(flowPlayerCards, myCards, false);
                            UpdateIndexCombobox();
                        }));

                        AppendLog("카드 수신 및 정렬 완료");
                    }
                    else if (msg.StartsWith("OPPCARD:"))
                    {
                        string data = msg.Substring("OPPCARD:".Length);
                        var newCards = ParseCards(data);

                        for (int i = 0; i < newCards.Count; i++)
                        {
                            var newCard = newCards[i];

                            if (i < opponentSlots.Count)
                            {
                                bool wasRevealed = opponentSlots[i].Revealed;
                                opponentSlots[i] = new OpponentCardSlot
                                {
                                    Card = newCard,
                                    Revealed = wasRevealed
                                };
                            }
                            else
                            {
                                opponentSlots.Add(new OpponentCardSlot
                                {
                                    Card = newCard,
                                    Revealed = false
                                });
                            }
                        }

                        Invoke(new Action(() =>
                        {
                            DisplayOpponentCards(opponentSlots);
                        }));
                        AppendLog("상대 카드 수신");
                    }
                    else if (msg.StartsWith("REVEAL:BY_GUESS:"))
                    {
                        string[] parts = msg.Substring("REVEAL:BY_GUESS:".Length).Split(':');
                        int idx = int.Parse(parts[0]);
                        string[] info = parts[1].Split('-');
                        string rawColor = info[0];
                        string color = (rawColor == "B") ? "Black" :
                                       (rawColor == "W") ? "White" : rawColor;
                        int number = (info[1] == "J") ? 13 : int.Parse(info[1]);

                        opponentSlots[idx].Card = new StandardCard(color, number);
                        opponentSlots[idx].Revealed = true;

                        Invoke(new Action(() =>
                        {
                            DisplayOpponentCards(opponentSlots);
                            AppendLog($"상대 카드 {idx}번 공개됨");

                            DialogResult result = MessageBox.Show(
                                "정답!\n계속 하겠습니까?",
                                "추리 성공",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question
                            );
                            if (result == DialogResult.Yes)
                            {
                                btnGuess.Enabled = true;
                                isMyTurn = true;
                            }
                            else
                            {
                                byte[] data = Encoding.UTF8.GetBytes("ENDTURN\n");
                                stream.Write(data, 0, data.Length);
                                btnGuess.Enabled = false;
                                isMyTurn = false;
                            }
                        }));
                    }
                    else if (msg.StartsWith("REVEAL:BY_FAIL:"))
                    {
                        string[] parts = msg.Substring("REVEAL:BY_FAIL:".Length).Split(':');
                        int idx = int.Parse(parts[0]);
                        string[] info = parts[1].Split('-');
                        string rawColor = info[0];
                        string color = (rawColor == "B") ? "Black" :
                                       (rawColor == "W") ? "White" : rawColor;
                        int number = (info[1] == "J") ? 13 : int.Parse(info[1]);

                        opponentSlots[idx].Card = new StandardCard(color, number);
                        opponentSlots[idx].Revealed = true;

                        Invoke(new Action(() =>
                        {
                            DisplayOpponentCards(opponentSlots);
                        }));
                        AppendLog($"(상대 추리 실패) 상대 카드 {idx}번 공개됨");
                    }
                    else if (msg.StartsWith("DRAW:"))
                    {
                        AppendLog("DRAW 메시지 수신: " + msg);
                        string cardInfo = msg.Substring("DRAW:".Length);
                        string[] info = cardInfo.Split('-');
                        string color = (info[0] == "B") ? "Black" : "White";
                        int number = (info[1] == "J") ? 13 : int.Parse(info[1]);
                        Card newCard = new StandardCard(color, number);

                        AppendLog("수신된 드로우 메시지: " + msg);

                        myCards.Add(newCard);
                        var sorted = myCards.Where(c => c.Number != 13).OrderBy(c => c.Number).ToList();
                        var jokers = myCards.Where(c => c.Number == 13).ToList();
                        myCards = sorted.Concat(jokers).ToList();

                        AppendLog("현재 내 카드 수: " + myCards.Count);

                        Invoke(new Action(() =>
                        {
                            DisplayCards(flowPlayerCards, myCards, false);
                            UpdateIndexCombobox();
                        }));
                        AppendLog("드로우 카드 추가 완료");
                    }
                    else if (msg == "FAIL")
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show("틀렸습니다! 상대 턴으로 넘어갑니다.", "추리 실패");
                            btnGuess.Enabled = false;
                            isMyTurn = false;
                        }));
                    }
                    else if (msg.StartsWith("LOG:"))
                    {
                        string logMessage = msg.Substring("LOG:".Length);
                        AppendLog(logMessage);
                    }
                    else if (msg == "TURN")
                    {
                        Invoke(new Action(() =>
                        {
                            isMyTurn = true;
                            btnGuess.Enabled = true;
                        }));
                        AppendLog("당신의 턴입니다!");
                    }
                    else if (msg == "NOT_YOUR_TURN")
                    {
                        AppendLog("아직 당신의 턴이 아닙니다.");
                    }
                    else if (msg == "WIN")
                    {
                        Invoke(new Action(() =>
                        {
                            btnGuess.Enabled = false;
                        }));
                        AppendLog("당신이 이겼습니다!");
                    }
                    else if (msg == "LOSE")
                    {
                        Invoke(new Action(() =>
                        {
                            btnGuess.Enabled = false;
                        }));
                        AppendLog("패배했습니다...");
                    }
                    else
                    {
                        AppendLog("알 수 없는 메시지: " + msg);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog("서버와 연결이 끊겼습니다: " + ex.Message);
            }
        }


        private List<Card> ParseCards(string data)
        {
            var cards = new List<Card>();
            string[] parts = data.Split(',');

            foreach (var part in parts)
            {
                string[] info = part.Split('-');
                if (info.Length != 2)
                {
                    throw new Exception("카드 정보 형식 오류: " + part);
                }

                string rawColor = info[0].Trim();
                string color;

                if (rawColor == "B" || rawColor == "Black")
                {
                    color = "Black";
                }
                else if (rawColor == "W" || rawColor == "White")
                {
                    color = "White";
                }
                else
                {
                    throw new Exception("❌ 알 수 없는 색상 코드: " + rawColor);
                }

                int number = (info[1] == "J") ? 13 : int.Parse(info[1]);

                cards.Add(new StandardCard(color, number));
            }

            return cards;
        }
        private List<OpponentCardSlot> ParseOpponentSlots(string data)
        {
            var slots = new List<OpponentCardSlot>();
            string[] parts = data.Split(',');

            foreach (var part in parts)
            {
                var info = part.Split('-');
                string color = (info[0] == "B") ? "Black" : "White";
                int number = (info[1] == "J") ? 13 : int.Parse(info[1]);

                slots.Add(new OpponentCardSlot
                {
                    Card = new StandardCard(color, number),
                    Revealed = false
                });
            }

            return slots;
        }


        private void DisplayCards(FlowLayoutPanel panel, List<Card> cards, bool isHidden)
        {
            panel.Controls.Clear();

            foreach (var card in cards)
            {
                Label lbl = new Label();
                lbl.Width = 60;
                lbl.Height = 90;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new Font("Segoe UI", 18, FontStyle.Bold);
                lbl.Margin = new Padding(5);
                lbl.BorderStyle = BorderStyle.FixedSingle;

                if (card.Color == "Black")
                {
                    lbl.BackColor = Color.Black;
                    lbl.ForeColor = Color.White;
                }
                else
                {
                    lbl.BackColor = Color.White;
                    lbl.ForeColor = Color.Black;
                }

                lbl.Text = isHidden ? "???" : (card.Number == 13 ? "J" : card.Number.ToString());

                panel.Controls.Add(lbl);
            }
        }

        private void AppendLog(string text)
        {
            if (txtLog.InvokeRequired)
                txtLog.Invoke(new Action(() => txtLog.AppendText($"{text}\r\n")));
            else
                txtLog.AppendText($"{text}\r\n");
        }


        private void btnGuess_Click(object sender, EventArgs e)
        {
            if (!isMyTurn)
            {
                MessageBox.Show("아직 당신의 턴이 아닙니다!");
                return;
            }

            if (cmbColor.SelectedItem == null || cmbNumber.SelectedItem == null || cmbIndex.SelectedItem == null)
            {
                MessageBox.Show("색상, 숫자, 위치를 모두 선택하세요!");
                return;
            }

            string color = cmbColor.SelectedItem.ToString();
            string number = cmbNumber.SelectedItem.ToString();
            string index = cmbIndex.SelectedItem.ToString();

            string colorCode = color == "Black" ? "B" : "W";
            string guess = $"GUESS:{colorCode}-{number}:{index}";


            try
            {
                byte[] data = Encoding.UTF8.GetBytes(guess + "\n");
                stream.Write(data, 0, data.Length);
                AppendLog($"추리 전송: {color}-{number} 위치 {index}");

                isMyTurn = false;
                btnGuess.Enabled = false;
            }
            catch (Exception ex)
            {
                AppendLog("전송 실패: " + ex.Message);
            }
        }

        private void DisplayOpponentCards(List<OpponentCardSlot> slots)
        {
            flowOpponentCards.Controls.Clear();

            foreach (var slot in slots.OrderBy(s => s.Card.Number))
            {
                Label lbl = new Label();
                lbl.Width = 60;
                lbl.Height = 90;
                lbl.TextAlign = ContentAlignment.MiddleCenter;
                lbl.Font = new Font("Segoe UI", 18, FontStyle.Bold);
                lbl.Margin = new Padding(5);
                lbl.BorderStyle = BorderStyle.FixedSingle;

                lbl.BackColor = slot.Card.Color == "Black" ? Color.Black : Color.White;
                lbl.ForeColor = slot.Card.Color == "Black" ? Color.White : Color.Black;

                lbl.Text = slot.Revealed ? (slot.Card.Number == 13 ? "J" : slot.Card.Number.ToString()) : "???";
                flowOpponentCards.Controls.Add(lbl);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            cmbColor.Items.Add("Black");
            cmbColor.Items.Add("White");

            for (int i = 0; i <= 12; i++)
            {
                cmbNumber.Items.Add(i.ToString());
            }
            cmbNumber.Items.Add("J");

            cmbIndex.Items.Clear();
            for (int i = 0; i < 4; i++)
            {
                cmbIndex.Items.Add(i.ToString());
            }
        }

        private void UpdateIndexCombobox()
        {
            cmbIndex.Items.Clear();
            for (int i = 0; i < myCards.Count; i++)
            {
                cmbIndex.Items.Add(i.ToString());
            }
        }
    }
}