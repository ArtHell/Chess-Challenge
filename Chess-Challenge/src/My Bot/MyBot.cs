using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    private int foundCounter = 0;
    enum EntryType
    {
        Exact,
        LowerBound,
        UpperBound
    }

    private class TranspositionTableEntry
    {
        public float Evaluation { get; set; }
        public int Depth { get; set; }

        public EntryType EntryType { get; set; }
    }

    private Dictionary<ulong, TranspositionTableEntry> transpositionTable = new();

    public Move Think(Board board, Timer timer)
    {
        System.Console.WriteLine("Table size");
        System.Console.WriteLine(transpositionTable.Count);
        System.Console.WriteLine("Counter");
        System.Console.WriteLine(foundCounter);
        return FindTheBestMove(board, timer);
    }

    private float EvaluatePosition(Board board)
    {
        if (board.IsInCheckmate())
        {
            return -90000;
        }


        float[] weights = { 100, 305, 333, 563, 950 };
        var allPiecesCounts = board.GetAllPieceLists().Select(x => x.Count).ToList();
        var evaluation = 0f;
        for (int i = 0; i < 5; i++)
        {
            evaluation += (allPiecesCounts[i] - allPiecesCounts[i + 6]) * weights[i];
        }

        if (board.IsDraw())
        {
            evaluation /= 10;
        }

        return board.IsWhiteToMove ? evaluation : -evaluation;
    }

    private Move FindTheBestMove(Board board, Timer timer)
    {
        float maxEval = -100000f;
        Move bestMove = Move.NullMove;
        for(var depth = 1; timer.MillisecondsElapsedThisTurn < 1000; depth++)
        {
            System.Console.WriteLine(timer.MillisecondsElapsedThisTurn);
            foreach (var move in board.GetLegalMoves())
            {
                board.MakeMove(move);

                var evaluation = -Negamax(board, depth, -90000, 90000);
                board.UndoMove(move);
                if (evaluation > maxEval)
                {
                    maxEval = evaluation;
                    bestMove = move;
                }
            }
        }
        

        return bestMove;
    }

    private float Negamax(Board board, int depth, float alpha, float beta)
    {
        var legalMoves = board.GetLegalMoves();

        var ttEntry = transpositionTable.GetValueOrDefault(board.ZobristKey);
        if (ttEntry != null && ttEntry.Depth >= depth)
        {
            foundCounter++;
            if (ttEntry.EntryType == EntryType.Exact)
            {
                return ttEntry.Evaluation;
            } else if(ttEntry.EntryType == EntryType.LowerBound)
            {
                alpha = System.Math.Max(alpha, ttEntry.Evaluation);
            } else if(ttEntry.EntryType == EntryType.UpperBound)
            {
                beta = System.Math.Min(beta, ttEntry.Evaluation);
            }
        }

        if (depth == 0 || legalMoves.Length == 0)
            return Quiesce(board, alpha, beta);

        var value = -90000f;
        foreach (var move in legalMoves)
        {
            board.MakeMove(move);
            value = Math.Max(value, -Negamax(board, depth - 1, -beta, -alpha));
            board.UndoMove(move);
            alpha = Math.Max(alpha, value);
            if (alpha >= beta)
                break;
        }

        var entry = new TranspositionTableEntry
        {
            Evaluation = value,
            Depth = depth,
            EntryType = EntryType.Exact
        };

        if(value <= alpha)
        {
            entry.EntryType = EntryType.UpperBound;
        } else if(value >= beta)
        {
            entry.EntryType = EntryType.LowerBound;
        }

        transpositionTable[board.ZobristKey] = entry;

        return value;
    }

    private float Quiesce(Board board, float alpha, float beta)
    {
        var standPat = EvaluatePosition(board);
        if (standPat >= beta)
            return beta;
        if (alpha < standPat)
            alpha = standPat;

        foreach (var move in board.GetLegalMoves(true))
        {
            board.MakeMove(move);
            var score = -Quiesce(board, -beta, -alpha);
            board.UndoMove(move);

            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }
        return alpha;
    }
}