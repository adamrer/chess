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
                        PrintBoard(fen, IsWhite);
                        if (splitMessage.Length == 2)
                        {
                            PrintEndOfTheGame(splitMessage[1]);
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
                        PrintBoard(fen, IsWhite);
                        if (splitMessage.Length == 2)
                        {
                            PrintEndOfTheGame(splitMessage[1]);
                            break;

                        }
                    }
                    Console.ReadLine();


                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }
                finally
                {
                    client.Close();
                }
            }

            private void PrintBoard(string fen, bool white)
            {//TODO: dont use Board
                Board board = new Board(fen);
                board.Print(white);
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
