namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Permissions;
    using System.Threading;
    using UnityEditor.Experimental.GraphView;
    using UnityEngine;
    using static System.Math;
    class MCTSSearch : ISearch
    {
        public event System.Action<Move> onSearchComplete;

        MoveGenerator moveGenerator;

        Move bestMove;
        int bestEval;
        bool abortSearch;
        bool ourTeam;

        MCTSNode searchNode;

        MCTSSettings settings;
        Board board;
        Evaluation evaluation;

        System.Random rand;

        int numOfPlays = 0;

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
            MCTSNode rootNode = new MCTSNode(board, null, board.WhiteToMove, new Move());
            ourTeam = !rootNode.Team;           // "jump over" rootNode

            rootNode.UntriedMovesList = moveGenerator.GenerateMoves(board, true, true);
            MCTSNode node = rootNode;
            numOfPlays = 0;

            while (!abortSearch)
            {
                if (settings.maxNumOfPlayouts < numOfPlays)
                {
                    abortSearch = true;
                    if (rootNode.NumOfVisits > 1)
                    {
                        searchNode = rootNode;
                    }
                    break;
                }
                // Selection
                node = Select(node);

                if (node.MoveLeadingToThisNode.Name == "g2-g1")
                {
                    Console.WriteLine("found g2-g1");
                }

                if (node.MoveLeadingToThisNode.Name == "g3-g4")
                {
                    Console.WriteLine("found g3-g4");
                }

                // Expansion
                node = ExpandNode(node);

                // Simulation
                double reward = Simulate(node);

                // Backpropagation
                Backpropagate(node, reward);

                node = rootNode;

                numOfPlays++;
            }

            if (searchNode.MoveLeadingToThisNode.Name == "a1-a1")
            {
                searchNode = UpdateBestNode(searchNode);
            }
            else
            {
                searchNode = UpdateBestNode(searchNode);
                searchNode = UpdateBestNode(searchNode);
            }

            UpdateBestMove(searchNode);
        }
        void UpdateBestMove(MCTSNode node)
        {
            if (node != null)
            {
                bestMove = node.MoveLeadingToThisNode;
            }
        }

        MCTSNode UpdateBestNode(MCTSNode node)
        {
            MCTSNode bestChild = node.ChildrenNodesList
                .OrderByDescending(node => node.NumOfVisits)
                .FirstOrDefault();

            if (bestChild != null)
            {
                return bestChild;
            }
            return node;
        }

        MCTSNode Select(MCTSNode node)
        {
            //Debug.Log($"Main Parent node: {node.MoveLeadingToThisNode.Name}");
            while (node.ChildrenNodesList.Count > 0 || node.UntriedMovesList.Count > 0)
            {
                if (node.UntriedMovesList.Count > 0)
                {
                    //Debug.Log($"Parent node: {node.MoveLeadingToThisNode.Name}");
                    return node;
                }
                else
                {
                    //Debug.Log($"Selected node: {node.MoveLeadingToThisNode.Name}");
                    node = node.SelectChild(!node.Team != ourTeam);
                }
            }
            return node;
        }

        void Backpropagate(MCTSNode node, double reward)
        {
            while (node != null)
            {
                node.Update(reward);

                node = node.GetParentNode();
            }
        }

        MCTSNode ExpandNode(MCTSNode node)
        {
            if (!node.IsFullyExpanded())
            {
                Move move = node.UntriedMovesList.Last();
                Board newBoard = node.BoardState.Clone();
                newBoard.MakeMove(move, false);

                if (node.BoardState.KingSquare[0] != move.TargetSquare && node.BoardState.KingSquare[1] != move.TargetSquare)
                {
                    node.AddChild(move, newBoard);
                    node.ChildrenNodesList.Last().UntriedMovesList = moveGenerator.GenerateMoves(newBoard, false, true);
                    return node.ChildrenNodesList.Last();
                }
                else
                {
                    node.UntriedMovesList.Remove(move);
                }


            }
            return node;
        }

        bool? SimulateRecursive(SimPiece[,] simBoard, int depth, bool team)
        {            
            if (depth >= settings.playoutDepthLimit)
            {
                return null;
            }

            byte kingCaptureResult = CheckForKingCapture(simBoard);

            if (kingCaptureResult == 1)
            {
                return true;
            }
            else if (kingCaptureResult == 2)
            {
                return false;
            }

            List<SimMove> moves = moveGenerator.GetSimMoves(simBoard, team);

            if (moves.Count == 0)
            {
                return null;
            }
            SimMove move = moves[rand.Next(moves.Count)];
            simBoard = ApplySimMove(simBoard, move);

            return SimulateRecursive(simBoard, depth + 1, !team);
        }

        double Simulate(MCTSNode node)
        {
            SimPiece[,] simBoard = node.BoardState.GetLightweightClone();
            int depth = 0;
            bool? winningTeam = null;

            winningTeam = SimulateRecursive(simBoard, depth, node.Team);

            double reward;
            if (winningTeam.HasValue)
            {
                reward = (ourTeam == winningTeam) ? 1 : 0;
            }
            else
            {
                reward = evaluation.EvaluateSimBoard(simBoard, ourTeam);
            }

            return reward;

        }

        private byte CheckForKingCapture(SimPiece[,] simBoard)
        {
            byte result = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (simBoard[x, y] != null && simBoard[x, y].type == SimPieceType.King)
                    {
                        result += simBoard[x, y].team ? (byte)2 : (byte)1;
                    }
                }
            }

            return result;
        }

        SimPiece[,] ApplySimMove(SimPiece[,] simBoard, SimMove move)
        {

            simBoard[move.endCoord1, move.endCoord2] = simBoard[move.startCoord1, move.startCoord2];

            simBoard[move.startCoord1, move.startCoord2] = null;
            return simBoard;
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