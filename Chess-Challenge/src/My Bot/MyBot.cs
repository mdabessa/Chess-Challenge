using ChessChallenge.API;
using System;
using System.Collections.Generic;

public class MyBot : IChessBot
{
    public int CalculateDepth(double time, double maxTime)
    {
        if (time < 1000)
            return 3;
        else if ((time / maxTime) > 0.95) // avoid wasting time on opening
            return 4;
        else
            return 5;
    }

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

    public double _Think(Board board, bool color, int depth, double alpha, double beta)
    {
        if (depth == 1)
            return Score(board, color);

        List<(Move, double)> moves = new();
        foreach(Move move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            double score = Score(board, color);
            moves.Add((move, score));
            board.UndoMove(move);
        }

        moves.Sort((a, b) => b.Item2.CompareTo(a.Item2));

        // best 10 moves and worst 10 moves
        int minIndex = Math.Min(moves.Count, 10);
        int maxIndex = Math.Max(0, moves.Count - 10);

        double maxScore = double.NegativeInfinity;
        for( int i = 0; i < moves.Count; i++)
        {
            if (i > minIndex && i < maxIndex)
                continue;

            Move move = moves[i].Item1;
            board.MakeMove(move);
            double score = _Think(board, !color, depth - 1, alpha, beta);
            board.UndoMove(move);

            maxScore = Math.Max(maxScore, score);
            alpha = Math.Max(alpha, score);

            if (beta <= alpha)
                break;
        }

        return maxScore;
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
            //int depth = CalculateDepth(timer.MillisecondsRemaining, timer.GameStartTimeMilliseconds);
            double score = _Think(board, board.IsWhiteToMove, 3, alpha, beta);
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