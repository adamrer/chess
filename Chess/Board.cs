using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;


namespace Chess
{
    public class Board
    {

        int Width { get; init; } = 8;
        int Height { get; init; } = 8;
        public ImmutableDictionary<Square, IPiece> Squares { get; set; }
        public Square? EnPassantSquare = null;// jen aby se políčko nemuselo hledat
        public ImmutableDictionary<char, List<Square>> WhitePieces; // keys: piece symbol, values: list of squares where are the pieces
        public ImmutableDictionary<char, List<Square>> BlackPieces;
        public Board()
        {
            Dictionary<Square, IPiece> squares = new Dictionary<Square, IPiece>();
            Dictionary<char, List < Square >> whitePieces = new Dictionary<char, List<Square>>();
            Dictionary<char, List < Square >> blackPieces = new Dictionary<char, List<Square>>();
            for (int i = 1; i < Height + 1; i++)
            {
                for (int j = 1; j < Width + 1; j++)
                {
                    Square currentSquare = new Square(i, j);
                    // pawns
                    if (i == 2) 
                    {
                        squares.Add(currentSquare, new Pawn());
                        if (!whitePieces.TryAdd((new Pawn()).Symbol, new List<Square>() { currentSquare }))
                            whitePieces[(new Pawn()).Symbol].Add(currentSquare);
                    }
                    else if (i == 7)
                    {
                        squares.Add(currentSquare, new Pawn(false));
                        if (!blackPieces.TryAdd((new Pawn(false)).Symbol, new List<Square>() { currentSquare }))
                            blackPieces[(new Pawn(false)).Symbol].Add(currentSquare);
                    }
                    // rooks
                    else if (i == 1 && (j == 1 || j == 8))
                    {
                        squares.Add(currentSquare, new Rook());
                        if (!whitePieces.TryAdd((new Rook()).Symbol, new List<Square>() { currentSquare }))
                            whitePieces[(new Rook()).Symbol].Add(currentSquare);
                    }
                    else if (i == 8 && (j == 1 || j == 8))
                    {
                        squares.Add(currentSquare, new Rook(false));
                        if (!blackPieces.TryAdd((new Rook(false)).Symbol, new List<Square>() { currentSquare }))
                            blackPieces[(new Rook(false)).Symbol].Add(currentSquare);
                    }
                    // knights
                    else if (i == 1 && (j == 2 || j == 7))
                    {
                        squares.Add(currentSquare, new Knight() );
                        if (!whitePieces.TryAdd((new Knight()).Symbol, new List<Square>() { currentSquare }))
                            whitePieces[(new Knight()).Symbol].Add(currentSquare);
                    }
                    else if (i == 8 && (j == 2 || j == 7))
                    {
                        squares.Add(currentSquare, new Knight(false));
                        if (!blackPieces.TryAdd((new Knight(false)).Symbol, new List<Square>() { currentSquare }))
                            blackPieces[(new Knight(false)).Symbol].Add(currentSquare);
                    }
                    // bishops
                    else if (i == 1 && (j == 3 || j == 6))
                    {
                        squares.Add(currentSquare, new Bishop());
                        if (!whitePieces.TryAdd((new Bishop()).Symbol, new List<Square>() { currentSquare }))
                            whitePieces[(new Bishop()).Symbol].Add(currentSquare);
                    }
                    else if (i == 8 && (j == 3 || j == 6))
                    {
                        squares.Add(currentSquare, new Bishop(false));
                        if (!blackPieces.TryAdd((new Bishop(false)).Symbol, new List<Square>() { currentSquare }))
                            blackPieces[(new Bishop(false)).Symbol].Add(currentSquare);
                    }
                    // queens
                    else if (i == 1 && j == 4)
                    {
                        squares.Add(currentSquare, new Queen());
                        whitePieces.Add((new Queen()).Symbol, new List<Square>() { currentSquare });
                    }
                    else if (i == 8 && j == 4)
                    {
                        squares.Add(currentSquare, new Queen(false));
                        blackPieces.Add((new Queen(false)).Symbol, new List<Square>() { currentSquare });
                    }
                    // kings
                    else if (i == 1 && j == 5)
                    {
                        squares.Add(currentSquare, new King());
                        whitePieces.Add((new King()).Symbol, new List<Square>() { currentSquare });
                    }
                    else if (i == 8 && j == 5)
                    {
                        squares.Add(currentSquare, new King(false));
                        blackPieces.Add((new King(false)).Symbol, new List<Square>() { currentSquare });
                    }
                    // empty squares
                    else squares.Add(currentSquare, new NoPiece());

                }

            }
            Squares = squares.ToImmutableDictionary();
            WhitePieces = whitePieces.ToImmutableDictionary();
            BlackPieces = blackPieces.ToImmutableDictionary();

        }
        public Board(ImmutableDictionary<Square, IPiece> squares, 
            ImmutableDictionary<char, List<Square>> whitePieces, 
            ImmutableDictionary<char, List<Square>> blackPieces, 
            Square? enPassantSquare)
        {
            Squares = squares;
            WhitePieces = whitePieces;
            BlackPieces = blackPieces;
            this.EnPassantSquare = enPassantSquare;
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
                    }
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
        public bool TryMakeMove(string text, bool whitePlaying)
        {
            List<Move> validMoves = ChessRules.GetAvailableMoves(this, whitePlaying);
            
            Move move;
            try
            {
                move = ChessRules.StringToMove(text, whitePlaying, this);
            }
            catch (ArgumentException)
            {
                return false;
            }
            if (!validMoves.Contains(move))
                return false;
            
            MakeMove(move);

            return true;
        }
        public void Print(bool forWhite = true)
        {
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
                Console.Write($"{row} ");
                while (columnCondition(column))// columns 
                {

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

                    if (column == Width + 1 - columnReset)
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
                if (forWhite)
                    Console.Write((char)('a' + i));
                else
                    Console.Write((char)('h' - i));
                Console.Write(' ');
            }
            Console.WriteLine();
            Console.WriteLine("---------------------");
            Console.WriteLine();
        }
        public string GetFen()
        {
            string fen = "";
            int emptySpaceCount = 0;
            for (int row = Height; row > 0; row--)
            {
                for (int column = 1; column <= Width; column++)
                {
                    if (Squares[(row, column)] is NoPiece)
                        emptySpaceCount++;
                    else
                    {
                        if (emptySpaceCount != 0)
                            fen += emptySpaceCount.ToString();

                        fen += Squares[(row, column)].Symbol.ToString();
                        emptySpaceCount = 0;
                    }
                }
                if (emptySpaceCount != 0)
                {
                    fen += emptySpaceCount;
                    emptySpaceCount = 0;
                }
                if (row != 1)
                    fen += "/";
            }

            return fen;
        }
        public int Evaluate(bool whitePlaying)
        {
            return ChessRules.EvaluateBoard(this, whitePlaying);
        }
        
        public ImmutableDictionary<Square, IPiece> GetSquares()
        {
            return Squares;
        }
        public void MakeMove(Move move)
        {
            Board newBoard = ChessRules.MakeMove(move, this);
            Squares = newBoard.Squares;
            WhitePieces = newBoard.WhitePieces;
            BlackPieces = newBoard.BlackPieces;
            EnPassantSquare = newBoard.EnPassantSquare;
        }
    }
}
