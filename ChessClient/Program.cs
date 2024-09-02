using System.Net.Sockets;
using System.Net;
using System.Text;
using Chess;
namespace ChessClient
{
    internal class Program
    {
        internal class ChessClient
        {
            TcpClient client;
            Socket sender;
            public bool IsWhite;
            public ChessClient(int port = 11111, string ip = "localhost")
            {

                client = new TcpClient(ip, port);

                sender = client.Client;


            }
            public void Run()
            {
                try
                {
                    Console.WriteLine($"Connected to -> {sender.RemoteEndPoint} ");

                    byte[] messageReceived = new byte[1024];


                    int byteRecv = sender.Receive(messageReceived);
                    string serverMessage = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);

                    if (serverMessage == "W")
                        IsWhite = true;
                    else
                        IsWhite = false;

                    while (true)
                    {

                        messageReceived = new byte[1024];

                        // servers move
                        Console.WriteLine("Opponent's turn");
                        byteRecv = sender.Receive(messageReceived);
                        serverMessage = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);
                        string[] splitMessage = serverMessage.Split();
                        string fen = splitMessage[0];
                        
                        Console.Clear();
                        if (splitMessage.Length == 1)
                            PrintBoard(fen);
                        else
                            PrintBoard(fen, splitMessage[1]);

                        if (splitMessage.Length == 3)// length 3 -> end
                        {
                            PrintEndOfTheGame(splitMessage[2]);
                            break;

                        }


                        while (true)
                        {
                            // clients move
                            Console.Write("Your turn: ");
                            byte[] messageSent = Encoding.ASCII.GetBytes(Console.ReadLine());
                            int byteSent = sender.Send(messageSent);

                            // validation of clients move from the server
                            byteRecv = sender.Receive(messageReceived);
                            serverMessage = Encoding.ASCII.GetString(messageReceived, 0, byteRecv);


                            if (serverMessage != "invalid")
                            {
                                Console.Clear();
                                break;
                            }
                            else
                                Console.WriteLine("Invalid move");
                        }

                        splitMessage = serverMessage.Split();
                        fen = splitMessage[0];

                        Console.Clear();
                        PrintBoard(fen, splitMessage[1]);
                        if (splitMessage.Length == 3)
                        {
                            PrintEndOfTheGame(splitMessage[2]);
                            break;

                        }
                    }
                    Console.ReadLine();


                }

                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
                finally
                {
                    client.Close();
                }
            }

            private void PrintBoard(string fen, string? movedPiece = null)
            {

                int movedPieceSquareIndex = -1;
                if (movedPiece != null && IsWhite)
                    movedPieceSquareIndex = (char.ToLower(movedPiece[0]) - 'a' + 1) + (9 - int.Parse(movedPiece[1].ToString())-1) * 9;
                else if (movedPiece != null && !IsWhite)
                    movedPieceSquareIndex = (9 - (char.ToLower(movedPiece[0]) - 'a' + 1)) + ((int.Parse(movedPiece[1].ToString())-1)) * 9;
                if (!IsWhite)
                {// reverse fen
                    char[] charArray = fen.ToCharArray();
                    Array.Reverse(charArray);
                    fen = new string(charArray);
                }

                int squareIndex = 1;

                Console.WriteLine();
                if (!IsWhite)
                    Console.Write("1 ");
                else
                    Console.Write("8 ");
                for (int charIndex = 0; charIndex < fen.Length; charIndex++)
                {
                    char ch = fen[charIndex];
                    if (squareIndex % 2 != 0)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    if (char.IsDigit(ch))
                    {
                        int whiteSpaceCount = int.Parse(ch.ToString());
                        for (int i = whiteSpaceCount; i > 0; i--)
                        {
                            if (squareIndex % 2 != 0)
                            {
                                Console.BackgroundColor = ConsoleColor.Gray;
                                Console.ForegroundColor = ConsoleColor.Black;
                            }
                            else
                            {
                                Console.BackgroundColor = ConsoleColor.DarkMagenta;
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            Console.Write(" ", Console.BackgroundColor);
                            squareIndex++;

                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;

                            Console.Write(' ');
                        }
                    }
                    else if (char.IsLetter(ch))
                    {
                        if (squareIndex == movedPieceSquareIndex)
                        {
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        Console.Write(ch.ToString(), Console.BackgroundColor);
                        squareIndex++;
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(' ');

                    }
                    else if (ch == '/')
                    {
                        Console.WriteLine();

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;

                        if (!IsWhite)
                            Console.Write(squareIndex % 8 + 1);
                        else
                            Console.Write(8 - (squareIndex) / 8);
                        Console.Write(' ');
                        squareIndex++;
                    }

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    
                }
                Console.WriteLine();

                // column letters
                Console.Write("  ");
                for (int i = 0; i < 8; i++)
                {
                    if (IsWhite)
                        Console.Write((char)('a' + i));
                    else
                        Console.Write((char)('h' - i));
                    Console.Write(' ');
                }
                Console.WriteLine();
                Console.WriteLine("---------------------");
                Console.WriteLine();
            }
            private void PrintEndOfTheGame(string result)
            {
                if (result == "WIN")
                {
                    Console.WriteLine("YOU WON");
                }
                else if (result == "LOSS")
                {
                    Console.WriteLine("YOU LOST");
                }
                else if (result == "DRAW")
                {
                    Console.WriteLine("DRAW");
                }
            }
        }
        static void Main(string[] args)
        {
            Console.Write("IP to connect to: ");
            ChessClient client;
            
            try
            {
                client = new ChessClient(ip: Console.ReadLine());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }
            client.Run();
        }
    }
}
