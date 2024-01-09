namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Permissions;
    using System.Threading;
    using UnityEngine;
    using static System.Math;
    class MCTSSearch : ISearch
    {
        public event System.Action<Move> onSearchComplete;

        MoveGenerator moveGenerator;

        Move bestMove;
        int bestEval;
        bool abortSearch;

        MCTSSettings settings;
        Board board;
        Evaluation evaluation;

        System.Random rand;

        // Diagnostics
        public SearchDiagnostics Diagnostics { get; set; }
        System.Diagnostics.Stopwatch searchStopwatch;

        public MCTSSearch(Board board, MCTSSettings settings)
        {
            this.board = board;
            this.settings = settings;
            evaluation = new Evaluation();
            moveGenerator = new MoveGenerator();
            rand = new System.Random();
        }

        public void StartSearch()
        {
            InitDebugInfo();

            // Initialize search settings
            bestEval = 0;
            bestMove = Move.InvalidMove;

            moveGenerator.promotionsToGenerate = settings.promotionsToSearch;
            abortSearch = false;
            Diagnostics = new SearchDiagnostics();

            SearchMoves();

            onSearchComplete?.Invoke(bestMove);

            if (!settings.useThreading)
            {
                LogDebugInfo();
            }
        }

        public void EndSearch()
        {
            if (settings.useTimeLimit)
            {
                abortSearch = true;
            }
        }

        void SearchMoves()
        {
            MCTSNode rootNode = new MCTSNode(board, null, board.WhiteToMove);

            while (!abortSearch)
            {
                MCTSNode node = rootNode;
                node.UntriedMovesList = moveGenerator.GenerateMoves(board, true, true);

                // Selection
                while (node.IsFullyExpanded() && !node.IsLeafNode())
                {
                    node = node.SelectChild();
                    if (node == null)
                    {
                        node = rootNode;
                        break;
                    }
                    node.UntriedMovesList = moveGenerator.GenerateMoves(board, false, true);
                }

                // Expansion
                ExpandNode(node);
                
                // Simulation
                double reward = Simulate(node);

                // Backpropagation
                Backpropagate(node, reward);
            }
        }
        void Backpropagate(MCTSNode node, double reward)
        {
            while (node != null)
            {
                node.NumOfWins += reward;
                node.NumOfVisits++;

                node = node.ParentNode;
            }
        }

        void ExpandNode(MCTSNode node)
        {
            if (!node.IsFullyExpanded() && !node.IsLeafNode())
            {
                Move move = node.UntriedMovesList.Last();
                Board newBoard = node.BoardState.Clone();
                newBoard.MakeMove(move, false);

                node.AddChild(move, newBoard);
            }
        }

        double Simulate(MCTSNode node)
        {
            SimPiece[,] simBoard = node.BoardState.GetLightweightClone();
            int depth = 0;
            bool isGameOver = false;
            bool? winningTeam = null;

            System.Random rand = new System.Random();

            while (depth < settings.playoutDepthLimit && !isGameOver)
            {
                List<SimMove> moves = moveGenerator.GetSimMoves(simBoard, node.Team);
                if (moves.Count == 0)
                {
                    break;
                }

                SimMove move = moves[rand.Next(moves.Count)]; // Choosing random move
                ApplySimMove(simBoard, move);

                bool? kingCaptureResult = CheckForKingCapture(simBoard);
                if (kingCaptureResult.HasValue)
                {
                    isGameOver = true;
                    winningTeam = kingCaptureResult.Value;
                }

                depth++;
            }

            double reward;
            if (isGameOver)
            {
                reward = (node.Team == winningTeam) ? 1 : 0;
            }
            else
            {
                reward = evaluation.EvaluateSimBoard(simBoard, node.Team);
            }

            return reward;

        }
        bool? CheckForKingCapture(SimPiece[,] simBoard)
        {
            bool whiteKingExists = false;
            bool blackKingExists = false;

            for (int i = 0; i < simBoard.GetLength(0); i++)
            {
                for (int j = 0; j < simBoard.GetLength(1); j++)
                {
                    if (simBoard[i, j].code == "wK")
                    {
                        whiteKingExists = true;
                    }
                    else if (simBoard[i, j].code == "bK")
                    {
                        blackKingExists = true;
                    }
                }
            }

            if (!whiteKingExists) return true;
            if (!blackKingExists) return false;
            return null;
        }
        void ApplySimMove(SimPiece[,] simBoard, SimMove move)
        {
            simBoard[move.endCoord1, move.endCoord2] = simBoard[move.startCoord1, move.startCoord2];

            simBoard[move.startCoord1, move.startCoord2] = null;
        }

        void LogDebugInfo()
        {
            // Optional
        }

        void InitDebugInfo()
        {
            searchStopwatch = System.Diagnostics.Stopwatch.StartNew();
            // Optional
        }
    }
}