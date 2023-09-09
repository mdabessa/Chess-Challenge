using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
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
            double score = Minimize(board, board.IsWhiteToMove, 5, alpha, beta);
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

        Console.WriteLine($"Actual score: {bestScore}");
        return bestMove;
    }
}