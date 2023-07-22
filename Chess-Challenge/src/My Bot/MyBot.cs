using ChessChallenge.API;
using System.Linq;

public class MyBot : IChessBot
{
    public Move Think(Board board, Timer timer)
    {
        return FindTheBestMove(board);
    }

    private int EvaluatePosition(Board board)
    {
        if (board.IsInCheckmate())
        {
            return board.IsWhiteToMove ? -10000 : 10000;
        }

        if (board.IsDraw())
        {
            return 0;
        }

        int[] weights = { 10, 30, 30, 50, 90, 900 };
        var allPiecesCounts = board.GetAllPieceLists().Select(x => x.Count).ToList();
        var evaluation = 0;
        for (int i = 0; i < 6; i++)
        {
            evaluation += (allPiecesCounts[i] - allPiecesCounts[i + 6]) * weights[i];
        }

        return board.IsWhiteToMove ? evaluation : -evaluation;
    }

    private Move FindTheBestMove(Board board)
    {
        int maxEval = -10000;
        Move bestMove = Move.NullMove;

        foreach (var move in board.GetLegalMoves())
        {
            board.MakeMove(move);
            var evaluation = Minimax(board, 3, false);
            board.UndoMove(move);
            if (evaluation > maxEval)
            {
                maxEval = evaluation;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private int Minimax(Board board, int depth, bool isMaximize)
    {
        var legalMoves = board.GetLegalMoves();
        if (depth == 0 || legalMoves.Length == 0)
            return EvaluatePosition(board);

        var evaluation = isMaximize ? -10000 : 10000;
        foreach (var move in legalMoves)
        {
            board.MakeMove(move);
            var newEvaluation = Minimax(board, depth - 1, !isMaximize);
            evaluation = isMaximize ? System.Math.Max(evaluation, newEvaluation) : System.Math.Min(evaluation, newEvaluation);
            board.UndoMove(move);
        }
        return evaluation;
    }
}