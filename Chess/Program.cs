﻿using Chess.Network;
using Chess.Pieces;
using System.Collections.Immutable;

namespace Chess
{
    internal class Program
    {
        public static int MoveNumber { get; set; } = 1;
        static void Main(string[] args)
        {
            while (true)
            {

                int mode;
                while (true)
                {
                    MoveNumber = 1;

                    Console.WriteLine("CHOOSE GAME MODE: \n ------------");
                    Console.WriteLine("1) offline multiplayer");
                    Console.WriteLine("2) solo against AI");
                    Console.WriteLine("3) online multiplayer");
                    Console.WriteLine("4) exit");
                    Console.WriteLine();

                    string? gamemode = Console.ReadLine();
                    if (gamemode == null || gamemode.Length != 1 || !char.IsDigit(gamemode[0]))
                    {
                        Console.Clear();
                        Console.WriteLine("Choose game mode (1-3)");
                        continue;
                    }
                    mode = gamemode[0] - '0';
                    if (mode > 4 || mode < 1)
                        continue;
                    break;
                }

                Console.Clear();
                switch (mode)
                {
                    case 1:
                        OfflineMultiplayer();
                        break;
                    case 2:
                        SoloAI();
                        break;
                    case 3:
                        OnlineMultiplayer();
                        break;
                    case 4:
                        return;
                    default:
                        throw new ArgumentException();
                }
                Console.Clear();
            }


        }
        private static List<Square> GetAllSquaresWithPiece(ImmutableDictionary<Square, IPiece> squares, bool white)
        {//TODO: smazat, to tu nemá být
            List<Square> pieceSquares = new List<Square>();

            foreach (Square square in squares.Keys)
            {
                if (squares[square] is not NoPiece && squares[square].IsWhite == white)
                {
                    pieceSquares.Add(square);
                }
            }
            return pieceSquares;
        }
        public static void OfflineMultiplayer()
        {
            Board board = new Board("1n6/p4k1p/2pq4/3P1p2/2Q2P2/2r5/P7/1K1RR3");

            while (true)
            {
                //List<Move> availableMoves = ChessRules.GetAvailableMoves(board.Squares, WhiteIsPlaying(), GetAllSquaresWithPiece(board.Squares, WhiteIsPlaying()));
                //foreach (var m in availableMoves)
                //{
                //    Console.WriteLine($"{board.Squares[m.From].Symbol} {m}");
                //}
                board.Print(WhiteIsPlaying());
                int evaluation = board.Evaluate(WhiteIsPlaying());
                //stalemate
                if (evaluation > 0)
                {
                    Console.WriteLine("DRAW");
                    break;
                }
                //loss
                else if (evaluation < 0)
                {
                    if (!WhiteIsPlaying())
                        Console.Write("WHITE ");
                    else
                        Console.Write("BLACK ");
                    Console.WriteLine("WON");
                    break;
                }

                if (WhiteIsPlaying())
                    Console.Write("White ");
                else
                    Console.Write("Black ");
                Console.WriteLine("is on the move");

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

            }
            Console.ReadLine();
        }
        public static void SoloAI()
        {
            bool playerIsWhite = true;// TODO: vybírání barvy
            Board board = new Board();
            ChessAI Zdenda = new ChessAI(!playerIsWhite);

            Move aiMove = new Move();

            while (true)
            {
                board.Print();
                int evaluation = board.Evaluate(WhiteIsPlaying());

                //stalemate
                if (evaluation > 0)
                {
                    Console.WriteLine("DRAW");
                    break;
                }
                //loss
                else if (evaluation < 0)
                {
                    Console.WriteLine("YOU ");
                    if (!playerIsWhite)
                        Console.Write("WON");
                    else
                        Console.Write("LOST");
                    Console.ReadLine();
                    break;
                }

                if (WhiteIsPlaying() == playerIsWhite)
                {
                    if (MoveNumber > 2)
                        Console.WriteLine($"Opponent's last move: {board.Squares[aiMove.From].Symbol} {aiMove}");
                    Console.WriteLine("You are on the move");
                    string? move = Console.ReadLine();

                    if (move == null || !board.TryMakeMove(move, WhiteIsPlaying()))
                    {
                        Console.Clear();
                        Console.WriteLine($"{move} is not a valid move!");
                        continue;
                    }
                }
                else
                {
                    Console.Write("Opponent is thinking of the best move");
                    aiMove = Zdenda.ChooseBestMove(board.GetSquares());
                    board.MakeMove(aiMove);

                }

                MoveNumber++;

                Console.Clear();

            }
        }
        public static void OnlineMultiplayer()
        {
            ChessServer server = new ChessServer();
            server.Run();
        }
        public static bool WhiteIsPlaying()
        {
            return MoveNumber % 2 != 0;
        }
    }
}
