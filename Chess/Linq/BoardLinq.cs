using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Linq
{
    internal class BoardLinq
    {
        int Width { get; init; } = 8;
        int Height { get; init; } = 8;
        IEnumerable<SquareWithPiece> Squares { get; init; }

        public BoardLinq()
        {
            // standard 8x8 board with standard pieces

            List<SquareWithPiece> squares = new List<SquareWithPiece>();
            for (int i = 1; i < Height + 1; i++)
            {
                for (int j = 1; j < Width + 1; j++)
                {
                    // pawns
                    if (i == 2) squares.Add(new SquareWithPiece(new Pawn(), i, j));
                    else if (i == 7) squares.Add(new SquareWithPiece(new Pawn(false), i, j));
                    // rooks
                    else if (i == 1 && (j == 1 || j == 8)) squares.Add(new SquareWithPiece(new Rook(), i, j));
                    else if (i == 8 && (j == 1 || j == 8)) squares.Add(new SquareWithPiece(new Rook(false), i, j));
                    // knights
                    else if (i == 1 && (j == 2 || j == 7)) squares.Add(new SquareWithPiece(new Knight(), i, j));
                    else if (i == 8 && (j == 2 || j == 7)) squares.Add(new SquareWithPiece(new Knight(false), i, j));
                    // bishops
                    else if (i == 1 && (j == 3 || j == 6)) squares.Add(new SquareWithPiece(new Bishop(), i, j));
                    else if (i == 8 && (j == 3 || j == 6)) squares.Add(new SquareWithPiece(new Bishop(false), i, j));
                    // queens
                    else if (i == 1 && j == 4) squares.Add(new SquareWithPiece(new Queen(), i, j));
                    else if (i == 8 && j == 4) squares.Add(new SquareWithPiece(new Queen(false), i, j));
                    // kings
                    else if (i == 1 && j == 5)
                        squares.Add(new SquareWithPiece(new King(), i, j));
                    else if (i == 8 && j == 5)
                        squares.Add(new SquareWithPiece(new King(false), i, j));
                    // empty squares
                    else squares.Add(new SquareWithPiece(new NoPiece(), i, j));

                }

            }
            squares.Sort(new SquareComparer());
            Squares = squares;
        }

        private MoveLinq StringToMove(string text, bool whitePlaying)
        {
            // move: [*piece_symbol*][*row_or_column*][destination_square]
            // find with LINQ, if there is ambiguity, then not valid move. Tell the user, that it's ambiguous


            // find square that has a piece symbol from text
            // if text doesn't have symbol, then find pawn, that can go to the square

            if (text.Length < 2 || text.Length > 3)
                throw new ArgumentException();


            char pieceSymbol;

            if (text.Length == 2)
                pieceSymbol = 'p';
            else
                pieceSymbol = text[0];

            SquareWithPiece destination = GetSquare(text.Substring(text.Length - 2));

            var startingSquares = Squares.Where(s => char.ToLower(s.Piece.Symbol) == char.ToLower(pieceSymbol))
                                .Where(s => s.Piece.IsWhite == whitePlaying)
                                .Where(s => GetAvailableMoves(s, whitePlaying).Contains(destination));
            // kde se rovná symbol a figurka může jít na ten square

            int startingSquaresCount = startingSquares.Count();
            if (startingSquaresCount > 1)
            {
                if (text.Length <= 3)
                    throw new ArgumentException();

            }
            else if (startingSquaresCount == 0)
                throw new ArgumentException();

            return new MoveLinq(startingSquares.First(), destination);
        }
        private IEnumerable<SquareWithPiece> GetAvailableMoves(SquareWithPiece square, bool whitePlaying)
        {
            // postupně procházím políčka a kontroluji, jestli jsou validní (nejsou mimo šachovnici a nejsou za jinou figurkou nebo na jiné figurce)

            Dictionary<(int, int), bool> directionPermissions = new Dictionary<(int, int), bool>();

            foreach ((int, int) vector in square.Piece.MoveVectors)
            {
                // piece can go this direction
                int differentDirectionsCounter = 0;
                (int, int) directionVector = vector;
                foreach (var direction in directionPermissions)
                {
                    if (AreSameDirection(direction.Key, vector) != null)
                    {
                        directionVector = direction.Key;
                        break;
                    }
                    differentDirectionsCounter++;
                }
                if (directionPermissions.Count == 0 || differentDirectionsCounter == directionPermissions.Count) // new direction found
                    directionPermissions.Add(vector, true);

                SquareWithPiece? availableMove = GetAvailableSquareFrom(square, vector, directionVector, whitePlaying, directionPermissions);
                if (availableMove != null)
                    yield return availableMove;
            }
        }
        private SquareWithPiece? GetAvailableSquareFrom(SquareWithPiece start, (int, int) vector, (int, int) direction, bool whitePlaying, Dictionary<(int, int), bool> directionPermissions)
        {
            foreach (SquareWithPiece square in Squares)
            {

                if (// check boundaries
                    square.Row > 0 && square.Row < Height + 1 &&
                    square.Column > 0 && square.Column < Width + 1 &&
                    // check square coordinates
                    square.Row == start.Row + vector.Item1 &&
                    square.Column == start.Column + vector.Item2
                    )
                {
                    if (square.Piece is not NoPiece)
                    {
                        if (start.Piece.IsWhite != whitePlaying)
                        {// enemy piece
                            if (directionPermissions[direction])
                            {
                                directionPermissions[direction] = false;
                                return square;
                            }
                        }
                        else
                        {// friendly piece
                            if (directionPermissions[direction])
                            {
                                directionPermissions[direction] = false;
                            }
                        }
                    }
                    else
                    {// no piece on square
                        if (directionPermissions[direction])
                        {
                            return square;
                        }
                    }
                }
            }
            return null;
        }
        private int? AreSameDirection((int, int) x, (int, int) y)
        {// null if they are not same (parallel), coeficient if they are same
            int k1;
            int k2;
            if (x.Item1 != 0)
                k1 = y.Item1 / x.Item1;
            else
                k1 = 0;
            if (x.Item2 != 0)
                k2 = y.Item2 / x.Item2;
            else
                k2 = 0;
            if (k1 > 0 && x.Item2 == 0 && y.Item2 == 0)
                return k1;
            else if (k2 > 0 && x.Item1 == 0 && y.Item1 == 0)
                return k2;

            if (k1 == k2 && k1 > 0)
                return k1;
            return null;
        }
        private SquareWithPiece GetSquare(string text)
        {
            if (text.Length != 2)
                throw new ArgumentException();

            int column = text[0] + 1 - 'a';
            int row = text[1] - '0';

            if (column < 1 || column > Width ||
                row < 1 || row > Height)
                throw new ArgumentException();

            var square = from s in Squares
                         where s.Row == row &&
                         s.Column == column
                         select s;

            return square.First();

        }

        public bool MovePiece(string text, bool whitePlaying)
        {
            //řešit move, take?

            MoveLinq move = StringToMove(text, whitePlaying);

            move.To.Piece = move.From.Piece;
            move.From.Piece = new NoPiece();

            move.From.Piece.Moved = true;
            return true;
        }
        public void Print()
        {
            Console.WriteLine();
            foreach (SquareWithPiece square in Squares)
            {
                if (square.Column == 1) Console.Write($"{square.Row}  "); // row numbers
                if (square.Piece != null)
                    Console.Write(square.Piece.Symbol);
                else
                    Console.Write('.');

                if (square.Column == 8) Console.WriteLine();
                else Console.Write(' ');
            }

            // column letters
            Console.WriteLine();
            Console.Write("   ");
            for (int i = 0; i < 8; i++)
            {
                Console.Write((char)('a' + i));
                Console.Write(' ');
            }
            Console.WriteLine();
            Console.WriteLine("------------------------");
            Console.WriteLine();
        }
    }
}
