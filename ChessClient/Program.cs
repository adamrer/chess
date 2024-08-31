using System.Net.Sockets;
using System.Net;
using System.Text;
using Chess;
using System.Collections.Immutable;
namespace ChessClient
{
    internal class Program
    {
        internal class ChessClient
        {
            Socket sender;
            IPEndPoint endPoint;
            public bool IsWhite;
            public ChessClient(int port = 11111, string? domain = null)
            {

                if (domain == null)
                    domain = Dns.GetHostName(); // localhost

                // Establish the remote endpoint for the socket.
                IPHostEntry ipHost;
                IPAddress ipAddr;

                ipHost = Dns.GetHostEntry(domain);
                ipAddr = ipHost.AddressList[0];

                endPoint = new IPEndPoint(ipAddr, port);

                sender = new Socket(ipAddr.AddressFamily,
                            SocketType.Stream, ProtocolType.Tcp);

            }
            public void Run()
            {
                try
                {

                    sender.Connect(endPoint);
                    Console.WriteLine($"Socket connected to -> {sender.RemoteEndPoint} ");

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

                            Console.Clear();

                            if (serverMessage != "invalid")
                                break;
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

                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException)
                {
                    Console.WriteLine("!!! Error connecting to server");
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
                finally
                {
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                }
            }

            private void PrintBoard(string fen, bool white)
            {//TODO: dont use Board
                Board board = new Board(fen);
                board.Print(white);
            }
            private void PrintEndOfTheGame(string resutl)
            {
                if (resutl == "WIN")
                {
                    Console.WriteLine("YOU WON");
                }
                else if (resutl == "LOSS")
                {
                    Console.WriteLine("YOU LOST");
                }
                else if (resutl == "DRAW")
                {
                    Console.WriteLine("DRAW");
                }
            }
        }
        static void Main(string[] args)
        {
            ChessClient client = new ChessClient();
            client.Run();
        }
    }
}
