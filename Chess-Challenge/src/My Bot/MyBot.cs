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
  
    public double Maximize(Board board, bool color, int deep)
    {
        if (deep == 1)
            return Score(board, color);

        double maxScore = double.NegativeInfinity;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            double score = Minimize(board, color, deep - 1);
            board.UndoMove(move);

            maxScore = Math.Max(maxScore, score);
        }

        return maxScore;
    }

    public double Minimize(Board board, bool color, int deep)
    {
        if (deep == 1)
            return Score(board, color);

        double minScore = double.PositiveInfinity;
        foreach (Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            double score = Maximize(board, color, deep - 1);
            board.UndoMove(move);

            minScore = Math.Min(minScore, score);
        }

        return minScore;
    }

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move bestMove = moves[0];
        double bestScore = double.NegativeInfinity;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            double score = Minimize(board, board.IsWhiteToMove, 4);
            board.UndoMove(move);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        Console.WriteLine($"Best score: {bestScore}");
        return bestMove;
    }
}