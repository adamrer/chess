using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;


namespace Chess
{
    internal class Board
    {

        int Width { get; init; } = 8;
        int Height { get; init; } = 8;
        ImmutableDictionary<Square, IPiece> Squares { get; set; }
        Square? enPassantSquare = null;// jen aby se políčko nemuselo hledat
        Square WhiteKingSquare { get; set; } = (1, 5);
        Square BlackKingSquare { get; set; } = (8, 5);
        List<Square> WhitePieces;
        List<Square> BlackPieces;
        public Board()
        {
            Dictionary<Square, IPiece> squares = new Dictionary<Square, IPiece>();
            for (int i = 1; i < Height + 1; i++)
            {
                for (int j = 1; j < Width + 1; j++)
                {
                    Square currentSquare = new Square(i, j);
                    // pawns
                    if (i == 2) squares.Add(currentSquare, new Pawn());
                    else if (i == 7) squares.Add(currentSquare, new Pawn(false));
                    // rooks
                    else if (i == 1 && (j == 1 || j == 8)) squares.Add(currentSquare, new Rook());
                    else if (i == 8 && (j == 1 || j == 8)) squares.Add(currentSquare, new Rook(false));
                    // knights
                    else if (i == 1 && (j == 2 || j == 7)) squares.Add(currentSquare, new Knight() );
                    else if (i == 8 && (j == 2 || j == 7)) squares.Add(currentSquare, new Knight(false));
                    // bishops
                    else if (i == 1 && (j == 3 || j == 6)) squares.Add(currentSquare, new Bishop());
                    else if (i == 8 && (j == 3 || j == 6)) squares.Add(currentSquare, new Bishop(false));
                    // queens
                    else if (i == 1 && j == 4) squares.Add(currentSquare, new Queen());
                    else if (i == 8 && j == 4) squares.Add(currentSquare, new Queen(false));
                    // kings
                    else if (i == 1 && j == 5)
                        squares.Add(currentSquare, new King());
                    else if (i == 8 && j == 5)
                        squares.Add(currentSquare, new King(false));
                    // empty squares
                    else squares.Add(currentSquare, new NoPiece());

                }

            }
            Squares = squares.ToImmutableDictionary();
        }
        public Board(string fen)
        {
            Dictionary<Square, IPiece> squares = new Dictionary<Square, IPiece>();
            string[] fenSplit = fen.Split();
            int row = 8;
            int column = 1;

            foreach (char ch in fenSplit[0])
            {
                IPiece piece;
                Square square = new Square(row, column);
                if (char.IsLetter(ch)) 
                {// piece
                    try
                    {
                        piece = GetPiece(ch, !char.IsLower(ch));
                    }
                    catch (ArgumentException)
                    {

                        throw;
                    }// TODO: řešit, jestli se figurka pohla
                    squares.Add(square, piece);
                    column++;
                }
                else if (ch == '/')
                {// next row
                    row--;
                    column = 1;
                }
                else if (char.IsDigit(ch))
                {// empty square
                    for (int i = 0; i < (int)char.GetNumericValue(ch); i++)
                    {
                        squares.Add(square, new NoPiece());
                        column++;
                        square = new Square(row, column);
                    }
                }
            }
            Squares = squares.ToImmutableDictionary();
        }
        private IPiece GetPiece(char pieceSymbol, bool whitePlaying)
        {
            Type pieceType = typeof(IPiece);
            var pieceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => pieceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);


            foreach (Type type in pieceTypes)
            {
                object piece = Activator.CreateInstance(type, args: new object[] { whitePlaying })!;
                
                if (piece is IPiece ipiece &&
                    char.ToLower(ipiece.Symbol) == char.ToLower(pieceSymbol))
                    return ipiece;
            }
            
            // unknown piece
            throw new ArgumentException();
        }
        private List<Square> GetAllSquaresWithPiece(ImmutableDictionary<Square, IPiece> squares, bool white)
        {
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
        public bool TryMakeMove(string text, bool whitePlaying)
        {
            List<Move> validMoves = ChessRules.GetAvailableMoves(Squares, whitePlaying, GetAllSquaresWithPiece(Squares, whitePlaying));
            
            Move move;
            try
            {
                move = ChessRules.StringToMove(text, whitePlaying, Squares);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (!validMoves.Contains(move))
                return false;
            ImmutableDictionary<Square, IPiece>? newSquares = ChessRules.MakeMove(move, Squares);
            if (newSquares == null)
                return false;
        
            Squares = newSquares;
            return true;
        }
        public void Print(bool forWhite = true)
        {//TODO: otočit desku pro černého
            Console.WriteLine();
            int row;
            int column;
            int columnReset;
            Predicate<int> rowCondition;
            Predicate<int> columnCondition;
            Func<int, int> increase = num => num + 1;
            Func<int, int> decrease = num => num - 1;
            Func<int, int> rowFunc;
            Func<int, int> columnFunc;
            if (forWhite)
            {
                row = Height;
                column = 1;
                columnReset = 1;
                rowCondition = row => row > 0;
                columnCondition = column => column < Width + 1;
                rowFunc = decrease;
                columnFunc = increase;
            }
            else
            {
                row = 1;
                column = Width;
                columnReset = Width;
                rowCondition = row => row < Height + 1;
                columnCondition = column => column > 0;
                rowFunc = increase;
                columnFunc = decrease;
            }
            while (rowCondition(row))// rows
            {
                while (columnCondition(column))// columns 
                {
                    if (column == columnReset)
                        Console.Write($"{row} ");

                    if ((row + column) % 2 != 0)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.DarkMagenta;
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    Console.Write(Squares[(row, column)].Symbol.ToString(), Console.BackgroundColor);

                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;

                    if (column == Width)
                        Console.WriteLine();
                    else
                        Console.Write(' ');
                    column = columnFunc(column);
                }
                column = columnReset;
                row = rowFunc(row);

            }
            // column letters
            Console.Write("  ");
            for (int i = 0; i < Height; i++)
            {
                Console.Write((char)('a' + i));
                Console.Write(' ');
            }
            Console.WriteLine();
            Console.WriteLine("---------------------");
            Console.WriteLine();
        }
        public int Evaluate(bool whitePlaying)
        {
            return ChessRules.EvaluateBoard(Squares, whitePlaying);
        }
        
        public ImmutableDictionary<Square, IPiece> GetSquares()
        {
            return Squares;
        }
        public void MakeMove(Move move)
        {
            Squares = ChessRules.MakeMove(move, Squares);
        }
    }
}
