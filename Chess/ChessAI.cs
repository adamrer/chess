using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess
{
    internal class ChessAI
    {
        bool IsWhite { get; init; }
        public ChessAI(bool isWhite)
        {
            IsWhite = isWhite;
        }

        private int CalculateMobilityScore(ImmutableDictionary<Square, IPiece> squares)
        {
            return ChessRules.GetAvailableMoves(squares, IsWhite, GetAllSquaresWithPiece(squares, IsWhite)).Count / 50;
        }
        private int CalculateKingSafetyScore(ImmutableDictionary<Square, IPiece> squares)
        {// TODO: calculate king safety
            return 0;
        }
        private int CalculateMaterialScore(ImmutableDictionary<Square, IPiece> squares)
        {
            int score = 0;
            foreach (Square square in squares.Keys)
            {
                IPiece piece = squares[square];
                if (piece is not NoPiece )
                {
                    if (piece.IsWhite == IsWhite)
                        score += piece.Value;
                    else
                        score -= piece.Value;
                }
            }
            return score;
        }
        private int CalculateCenterScore(ImmutableDictionary<Square, IPiece> squares)
        {// políčka d4, e4, d5, e5 jsou zabraná
            int score = 0;
            
            List<Square> centerSquares = new List<Square>() { 
                // d4, d5, e4, e5
                new Square(4, 4), new Square(5, 4), new Square(4, 5), new Square(5, 5) 
            };
            foreach (Square centerSquare in centerSquares)
            {
                if (squares[centerSquare] is not NoPiece)
                {
                    if (squares[centerSquare].IsWhite == IsWhite)
                        score += 3;
                    else
                        score -= 3;
                }
            }

            return score;
        }
        private int EvaluateBoardScore(ImmutableDictionary<Square, IPiece> squares)
        {
            int totalScore = CalculateKingSafetyScore(squares) + 
                CalculateMobilityScore(squares) + 
                CalculateCenterScore(squares) + 
                CalculateMaterialScore(squares);

            return totalScore;
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
        
        private MoveValue EvaluateBestMoveParallel(ImmutableDictionary<Square, IPiece> squares, 
            int depth, bool whitePlaying, int alpha, int beta)
        {//minimax

            //int boardEvaluation = ChessRules.EvaluateBoard(squares, whitePlaying);
            //if (depth == 0 || boardEvaluation != 0)
            //{
            //    if (boardEvaluation > 0) // draw
            //        return new MoveValue(null, 0);
            //    else if (boardEvaluation < 0) // win
            //        return new MoveValue(null, int.MaxValue);
            //    else // leaf, depth
            //        return new MoveValue(null, EvaluateBoardScore(squares));
            //}

            MoveValue maxEval = new MoveValue(null, int.MinValue);
            if (whitePlaying != IsWhite)
                maxEval.Value = int.MaxValue;
            var moves = ChessRules.GetAvailableMoves(squares, whitePlaying, GetAllSquaresWithPiece(squares, whitePlaying));// TODO: nějak si udržovat, kde jsou figurky?

            Parallel.ForEach(moves, move =>
            {
                MoveValue childrenEval = EvaluateBestMove(ChessRules.MakeMove(move, squares), depth - 1, !whitePlaying, alpha, beta);

                if (MinimaxStep(move, alpha, beta, whitePlaying, maxEval, childrenEval, moves))
                    return;
            });

            return maxEval;


        }
        
        private MoveValue EvaluateBestMove(ImmutableDictionary<Square, IPiece> squares,
            int depth, bool whitePlaying, int alpha, int beta)
        {
            int boardEvaluation = ChessRules.EvaluateBoard(squares, whitePlaying);
            if (depth == 0 || boardEvaluation != 0)
            {
                if (boardEvaluation > 0) // draw
                    return new MoveValue(null, 0);
                else if (boardEvaluation < 0) 
                    if (whitePlaying == IsWhite)// win
                        return new MoveValue(null, int.MaxValue);
                    else
                        return new MoveValue(null, int.MinValue);
                else // leaf, depth
                    return new MoveValue(null, EvaluateBoardScore(squares));
            }

            MoveValue maxEval = new MoveValue(null, int.MinValue);
            if (whitePlaying != IsWhite)
                maxEval.Value = int.MaxValue;
            var moves = ChessRules.GetAvailableMoves(squares, whitePlaying, GetAllSquaresWithPiece(squares, whitePlaying));// TODO: nějak si udržovat, kde jsou figurky?
            foreach (Move move in moves)
            {
                MoveValue childrenEval = EvaluateBestMove(ChessRules.MakeMove(move, squares), depth - 1, !whitePlaying, alpha, beta);

                if (MinimaxStep(move, alpha, beta, whitePlaying, maxEval, childrenEval, moves))
                    break;
            }

            return maxEval;

        }
        private bool MinimaxStep(Move move, int alpha, int beta, bool whitePlaying, MoveValue maxEval, MoveValue childrenEval, object oLock)
        {// true when cutoff must happen
            lock (oLock)
            {// maxEval and alpha/beta is modified by one thread in time
                if ((whitePlaying == IsWhite && childrenEval.Value > maxEval.Value) || // max
                    (whitePlaying != IsWhite && childrenEval.Value < maxEval.Value) || // min
                    maxEval.Move == null)
                {
                    maxEval.Move = move;
                    maxEval.Value = childrenEval.Value;
                }
                if (whitePlaying == IsWhite)
                    alpha = Math.Max(alpha, childrenEval.Value);
                else
                    beta = Math.Min(beta, childrenEval.Value);
                if (beta <= alpha)
                {// cutoff
                    return true;
                }
            }
            return false;
        }
        public Move ChooseBestMove(ImmutableDictionary<Square, IPiece> squares)
        {
            MoveValue result = EvaluateBestMoveParallel(squares, 3, IsWhite, int.MinValue, int.MaxValue);
            return result.Move.GetValueOrDefault();
        }
    }
}
