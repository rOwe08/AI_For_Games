namespace Chess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing.Reader;
    using UnityEngine;

    public class MCTSNode
    {
        public bool Team { get; private set; }
        public Board BoardState { get; private set; }
        public MCTSNode ParentNode { get; private set; }
        public List<MCTSNode> ChildrenNodesList { get; private set; }
        public double NumOfWins { get; set; }
        public double NumOfVisits { get; set; }
        public List<Move> UntriedMovesList { get; set; }
        public Move MoveLeadingToThisNode { get; private set; }

        private double c = 1;

        public MCTSNode(Board boardState, MCTSNode parent, bool team, Move moveLeadingToThisNode)
        {
            Team = team;
            BoardState = boardState;
            ParentNode = parent;
            ChildrenNodesList = new List<MCTSNode>();
            UntriedMovesList = new List<Move>();
            NumOfWins = 0;
            NumOfVisits = 0;
            MoveLeadingToThisNode = moveLeadingToThisNode;
        }

        public MCTSNode SelectChild(bool IsOpponentsTurn)
        {
            MCTSNode selected = null;
            double bestValue = double.MinValue;

            foreach (MCTSNode childNode in ChildrenNodesList)
            {
                double ucbValue = UCB(childNode, IsOpponentsTurn);
                if (ucbValue > bestValue)
                {
                    selected = childNode;
                    bestValue = ucbValue;
                }
            }
            return selected;
        }

        public MCTSNode GetParentNode()
        {
            return ParentNode;
        }

        public void AddChild(Move move, Board state)
        {
            ChildrenNodesList.Add(new MCTSNode(state, this, !Team, move));
            UntriedMovesList.Remove(move);
        }

        public void Update(double result)
        {
            NumOfVisits++;
            NumOfWins += result;
        }

        public bool IsFullyExpanded()
        {
            return UntriedMovesList.Count == 0;
        }

        private double UCB(MCTSNode node, bool IsOpponentsTurn)
        {
            if (node.NumOfVisits == 0)
            {
                return double.MaxValue;
            }

            double winRate;

            if (IsOpponentsTurn)
            {
                winRate = 1 - (double)node.NumOfWins / node.NumOfVisits;
            }
            else
            {
                winRate = (double)node.NumOfWins / node.NumOfVisits;
            }

            double ucbValue = winRate + c * Mathf.Sqrt(Mathf.Log((float)this.NumOfVisits) / (float)node.NumOfVisits);   // UCB(a_i) = Q(a_i) + c * sqrt((ln(n) / n_i))
            return ucbValue;
        }
    }
}