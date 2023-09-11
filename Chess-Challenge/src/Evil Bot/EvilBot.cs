using ChessChallenge.API;
using System;
using System.Collections.Generic;

namespace ChessChallenge.Example
{
    // A bot that uses the minimax algorithm to find the best move with depth 4.
    public class EvilBot : IChessBot
    {
        public double Score(Board board, bool color)
        {
            if (board.IsInCheckmate())
            {
                if (board.IsWhiteToMove == color)
                    return double.NegativeInfinity;
                else
                    return double.PositiveInfinity;
            }

            Dictionary<PieceType, int> values = new Dictionary<PieceType, int>
        {
            { PieceType.Pawn, 100 },
            { PieceType.Knight, 300 },
            { PieceType.Bishop, 300 },
            { PieceType.Rook, 500 },
            { PieceType.Queen, 900 },
            { PieceType.King, 0 }
        };

            double score = 0.0;
            PieceList[] pieces = board.GetAllPieceLists();

            foreach (PieceList pieceList in pieces)
            {
                foreach (Piece piece in pieceList)
                {
                    if (piece.IsWhite != color)
                        score += values[piece.PieceType];
                    else
                        score -= values[piece.PieceType];
                }
            }

            return score;
        }

        public double Maximize(Board board, bool color, int deep, double alpha, double beta)
        {
            if (deep == 1)
                return Score(board, color);

            double maxScore = double.NegativeInfinity;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                double score = Minimize(board, color, deep - 1, alpha, beta);
                board.UndoMove(move);

                maxScore = Math.Max(maxScore, score);
                alpha = Math.Max(alpha, score);

                if (beta <= alpha)
                    break;
            }

            return maxScore;
        }


        public double Minimize(Board board, bool color, int deep, double alpha, double beta)
        {
            if (deep == 1)
                return Score(board, color);

            double minScore = double.PositiveInfinity;
            foreach (Move move in board.GetLegalMoves())
            {
                board.MakeMove(move);
                double score = Maximize(board, color, deep - 1, alpha, beta);
                board.UndoMove(move);

                minScore = Math.Min(minScore, score);
                beta = Math.Min(beta, score);

                if (beta <= alpha)
                    break;
            }

            return minScore;
        }


        public Move Think(Board board, Timer timer)
        {
            Move[] moves = board.GetLegalMoves();

            // list with only the first move of moves
            List<Move> bestMoves = new();
            double bestScore = double.NegativeInfinity;
            double alpha = double.NegativeInfinity;
            double beta = double.PositiveInfinity;

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                double score = Minimize(board, board.IsWhiteToMove, 4, alpha, beta);
                board.UndoMove(move);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(move);
                }
            }


            Random random = new();
            Move bestMove = bestMoves[random.Next(bestMoves.Count)];
            return bestMove;
        }
    }

    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot1 : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

        public Move Think(Board board, Timer timer)
        {
            Move[] allMoves = board.GetLegalMoves();

            // Pick a random move to play if nothing better is found
            Random rng = new();
            Move moveToPlay = allMoves[rng.Next(allMoves.Length)];
            int highestValueCapture = 0;

            foreach (Move move in allMoves)
            {
                // Always play checkmate in one
                if (MoveIsCheckmate(board, move))
                {
                    moveToPlay = move;
                    break;
                }

                // Find highest value capture
                Piece capturedPiece = board.GetPiece(move.TargetSquare);
                int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

                if (capturedPieceValue > highestValueCapture)
                {
                    moveToPlay = move;
                    highestValueCapture = capturedPieceValue;
                }
            }

            return moveToPlay;
        }

        // Test if this move gives checkmate
        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            bool isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }
    }
}