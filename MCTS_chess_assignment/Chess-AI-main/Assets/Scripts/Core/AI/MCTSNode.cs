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
        public int NumOfVisits { get; set; }
        public List<Move> UntriedMovesList { get; set; }

        private double c = 1;

        public MCTSNode(Board boardState, MCTSNode parent, bool team)
        {
            Team = team;
            BoardState = boardState;
            ParentNode = parent;
            ChildrenNodesList = new List<MCTSNode>();
            UntriedMovesList = new List<Move>();
            NumOfWins = 0;
            NumOfVisits = 0;
        }

        public MCTSNode SelectChild()
        {
            MCTSNode selected = null;
            double bestValue = double.MinValue;

            foreach (MCTSNode childNode in ChildrenNodesList)
            {
                double ucbValue = UCB(childNode);
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
            ChildrenNodesList.Add(new MCTSNode(state, this, !Team));
            UntriedMovesList.Remove(move);
        }

        public void Update(double result)
        {
            NumOfVisits++;
            NumOfWins += (int)result;
        }

        public bool IsFullyExpanded()
        {
            return UntriedMovesList.Count == 0;
        }

        public bool IsLeafNode()
        {
            //if(Win or Lose or Draw)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
            return false;     
        }

        private double UCB(MCTSNode node)
        {
            if (node.NumOfVisits == 0)
            {
                return double.MaxValue;
            }

            double winRate = (double)node.NumOfWins / node.NumOfVisits;
            double ucbValue = winRate + c * Mathf.Sqrt(Mathf.Log(this.NumOfVisits) / node.NumOfVisits);   // UCB(a_i) = Q(a_i) + c * sqrt((ln(n) / n_i))
            return ucbValue;
        }
    }
}