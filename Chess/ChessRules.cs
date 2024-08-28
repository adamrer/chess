using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Chess
{
    internal class ChessRules
    {
        public static int Height { get; } = 8;
        public static int Width { get; } = 8;
        public static Move StringToMove(string text, bool whitePlaying, ImmutableDictionary<Square, IPiece> squares)
        {
            // move: [*piece_symbol*][*row_or_column*][destination_square]


            MoveCommand query = StringToCommand(text, squares);
            
            
            List<Square> piecesSquares = FindPiecesSatisfyingCommand(query, false, whitePlaying, squares);

            // ambiguity
            if (piecesSquares.Count > 1)
            {

                if (query.PieceIdentification != null)
                {
                    char identifier = query.PieceIdentification.GetValueOrDefault();

                    if (char.IsLetter(identifier))
                    {
                        int column = identifier - 'a' + 1;
                        for (int i = 0; i < piecesSquares.Count; i++)
                        {
                            if (piecesSquares[i].Column != column)
                                piecesSquares.RemoveAt(i);
                        }
                    }
                    else if (char.IsDigit(identifier))
                    {
                        int row = identifier - '0';
                        for (int i = 0; i < piecesSquares.Count; i++)
                        {
                            if (piecesSquares[i].Row != row)
                                piecesSquares.RemoveAt(i);
                        }
                    }
                    else
                        throw new ArgumentException();

                    if (piecesSquares.Count != 1)
                        throw new ArgumentException();
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            else if (piecesSquares.Count == 0)
            {
                if (query.PieceSymbol == 'k')
                    return new Move(FindPiece('k', whitePlaying, squares), query.Destination);
                throw new ArgumentException();
            }



            return new Move(piecesSquares[0], query.Destination);
        }

        private static MoveCommand StringToCommand(string text, ImmutableDictionary<Square, IPiece> squares)
        {
            if (text.Length < 2 || text.Length > 4)
                throw new ArgumentException();

            char pieceSymbol;

            if (text.Length == 2)
                pieceSymbol = 'p';
            else
                pieceSymbol = text[0];

            Square destination = GetSquare(text.Substring(text.Length - 2), squares);

            char? specifyingChar;
            if (text.Length == 4)
                specifyingChar = text[1];
            else if (pieceSymbol == 'p' && text.Length == 3)
                specifyingChar = text[0];
            else
                specifyingChar = null;

            return new MoveCommand(pieceSymbol, destination, specifyingChar);
        }
        private static Square GetSquare(string text, ImmutableDictionary<Square, IPiece> squares)
        {
            if (text.Length != 2)
                throw new ArgumentException();

            int column = text[0] + 1 - 'a';
            int row = text[1] - '0';

            if (column < 1 || column > Width ||
                row < 1 || row > Height)
                throw new ArgumentException();

            Square square = new Square(row, column);

            if (squares.ContainsKey(square))
                return square;

            throw new ArgumentException();

        }
        private static List<Square> GetAllSquaresWithPiece(ImmutableDictionary<Square, IPiece> squares, bool white)
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
        private static IPiece GetPiece(char pieceSymbol, bool whitePlaying)
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
        private static bool AreSameDirection((int, int) x, (int, int) y)
        {
            float k1;
            float k2;
            if (x.Item1 != 0)
                k1 = y.Item1 / x.Item1;
            else
                k1 = 0;
            if (x.Item2 != 0)
                k2 = y.Item2 / x.Item2;
            else
                k2 = 0;
            if ((k1 == k2 && k1 > 0) ||
                    (k2 > 0 && x.Item1 == 0 && y.Item1 == 0) ||
                    (k1 > 0 && x.Item2 == 0 && y.Item2 == 0))
                return true;

            return false;
        }
        private static double VectorMagnitude((int, int) x)
        {
            return Math.Sqrt(x.Item1 * x.Item1 + x.Item2 * x.Item2);
        }
        private static bool SmallerSameDirectionInList(IEnumerable<(int, int)> directions, (int, int) direction)
        {
            foreach (var d in directions)
            {
                if (AreSameDirection(d, direction) &&
                    VectorMagnitude(d) < VectorMagnitude(direction))
                    return true;
            }

            return false;
        }
        private static List<Square> FindPiecesSatisfyingCommand(
            MoveCommand query, bool attacking, bool whitePlaying, ImmutableDictionary<Square, IPiece> squares, bool guard = false)
        {
            // najít figurku/y, která se dostane na square
            List<Square> piecesSquares = new List<Square>();
            IPiece piece = GetPiece(query.PieceSymbol, whitePlaying);

            List<(int, int)> blockedDirections = new List<(int, int)>();
            if (squares[query.Destination] is not NoPiece && squares[query.Destination].IsWhite == whitePlaying && !guard)
                return piecesSquares;

            IEnumerable<(int, int)> vectors;
            if (((squares[query.Destination] is not NoPiece ||
                (squares[query.Destination] is EnPassant && char.ToLower(query.PieceSymbol) == 'p')) &&
                squares[query.Destination].IsWhite != whitePlaying) ||
                attacking)
                vectors = piece.AttackVectors;
            else
                vectors = piece.MoveVectors;

            foreach (var moveVector in vectors)
            {
                (int, int) opositeVector = (-moveVector.Item1, -moveVector.Item2);
                Square possibleSquareOfPiece =
                    new Square(query.Destination.Row + opositeVector.Item1, query.Destination.Column + opositeVector.Item2);
                
                int? pieceId = null;
                if (query.PieceIdentification != null)
                    if (char.IsLetter(query.PieceIdentification.GetValueOrDefault()))
                        pieceId = query.PieceIdentification - 'a' + 1;
                    else if (char.IsDigit(query.PieceIdentification.GetValueOrDefault()))
                        pieceId = query.PieceIdentification - '0';
                    
                if (squares.ContainsKey(possibleSquareOfPiece) &&
                    squares[possibleSquareOfPiece] is not NoPiece)
                {// square is on board and not empty
                    if (squares[possibleSquareOfPiece].Symbol == piece.Symbol &&
                        squares[possibleSquareOfPiece].IsWhite == whitePlaying &&
                        !SmallerSameDirectionInList(blockedDirections, opositeVector) &&
                        (squares[possibleSquareOfPiece].MoveVectors.Contains(moveVector) || // because of .Moved (piece.Moved = false always)
                        squares[possibleSquareOfPiece].AttackVectors.Contains(moveVector)) &&
                        (pieceId == null || (pieceId == possibleSquareOfPiece.Column || pieceId == possibleSquareOfPiece.Row)) // column specification
                        )
                    {// the right piece is on it

                        piecesSquares.Add(possibleSquareOfPiece);
                    }
                    else
                    {// piece blocking
                        blockedDirections.Add(opositeVector);
                    }
                }
            }


            return piecesSquares;

        }
        private static List<Square> FindPiecesTargetingDestinationSquare(MoveCommand query, ImmutableDictionary<Square, IPiece> squares, bool white)
        {// míří skrz figurky např.
            IPiece piece = GetPiece(query.PieceSymbol, white);

            List<(int, int)> blockedDirections = new List<(int, int)>();
            List<Square> targetingPieces = new List<Square>();
            if (squares[query.Destination] is not NoPiece && squares[query.Destination].IsWhite == white)
                return targetingPieces;


            foreach (var moveVector in piece.AttackVectors)
            {
                (int, int) opositeVector = (-moveVector.Item1, -moveVector.Item2);
                Square possibleSquareOfPiece =
                    new Square(query.Destination.Row + opositeVector.Item1, query.Destination.Column + opositeVector.Item2);

                if (squares.ContainsKey(possibleSquareOfPiece) &&
                    squares[possibleSquareOfPiece] is not NoPiece)
                {// square is on board and not empty
                    if (squares[possibleSquareOfPiece].Symbol == piece.Symbol &&
                        squares[possibleSquareOfPiece].IsWhite == white &&
                        !SmallerSameDirectionInList(blockedDirections, opositeVector) &&
                        (squares[possibleSquareOfPiece].MoveVectors.Contains(moveVector) || // because of .Moved (piece.Moved = false always)
                        squares[possibleSquareOfPiece].AttackVectors.Contains(moveVector)) &&
                        (query.PieceIdentification == null || query.PieceIdentification - '0' == possibleSquareOfPiece.Column) // column specification
                        )
                    {// the right piece is on it

                        targetingPieces.Add(possibleSquareOfPiece);
                    }
                    else if (squares[possibleSquareOfPiece].IsWhite == white && squares[possibleSquareOfPiece].Symbol != piece.Symbol)
                    {// same colored piece is blocking 
                        blockedDirections.Add(opositeVector);
                    }
                    // different colored piece or empty square
                }
            }
            return targetingPieces;
        }
        private static List<Move> GetAvailableMovesForPiece(Square pieceSquare, ImmutableDictionary<Square, IPiece> squares)
        {
            IPiece piece = squares[pieceSquare];

            List<Move> availableMoves = new List<Move>();

            if (piece is NoPiece)
                return availableMoves;

            List<(int, int)> blockedDirections = new List<(int, int)>();


            foreach ((int, int) vector in piece.MoveVectors)
            {
                Square square = new Square(pieceSquare.Row + vector.Item1, pieceSquare.Column + vector.Item2);
                if (!squares.ContainsKey(square))
                    continue;
                if (squares[square] is NoPiece || (piece is not Pawn && squares[square].IsWhite != piece.IsWhite)) // except pawn, every piece is attacking with moveVectors (MoveVectors = AttackVectors)
                {// is empty
                    
                    if (piece is not Pawn && squares[square] is not NoPiece && squares[square].IsWhite != piece.IsWhite)
                        blockedDirections.Add(vector);// enemy piece blocking
                    
                    if (!SmallerSameDirectionInList(blockedDirections, vector))
                        availableMoves.Add(new Move(pieceSquare, square)); //direction is not blocked
                } 
                else
                    blockedDirections.Add(vector); // ally piece blocking
                    
            }
            if (piece is Pawn)
            {
                blockedDirections = new List<(int, int)>();
                foreach ((int, int) vector in piece.AttackVectors)
                {
                    Square square = new Square(pieceSquare.Row + vector.Item1, pieceSquare.Column + vector.Item2);
                    if (!squares.ContainsKey(square))
                        continue;
                    if (squares[square] is not NoPiece || squares[square] is EnPassant)
                    {// piece
                        if (squares[square].IsWhite != piece.IsWhite)
                        {// enemy piece
                                if (!SmallerSameDirectionInList(blockedDirections, vector))
                                    availableMoves.Add(new Move(pieceSquare, square)); // direction is not blocked
                            else
                                blockedDirections.Add(vector); // ally piece blocking
                        }
                        else
                            blockedDirections.Add(vector); // enemy piece blocking
                    }
                }
            }

            return availableMoves;
        }
        private static bool IsSafe(Square square, bool whitePlaying, ImmutableDictionary<Square, IPiece> squares)
        {// square is not attacked by any enemy piece
            Type pieceType = typeof(IPiece);
            // all piece classes
            var pieceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => pieceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);


            foreach (Type type in pieceTypes)
            {
                object piece = Activator.CreateInstance(type, args: new object[] { whitePlaying })!;
                if (piece is IPiece ipiece)
                {
                    if (FindPiecesSatisfyingCommand(new MoveCommand(ipiece.Symbol, square, null), true, !whitePlaying, squares).Count > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        private static bool IsGuarded(Square square, bool white, ImmutableDictionary<Square, IPiece> squares)
        {
            Type pieceType = typeof(IPiece);
            // all piece classes
            var pieceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => pieceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);


            foreach (Type type in pieceTypes)
            {
                object piece = Activator.CreateInstance(type, args: new object[] { white })!;
                if (piece is IPiece ipiece)
                {
                    if (FindPiecesSatisfyingCommand(new MoveCommand(ipiece.Symbol, square, null), true, white, squares, true).Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        private static bool CanCastle(string move, bool whitePlaying, ImmutableDictionary<Square, IPiece> squares)
        {
            MoveCommand query = StringToCommand(move, squares);

            Square leftRookSquare = new Square(1, 1);
            Square rightRookSquare = new Square(1, 8);
            Square kingSquare = new Square(1, 5);
            if (!whitePlaying)
            {
                kingSquare = new Square(8, 5);
                leftRookSquare = new Square(8, 1);
                rightRookSquare = new Square(8, 8);
            }
            if (char.ToLower(query.PieceSymbol) == 'k' &&
                char.ToLower(squares[kingSquare].Symbol) == 'k' &&
                !squares[kingSquare].Moved &&
                IsSafe(kingSquare, whitePlaying, squares))
            {// king didn't move and is not in check
                if (query.Destination.Row == leftRookSquare.Row &&
                    query.Destination.Column == kingSquare.Column - 2 &&
                    char.ToLower(squares[leftRookSquare].Symbol) == 'r' &&
                    !squares[leftRookSquare].Moved &&
                    IsSafe(query.Destination, whitePlaying, squares))
                {// left rook ready
                    for (int column = leftRookSquare.Column + 1; column < kingSquare.Column; column++)
                    {
                        if (squares[(leftRookSquare.Row, column)] is not NoPiece)
                            return false;// piece is in way
                    }
                    return true;
                }
                else if (query.Destination.Row == rightRookSquare.Row &&
                    query.Destination.Column == kingSquare.Column + 2 &&
                    char.ToLower(squares[rightRookSquare].Symbol) == 'r' &&
                    !squares[rightRookSquare].Moved &&
                    IsSafe(query.Destination, whitePlaying, squares))
                {// right rook ready
                    for (int column = rightRookSquare.Column - 1; column > kingSquare.Column; column--)
                    {
                        if (squares[(rightRookSquare.Row, column)] is not NoPiece)
                            return false;// piece is in way
                    }
                    return true;
                }
            }
            return false;
        }
        private static Square FindPiece(char pieceSymbol, bool isWhite, ImmutableDictionary<Square, IPiece> squares)
        {
            foreach (var item in squares)
            {
                if (item.Value.IsWhite == isWhite &&
                    char.ToLower(item.Value.Symbol) == char.ToLower(pieceSymbol))
                    return item.Key;
            }
            throw new KeyNotFoundException();
        }

        private static List<Move> GetSafeMovesForKing(ImmutableDictionary<Square, IPiece> squares, Square kingSquare, List<Square> piecesCheckingKing, bool white)
        {
            List<Move> safeMoves = new List<Move>();
            foreach ((int, int) moveVector in squares[kingSquare].MoveVectors)
            {
                Square square = new Square(kingSquare.Row + moveVector.Item1, kingSquare.Column + moveVector.Item2);
                if (squares.ContainsKey(square) && IsSafe(square, white, squares))
                {// square is safe
                    if (squares[square] is NoPiece ||
                        (squares[square] is not NoPiece && // piece
                        squares[square].IsWhite != white && // enemy
                        !IsGuarded(square, !white, squares)) // not guarded
                        )
                    {
                        foreach (Square pieceCheckingKing in piecesCheckingKing)
                        {
                            bool pieceCheckingKingCanBeTaken = squares[kingSquare].MoveVectors.Contains(pieceCheckingKing);
                            if ((squares[pieceCheckingKing] is Rook || squares[pieceCheckingKing] is Queen) && pieceCheckingKingCanBeTaken)
                            {
                                if (pieceCheckingKing.Row == square.Row || pieceCheckingKing.Column == square.Column)
                                    goto NEXTSQUARE; // enemy piece could still attack this square after moving king to this square (same row/column as the rook/queen)
                            }
                            else if ((squares[pieceCheckingKing] is Bishop || squares[pieceCheckingKing] is Queen) && pieceCheckingKingCanBeTaken)
                            {// if we are here, queen is checking king diagonally
                                (int, int) checkingVector = (kingSquare.Row - pieceCheckingKing.Row, kingSquare.Column - pieceCheckingKing.Column);
                                (int, int) checkingVectorOne = (checkingVector.Item1 / Math.Abs(checkingVector.Item1), checkingVector.Item2 / Math.Abs(checkingVector.Item2));
                                
                                // square in the same direction that checking piece is attacking king plus one square
                                Square dangerousSquare = (kingSquare.Row + checkingVectorOne.Item1, kingSquare.Column + checkingVectorOne.Item2);
                                if (square.Equals(dangerousSquare))
                                    goto NEXTSQUARE;
                            }
                        }
                        safeMoves.Add(new Move(kingSquare, square));// has a safe square
                    NEXTSQUARE:;
                    }
                }
            }
            return safeMoves;
        }
        private static List<Square> GetSquaresBetweenSquares(Square square1, Square square2)
        {

            List<Square> squaresBetween = new List<Square>();
            
            (int, int) vector = (square1.Row - square2.Row, square1.Column - square2.Column);

            if (vector.Item1 != 0 && vector.Item2 != 0 && Math.Abs(vector.Item1) != Math.Abs(vector.Item2))
                return squaresBetween;// knight -> no piece between them

            if (vector.Item1 != 0)
            {
                vector.Item1 /= Math.Abs(vector.Item1);
            }
            if (vector.Item2 != 0)
            {
                vector.Item2 /= Math.Abs(vector.Item2);
            }

            Square betweenSquare = (square2.Row + vector.Item1, square2.Column + vector.Item2);
            while (!betweenSquare.Equals(square1))
            {
                squaresBetween.Add(betweenSquare);
                betweenSquare = (betweenSquare.Row + vector.Item1, betweenSquare.Column + vector.Item2);
            }
            return squaresBetween;
        }
        private static List<Square> KingCheckedBy(ImmutableDictionary<Square, IPiece> squares, Square kingSquare, bool whitePlaying)
        {// return squares, where stand enemy pieces that are checking king
            Type pieceType = typeof(IPiece);
            // all piece classes without king
            var pieceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => pieceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract && p != typeof(King));


            foreach (Type type in pieceTypes)
            {
                object piece = Activator.CreateInstance(type, args: new object[] { whitePlaying })!;
                if (piece is IPiece ipiece)
                {
                    List<Square> dangerousPiecesSquares = FindPiecesSatisfyingCommand(new MoveCommand(ipiece.Symbol, kingSquare, null), true, !whitePlaying, squares);
                    if (dangerousPiecesSquares.Count > 0)
                    {
                        return dangerousPiecesSquares;
                    }
                }
            }

            return new List<Square>();
        }
        private static Dictionary<Square, Square> FindPinnedPieces(ImmutableDictionary<Square, IPiece> squares, Square kingSquare)
        {// keys - pinned pieces, values - by who is pinned
            Type pieceType = typeof(IPiece);
            // all piece classes without king
            var pieceTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => pieceType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract && p != typeof(King));

            Dictionary<Square, Square> pinnedPieces = new Dictionary<Square, Square>();

            foreach (Type type in pieceTypes)
            {
                object piece = Activator.CreateInstance(type, args: new object[] { squares[kingSquare].IsWhite })!;

                if (piece is IPiece ipiece)
                {
                    List<Square> attackingPieces = FindPiecesTargetingDestinationSquare(new MoveCommand(ipiece.Symbol, kingSquare, null), squares, !squares[kingSquare].IsWhite);
                    foreach (Square enemySquare in attackingPieces)
                    {
                        List<Square> squaresBetween = GetSquaresBetweenSquares(enemySquare, kingSquare);
                        List<Square> guardingPieces = new List<Square>();
                        foreach (Square squareBetween in squaresBetween)
                        {
                            if (squares[squareBetween] is not NoPiece)
                            {
                                guardingPieces.Add(squareBetween);
                            }
                        }
                        if (guardingPieces.Count == 1)// only one piece, that is guarding the king from being attacked by attackingPiece
                            pinnedPieces.Add(guardingPieces[0], enemySquare);
                    }
                }
            }
            return pinnedPieces;
        }

        public static int EvaluateBoard(ImmutableDictionary<Square, IPiece> squares, bool whitePlaying)
        {// returns: greater than zero - draw, less than zero - player lost, zero - game continues
            List<Move> availableMoves = GetAvailableMoves(squares, whitePlaying, GetAllSquaresWithPiece(squares, whitePlaying));
            Square kingSquare = FindPiece('k', whitePlaying, squares);
            if (availableMoves.Count == 0 && !IsSafe(kingSquare, whitePlaying, squares))
                return -1;// checkmate

            if (availableMoves.Count == 0)
                return 1;// stalemate

            return 0;

        }

        public static ImmutableDictionary<Square, IPiece> MakeMove(Move move, ImmutableDictionary<Square, IPiece> squares)
        {
            var builder = ImmutableDictionary.CreateBuilder<Square, IPiece>();

            IPiece pieceMoving = GetPiece(squares[move.From].Symbol, squares[move.From].IsWhite);
            pieceMoving.Moved = true;
            
            //promotion to queen
            if (pieceMoving is Pawn && ((pieceMoving.IsWhite && move.To.Row == Height) || (!pieceMoving.IsWhite && move.To.Row == 1)))
            {
                IPiece promotion = GetPiece('q', pieceMoving.IsWhite);

                for (int i = 1; i < Height + 1; i++)
                {
                    for (int j = 1; j < Width + 1; j++)
                    {
                        Square currentSquare = new Square(i, j);
                        if (move.To.Equals(currentSquare))
                            builder.Add(new((i, j), promotion));//promotion
                        else if (move.From.Equals(currentSquare))
                            builder.Add(new((i, j), new NoPiece()));// remove pawn
                        else
                            builder.Add(new((i, j), squares[currentSquare]));
                    }
                }
                
            }
            // en passant
            else if(pieceMoving is Pawn && squares[move.To] is EnPassant )
            {
                Square prayPawnSquare = new Square(move.To.Row - 1, move.To.Column);
                if (!squares[move.To].IsWhite)
                    prayPawnSquare = new Square(move.To.Row + 1, move.To.Column);
                
                for (int i = 1; i < Height + 1; i++)
                {
                    for (int j = 1; j < Width + 1; j++)
                    {
                        Square currentSquare = new Square(i, j);
                        if (move.To.Equals(currentSquare))
                            builder.Add(new((i, j), pieceMoving)); // piece moved here
                        else if (prayPawnSquare.Equals(currentSquare))
                            builder.Add(new((i, j), new NoPiece())); // pawn taken
                        else if (move.From.Equals(currentSquare))
                            builder.Add(new((i, j), new NoPiece())); // piece moved
                        else
                            builder.Add(new((i, j), squares[currentSquare])); // no changes
                    }
                }
            }
            // castle
            else if (pieceMoving is King &&
                    (move.From.Equals(new Square(1,5)) && 
                    (move.To.Equals(new Square(1, 7)) || move.To.Equals(new Square(1, 3))) ||
                    (move.From.Equals(new Square(8,5)) && 
                    (move.To.Equals(new Square(8, 7)) || move.To.Equals(new Square(8, 3))))))
            {
                Square kingSquare = move.From;
                Square rookSquare = new Square(move.From.Row, 1); // left rook
                Square rookDestination = new Square(move.From.Row, kingSquare.Column - 1);
                if (move.To.Column != 3)
                {
                    rookSquare = new Square(move.From.Row, 8); // right rook
                    rookDestination = new Square(move.From.Row, kingSquare.Column + 1);
                }
                IPiece rook = squares[rookSquare];
                for (int i = 1; i < Height + 1; i++)
                {
                    for (int j = 1; j < Width + 1; j++)
                    {
                        Square currentSquare = new Square(i, j);
                        if (move.To.Equals(currentSquare))
                            builder.Add(new((i, j), pieceMoving)); // piece moved here
                        else if (kingSquare.Equals(currentSquare))
                            builder.Add(new((i, j), new NoPiece())); // piece moved
                        else if (rookSquare.Equals(currentSquare))
                            builder.Add(new((i, j), new NoPiece())); // rook moved
                        else if (rookDestination.Equals(currentSquare))
                            builder.Add(new((i, j), rook)); // rook moved here
                        else
                            builder.Add(new((i, j), squares[(i, j)])); // no changes
                    }
                }
            }
            else
            {
                for (int i = 1; i < Height + 1; i++)
                {
                    for (int j = 1; j < Width + 1; j++)
                    {
                        Square currentSquare = new Square(i, j);
                        if (move.To.Equals(currentSquare))
                            builder.Add(new((i, j), pieceMoving));// piece moving
                        else if (move.From.Equals(currentSquare))
                            builder.Add(new((i, j), new NoPiece()));// remove pawn
                        else
                            builder.Add(new((i, j), squares[currentSquare]));
                    }
                }
            }

            return builder.ToImmutable();
        }

        public static List<Move> GetAvailableMoves(ImmutableDictionary<Square, IPiece> squares, bool white, List<Square> squaresWithPieces)
        {
            List <Move> availableMoves = new List<Move>();
            Square kingSquare = FindPiece('k', white, squares);
            
            Dictionary<Square, Square> pinnedPieces = FindPinnedPieces(squares, kingSquare);

            // castle
            if (white)
            {
                if (CanCastle("kg1", white, squares))
                    availableMoves.Add( new Move(new Square(1, 5), new Square(1, 7)));
                if (CanCastle("kc1", white, squares))
                    availableMoves.Add(new Move(new Square(1, 5), new Square(1, 3)));
            }
            else
            {
                if (CanCastle("kg8", white, squares))
                    availableMoves.Add(new Move(new Square(8, 5), new Square(8, 7)));
                if (CanCastle("kc8", white, squares))
                    availableMoves.Add(new Move(new Square(8, 5), new Square(8, 3)));
            }
            
            List<Square> dangerousPiecesSquares = KingCheckedBy(squares, kingSquare, white);
            if (dangerousPiecesSquares.Count > 0)
            {// king is checked
                
                availableMoves.AddRange(GetSafeMovesForKing(squares, kingSquare, dangerousPiecesSquares, white));

                List<Square> squaresBetween = GetSquaresBetweenSquares(dangerousPiecesSquares[0], kingSquare);
                foreach (Square square in squaresWithPieces)
                {
                    IPiece piece = squares[square];
                    if (piece is King)
                        continue;
                    List<Square> piecesThatCanTakeDangerousPiece = FindPiecesSatisfyingCommand(new MoveCommand(piece.Symbol, dangerousPiecesSquares[0], char.Parse(square.Column.ToString())), true, white, squares);
                    foreach (Square item in piecesThatCanTakeDangerousPiece)
                    {
                        availableMoves.Add(new Move(item, dangerousPiecesSquares[0]));
                    }
                    foreach (Square squareToBlock in squaresBetween)
                    {
                        List<Square> piecesThatCanBlockCheck = FindPiecesSatisfyingCommand(new MoveCommand(piece.Symbol, squareToBlock, char.Parse(square.Column.ToString())), false, white, squares);
                        foreach (Square blockingPieceSquare in piecesThatCanBlockCheck)
                        {
                            if (!pinnedPieces.ContainsKey(blockingPieceSquare))
                                availableMoves.Add(new Move(blockingPieceSquare, squareToBlock));
                        }
                    }
                }
            }
            else
            {
                foreach (Square square in squaresWithPieces)
                {
                    IPiece piece = squares[square];
                    if (piece is King)
                    {
                        availableMoves.AddRange(GetSafeMovesForKing(squares, kingSquare, dangerousPiecesSquares, white));

                    }
                    else if (piece is not NoPiece && !pinnedPieces.ContainsKey(square))// pinned pieces cannot move
                    {

                        availableMoves.AddRange( GetAvailableMovesForPiece(square, squares) );
                    }
                    else if (piece is not NoPiece && pinnedPieces.ContainsKey(square))
                    {
                        List<Move> moves = GetAvailableMovesForPiece(square, squares);
                        foreach (Move m in moves)
                        {
                            if (m.To.Equals(pinnedPieces[square]))
                                availableMoves.Add(m);// pinned piece can take the pinning enemy piece
                        }
                    }
                }

            }


            return availableMoves;
        }
    }
}
