// Šachy
// Adam Řeřicha, II. ročník
// letní semestr 2023/24
// Pokročilé programování v jazyce C# NPRG038


using Chess.Network;
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
                    Console.WriteLine("3) LAN multiplayer");
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
                        NetworkMultiplayer();
                        break;
                    case 4:
                        return;
                    default:
                        throw new ArgumentException();
                }
                Console.Clear();
            }


        }
        public static void OfflineMultiplayer()
        {
            Board board = new Board();
            string? move = null;
            while (true)
            {
                if (move == null)
                    board.Print(WhiteIsPlaying());
                else
                    board.Print(WhiteIsPlaying(), new Square(move[move.Length-1] - '0', (char)char.ToLower(move[move.Length - 2])));
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

                move = Console.ReadLine();

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
            bool playerIsWhite = true;
            string? move = null;

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

            }
            if (char.ToUpper(color[0]) == 'B')
                playerIsWhite = false;

            Board board = new Board();
            ChessAI Zdenda = new ChessAI(!playerIsWhite);

            Move aiMove = new Move();

            while (true)
            {
                if (move == null)
                    board.Print(playerIsWhite);
                else if (WhiteIsPlaying() == playerIsWhite)
                    board.Print(playerIsWhite, aiMove.To);
                else
                    board.Print(playerIsWhite, new Square(move[move.Length - 1] - '0', (char)char.ToLower(move[move.Length - 2])));

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
                    Console.Write("YOU ");
                    if (playerIsWhite != WhiteIsPlaying())
                        Console.WriteLine("WON");
                    else
                        Console.WriteLine("LOST");
                    Console.ReadLine();
                    break;
                }

                if (WhiteIsPlaying() == playerIsWhite)
                {
                    if (MoveNumber > 2)
                    Console.WriteLine("You are on the move");
                    move = Console.ReadLine();

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
                    aiMove = Zdenda.ChooseBestMove(board);
                    board.MakeMove(aiMove);

                }

                MoveNumber++;

                Console.Clear();

            }
        }
        public static void NetworkMultiplayer()
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
