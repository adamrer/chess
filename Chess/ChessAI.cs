﻿using Chess.Pieces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Chess
{
    internal class ChessAI
    {
        bool IsWhite { get; init; }
        public ChessAI(bool isWhite)
        {
            IsWhite = isWhite;
        }

        private int CalculateMobilityScore(Board board)
        {
            return ChessRules.GetAvailableMoves(board, IsWhite).Count / 10;
        }
        private int CalculateKingSafetyScore(Board board, bool white)
        {
            Square kingSquare = board.WhitePieces['K'][0];
            if (!white)
                kingSquare = board.BlackPieces['k'][0];

            int score = 0;
            foreach ((int, int) moveVector in board.Squares[kingSquare].MoveVectors)
            {
                Square square = new Square(kingSquare.Row + moveVector.Item1, kingSquare.Column + moveVector.Item2);
                if (board.Squares.ContainsKey(square) && board.Squares[square] is not NoPiece) 
                {
                    score += 1;
                }
                else if (!board.Squares.ContainsKey(square))
                {
                    score += 1;
                }

            }
            return score;
        }
        private int CalculateMaterialScore(Board board)
        {
            int score = 0;
            foreach (Square square in board.Squares.Keys)
            {
                IPiece piece = board.Squares[square];
                if (piece is not NoPiece )
                {
                    if (piece.IsWhite == IsWhite)
                        score += piece.Value *3;
                    else
                        score -= piece.Value *3;
                }
            }
            return score;
        }
        private int CalculateCenterScore(Board board)
        {// políčka d4, e4, d5, e5 jsou zabraná
            int score = 0;
            
            List<Square> centerSquares = new List<Square>() { 
                // d4, d5, e4, e5
                new Square(4, 4), new Square(5, 4), new Square(4, 5), new Square(5, 5) 
            };
            foreach (Square centerSquare in centerSquares)
            {
                if (board.Squares[centerSquare] is not NoPiece)
                {
                    if (board.Squares[centerSquare].IsWhite == IsWhite)
                        score += 3;
                    else
                        score -= 3;
                }
            }

            return score;
        }
        private int EvaluateBoardScore(Board board, bool white)
        {
            int totalScore = CalculateKingSafetyScore(board, white) + 
                CalculateMobilityScore(board) + 
                CalculateCenterScore(board) + 
                CalculateMaterialScore(board);

            return totalScore;
        }
        
        private MoveValue EvaluateBestMoveParallel(Board board, 
            int depth, bool whitePlaying, int alpha, int beta)
        {//minimax

            MoveValue maxEval = new MoveValue(null, int.MinValue);
            if (whitePlaying != IsWhite)
                maxEval.Value = int.MaxValue;
            var moves = ChessRules.GetAvailableMoves(board, whitePlaying);

            Parallel.ForEach(moves, move =>
            {
                MoveValue childrenEval = EvaluateBestMove(ChessRules.MakeMove(move, board), depth - 1, !whitePlaying, alpha, beta);

                if (MinimaxStep(move, alpha, beta, whitePlaying, maxEval, childrenEval))
                    return;
            });

            return maxEval;


        }
        
        private MoveValue EvaluateBestMove(Board board,
            int depth, bool whitePlaying, int alpha, int beta)
        {
            int boardEvaluation = ChessRules.EvaluateBoard(board, whitePlaying);
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
                    return new MoveValue(null, EvaluateBoardScore(board, whitePlaying));
            }

            MoveValue maxEval = new MoveValue(null, int.MinValue);
            if (whitePlaying != IsWhite)
                maxEval.Value = int.MaxValue;
            var moves = ChessRules.GetAvailableMoves(board, whitePlaying);
            foreach (Move move in moves)
            {
                MoveValue childrenEval = EvaluateBestMove(ChessRules.MakeMove(move, board), depth - 1, !whitePlaying, alpha, beta);

                if (MinimaxStep(move, alpha, beta, whitePlaying, maxEval, childrenEval))
                    break;
            }

            return maxEval;

        }
        private bool MinimaxStep(Move move, int alpha, int beta, bool whitePlaying, MoveValue maxEval, MoveValue childrenEval)
        {// true when cutoff must happen
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
            return false;
        }
        public Move ChooseBestMove(Board board)
        {
            MoveValue result = EvaluateBestMoveParallel(board, 2, IsWhite, int.MinValue, int.MaxValue);

            return result.Move.GetValueOrDefault();
        }
    }
}
