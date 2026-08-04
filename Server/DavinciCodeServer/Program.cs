using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics.Eventing.Reader;

namespace DavinciCodeServer
{
    internal class Program
    {
        static TcpListener listener;
        static TcpClient player1Client, player2Client;
        static NetworkStream stream1, stream2;
        static List<Card> player1Cards, player2Cards;
        static bool[] p1Revealed;
        static bool[] p2Revealed;
        static int currentTurn = 1;
        static List<Card> deck;
        static StreamWriter writer1, writer2;
        static int player1LastDrawIndex = -1;
        static int player2LastDrawIndex = -1;
        
        static void Main(string[] args)
        {
            Console.WriteLine("다빈치 코드 서버 시작 중...");
            listener = new TcpListener(IPAddress.Any, 9999);
            listener.Start();

            Console.WriteLine("클라이언트 2명 접속 대기 중...");

            player1Client = listener.AcceptTcpClient();
            Console.WriteLine("Player 1 접속됨");
            stream1 = player1Client.GetStream();

            player2Client = listener.AcceptTcpClient();
            Console.WriteLine("Player 2 접속됨");
            stream2 = player2Client.GetStream();

            deck = CardManager.GenerateDeck();
            player1Cards = CardManager.DrawCards(deck, 4);
            player2Cards = CardManager.DrawCards(deck, 4);

            p1Revealed = new bool[player1Cards.Count];
            p2Revealed = new bool[player2Cards.Count];

            player1Cards.Sort((a, b) => a.Number.CompareTo(b.Number));
            player2Cards.Sort((a, b) => a.Number.CompareTo(b.Number));

            writer1 = new StreamWriter(stream1, Encoding.UTF8) { AutoFlush = true };
            writer2 = new StreamWriter(stream2, Encoding.UTF8) { AutoFlush = true };

            DrawAndStartTurn(1);

            writer1.WriteLine("MYCARD:" + FormatCardList(player1Cards));
            writer2.WriteLine("MYCARD:" + FormatCardList(player2Cards));

            writer1.WriteLine("OPPCARD:" + FormatCardList(player2Cards));
            writer2.WriteLine("OPPCARD:" + FormatCardList(player1Cards));



         

            StreamReader reader1 = new StreamReader(stream1, Encoding.UTF8);
            StreamReader reader2 = new StreamReader(stream2, Encoding.UTF8);

            new Thread(() => HandleGuess(reader1, writer1, writer2, player2Cards, p2Revealed, 1)).Start();
            new Thread(() => HandleGuess(reader2, writer2, writer1, player1Cards, p1Revealed, 2)).Start();

            Console.WriteLine("카드 전송 완료!");
            Console.WriteLine("엔터 입력 시 서버 종료");
            Console.ReadLine();
            Cleanup();
        }

        static string FormatCardList(List<Card> cards)
        {
            List<string> formatted = new List<string>();
            foreach (var card in cards)
            {
                formatted.Add(card.Display());
            }
            return string.Join(",", formatted);
        }

        static void Cleanup()
        {
            stream1?.Close();
            stream2?.Close();
            player1Client?.Close();
            player2Client?.Close();
            listener?.Stop();
            Console.WriteLine("서버 종료됨");
        }

        static void HandleGuess(
            StreamReader reader, StreamWriter writerSelf, StreamWriter writerOpponent,
            List<Card> opponentCards, bool[] revealedCards, int myPlayerNum)
        {
            try
            {
                while (true)
                {
                    string msg = reader.ReadLine();
                    Console.WriteLine($"Player {myPlayerNum} 추리: {msg}");

                    if (msg.StartsWith("GUESS:"))
                    {
                        if (currentTurn != myPlayerNum)
                        {
                            writerSelf.WriteLine("NOT_YOUR_TURN");
                            continue;
                        }

                        string[] parts = msg.Substring("GUESS:".Length).Split(':');
                        string[] info = parts[0].Split('-');

                        string guessColor = info[0].Trim().ToUpper();
                        if (guessColor == "B" || guessColor == "BLACK") guessColor = "Black";
                        else if (guessColor == "W" || guessColor == "WHITE") guessColor = "White";

                        string numberPart = info[1].Trim().ToUpper();
                        int guessNumber = (numberPart == "J") ? 13 : int.Parse(numberPart);
                        int guessIndex = int.Parse(parts[1]);

                        Card target = opponentCards[guessIndex];
                        string targetColor = target.Color.Trim().ToUpper();
                        int targetNumber = target.Number;

                        Console.WriteLine($"[DEBUG] 상대 카드: {targetColor}-{targetNumber}, 내가 보낸 추리: {guessColor}-{guessNumber}");

                        if (target.Color == guessColor && target.Number == guessNumber)
                        {
                            revealedCards[guessIndex] = true;
                            string revealMsg = $"REVEAL:BY_GUESS:{guessIndex}:{target.Display()}";
                            writerSelf.WriteLine(revealMsg);
                            Console.WriteLine($"Player {myPlayerNum} 정답 → {revealMsg}");

                            if (AllRevealed(revealedCards))
                            {
                                writerSelf.WriteLine("WIN");
                                writerOpponent.WriteLine("LOSE");                                
                                return;
                            }

                            Console.WriteLine($"[DEBUG] 상대 카드: {target.Display()}, 내가 보낸 추리: {guessColor}-{guessNumber}");
                            // 턴 유지
                        }
                        else
                        {
                            Console.WriteLine($"Player {myPlayerNum} 오답");

                            writerSelf.WriteLine("FAIL");

                            // 공개 메시지 전송
                            int revealIndex = (myPlayerNum == 1) ? player1LastDrawIndex : player2LastDrawIndex;
                            Card revealCard = (myPlayerNum == 1) ? player1Cards[revealIndex] : player2Cards[revealIndex];

                            string revealMsg = $"REVEAL:BY_FAIL:{revealIndex}:{revealCard.Display()}";
                            writerOpponent.WriteLine(revealMsg);  // 상대에게 보여줘야 하므로 opponent에게 보냄

                            // 턴 넘김
                            currentTurn = (myPlayerNum == 1) ? 2 : 1;
                            DrawAndStartTurn(currentTurn);
                        }

                    }
                    else if (msg == "ENDTURN")
                    {
                        Console.WriteLine($"Player {myPlayerNum}가 턴을 종료했습니다.");
                        currentTurn = (myPlayerNum == 1) ? 2 : 1;

                        DrawAndStartTurn(currentTurn);
                    }
                    else if (msg == "FAIL")
                    {
                        Console.WriteLine($"Player {myPlayerNum} 오답 → 턴 넘김");
                        currentTurn = (myPlayerNum == 1) ? 2 : 1;
                        DrawAndStartTurn(currentTurn);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Player {myPlayerNum} 에러: {ex.Message}");
            }
        }

        static void DrawAndStartTurn(int playerNum)
        { 

            Console.WriteLine("DrawAndStartTurn() 호출됨 - Player " + playerNum);

            List<Card> targetHand = (playerNum == 1) ? player1Cards : player2Cards;
            StreamWriter writer = (playerNum == 1) ? writer1 : writer2;
            Console.WriteLine($"[DEBUG] 덱 남은 수: {deck.Count}");
            Console.WriteLine($"[DEBUG] Player {playerNum} 핸드: {string.Join(",", targetHand.Select(c => c.Display()))}");

            if (deck.Count > 0)
            {

                
                Console.WriteLine("현재 핸드: " + string.Join(",", targetHand.Select(c => c.Display())));
                Console.WriteLine("덱 상태: " + string.Join(",", deck.Select(c => c.Display())));

                var drawResult = CardManager.DrawCards(deck, 1, targetHand);

                Console.WriteLine("Draw 가능 카드 수: " + drawResult.Count);
                int lastDrawIndex = -1;

                if (drawResult.Count > 0)
                {
                    Card drawn = drawResult.First();
                    targetHand.Add(drawn);

                    if (playerNum == 1)
                        player1LastDrawIndex = targetHand.Count - 1;
                    else
                        player2LastDrawIndex = targetHand.Count - 1;
                        Console.WriteLine("DRAW 메시지 전송됨: " + drawn.Display());
                    writer.WriteLine("DRAW:" + drawn.Display());
                    string opponentCardList = FormatCardList(targetHand);
                    if (playerNum == 1)
                        writer2.WriteLine("OPPCARD:" + opponentCardList);  
                    else
                        writer1.WriteLine("OPPCARD:" + opponentCardList);
                    Console.WriteLine($"Player {playerNum} 카드 드로우: {drawn.Display()}");
                }
                else
                {
                    Console.WriteLine($"Player {playerNum} 드로우 실패: 겹치지 않는 카드 없음");
                    writer.WriteLine("LOG:겹치지 않는 카드가 없어 드로우하지 못했습니다.");
                }
            }
            else
            {
                Console.WriteLine("덱이 비었습니다.");
                writer.WriteLine("LOG:덱이 비어 더 이상 드로우할 수 없습니다.");
            }

            // 턴 메시지 전송
            writer.WriteLine("TURN");
            Console.WriteLine($"Player {playerNum} 턴 시작");
        }


        static bool AllRevealed(bool[] revealed)
        {
            foreach (bool r in revealed)
                if (!r) return false;
            return true;
        }

       
    }
}
