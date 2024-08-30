using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel.Design;

namespace Chess.Network
{
    internal class ChessServer
    {
        Socket listener;
        IPEndPoint endPoint;
        public int MoveNumber { get; set; } = 1;
        public bool IsWhite { get; set; }
        Socket clientSocket = null;

        public ChessServer(int port = 11111, string? domain = null)
        {
            if (domain == null)
                domain = Dns.GetHostName(); // localhost

            // Establish the remote endpoint for the socket.
            IPHostEntry ipHost;
            IPAddress ipAddr;

            ipHost = Dns.GetHostEntry(domain);
            ipAddr = ipHost.AddressList[0];

            endPoint = new IPEndPoint(ipAddr, port);

            listener = new Socket(ipAddr.AddressFamily,
                       SocketType.Stream, ProtocolType.Tcp);
        }

        public void Run()
        {
            try
            {

                listener.Bind(endPoint);

                listener.Listen(1);

                Console.WriteLine("Waiting connection ... ");

                clientSocket = listener.Accept();
                
                // choosing color
                string? color;
                while (true)
                {
                    Console.Clear();
                    Console.Write("Choose color (W/B): ");
                    color = Console.ReadLine();
                    if (color != null && color.Length == 1 && 
                        (char.ToUpper(color[0]) == 'W' || char.ToUpper(color[0]) == 'B'))
                        break;
                    else if (color == "exit")
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        return;
                    }

                }
                if (char.ToUpper(color[0]) == 'W')
                    IsWhite = true;
                else
                    IsWhite = false;

                string opponentsColor = "";
                if (IsWhite)
                {
                    opponentsColor = "B";
                }
                else
                {
                    opponentsColor = "W";
                }

                SendToClient(opponentsColor);


                bool skipTurn = IsWhite == false;

                Board board = new Board();

                // playing
                while (true)
                {
                    board.Print(IsWhite);

                    if (!skipTurn)
                    {
                        if (PrintEvaluation(board))
                            break;

                        Console.Write("Your turn: ");
                        string? move = Console.ReadLine();

                        if (move != null && board.TryMakeMove(move, WhiteIsPlaying()))
                        {
                            MoveNumber++;
                        }
                        else
                        {
                            Console.Clear();
                            Console.WriteLine($"{move} is not a valid move!");
                            continue;
                        }
                    
                        Console.Clear();
                        board.Print(IsWhite);
                    }
                    skipTurn = false;

                    // send board
                    string response = board.GetFen();
                    SendToClient(response);

                    if (PrintEvaluation(board))
                        break;

                    Console.WriteLine("Opponent's move");
                    // receive move
                    byte[] bytes = new Byte[1024];
                    string opponentsMove = null;
                    int numByte = clientSocket.Receive(bytes);
                    opponentsMove = Encoding.ASCII.GetString(bytes, 0, numByte);
                    
                    while (!board.TryMakeMove(opponentsMove, WhiteIsPlaying()))
                    {
                        SendToClient("invalid");
                        numByte = clientSocket.Receive(bytes);
                        opponentsMove = Encoding.ASCII.GetString(bytes, 0, numByte);
                    }

                    MoveNumber++;
                    response = board.GetFen();
                    SendToClient(response);


                    Console.WriteLine($"Text received -> {opponentsMove}");
                    if (opponentsMove == "<EOC>")
                    {
                        break;
                    }
                    Console.Clear();
                }

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
        }
        private bool PrintEvaluation(Board board)
        {// false if the game ended
            int evaluation = board.Evaluate(WhiteIsPlaying());
            //stalemate
            if (evaluation > 0)
            {
                Console.WriteLine("DRAW");
                SendToClient(board.GetFen() + " DRAW");
                return true;
            }
            //loss
            else if (evaluation < 0)
            {
                Console.Write("YOU ");

                if (WhiteIsPlaying() != IsWhite)
                {
                    Console.WriteLine("WON");
                    SendToClient(board.GetFen() + " LOSS");
                }
                else
                {
                    Console.WriteLine("LOST");
                    SendToClient(board.GetFen() + " WIN");
                }
                Console.ReadLine();
                return true;
            }
            return false;
        }
        private void SendToClient(string message)
        {
            byte[] response = Encoding.ASCII.GetBytes(message);
            clientSocket.Send(response);
        }
        private bool WhiteIsPlaying()
        {
            return MoveNumber % 2 != 0;
        }
    }

}
