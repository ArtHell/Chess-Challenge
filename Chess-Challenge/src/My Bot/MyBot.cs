using ChessChallenge.API;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        return FindTheBestMove(board);
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

    private Move FindTheBestMove(Board board)
    {
        float maxEval = -90000f;
        Move bestMove = Move.NullMove;

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            var evaluation = -Negamax(board, 3);
            board.UndoMove(move);
            if (evaluation > maxEval)
            {
                maxEval = evaluation;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private float Negamax(Board board, int depth)
    {
        var legalMoves = board.GetLegalMoves();
        if (depth == 0 || legalMoves.Length == 0)
            return EvaluatePosition(board);

        var evaluation = -90000f;
        foreach (var move in legalMoves)
        {
            board.MakeMove(move);
            var newEvaluation = -Negamax(board, depth - 1);
            if (newEvaluation > evaluation)
                evaluation = newEvaluation;
            board.UndoMove(move);
        }
        return evaluation;
    }
}