using Prong;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;

public class Node
{
    public DynamicState nodeState;
    public float gCost;
    public float hCost;
    public Node parent;
    public float fCost;
}
public class NodeComparer : IComparer<Node>
{
    public int Compare(Node x, Node y)
    {
        int result = x.fCost.CompareTo(y.fCost);
        if (result == 0)
        {
            result = x.gCost.CompareTo(y.gCost);
            if (result == 0)
            {
                result = x.nodeState.plr2PaddleY.CompareTo(y.nodeState.plr2PaddleY);
            }
        }
        return result;
    }
}

public class PlayerAIAstar : Player
{
    private PongEngine engine;
    private Player opponent;

    public PlayerAIAstar(PongEngine engine, Player opponent)
    {
        this.engine = engine;
        this.opponent = opponent;
    }
    public float CalculateHeuristic(DynamicState state, StaticState config)
    {
        float paddleTop = state.plr2PaddleY + config.paddleHeight() / 2;
        float paddleBottom = state.plr2PaddleY - config.paddleHeight() / 2;

        float nearestPaddleY = Math.Max(paddleBottom, Math.Min(paddleTop, state.ballY));

        float distanceX = Math.Abs(state.ballX - plr2PaddleBounceX(config));
        float distanceY = Math.Abs(state.ballY - nearestPaddleY);

        return (float)Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
    }


    //private bool IsGoal(Node current, StaticState config)
    //{
    //    float paddleTop = current.nodeState.plr2PaddleY + config.paddleHeight() / 2;
    //    float paddleBottom = current.nodeState.plr2PaddleY - config.paddleHeight() / 2;

    //    //Console.WriteLine(current.nodeState.ballY >= paddleBottom && current.nodeState.ballY <= paddleTop);

    //    return current.nodeState.ballY >= paddleBottom && current.nodeState.ballY <= paddleTop;
    //}

    private bool IsGoal(Node current, StaticState config)
    {
        engine.SetState(current.nodeState);

        if (engine.ballHitsRightPaddle())
        {
            return true;
        }

        return false;
    }


    private int plr2PaddleBounceX(StaticState config)
    {
        return config.ClientSize_Width / 2 - config.paddleWidth() / 2;
    }

    private PlayerAction ReconstructPath(Node current)
    {
        List<Node> path = new List<Node>();
        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }
        path.Reverse();

        if (path.Count < 2)
        {
            return PlayerAction.NONE;
        }

        float nextPaddleY = path[1].nodeState.plr2PaddleY;
        float currentPaddleY = path[0].nodeState.plr2PaddleY;

        if (nextPaddleY > currentPaddleY)
        {
            //Console.WriteLine("Reconstructed action: UP");
            return PlayerAction.UP;
        }
        else if (nextPaddleY < currentPaddleY)
        {
            //Console.WriteLine("Reconstructed action: DOWN");
            return PlayerAction.DOWN;
        }
        else
        {
            return PlayerAction.NONE;
        }
    }
    private DynamicState AdvanceState(PlayerAction myAction, DynamicState currentState, float timeDelta)
    {
        PlayerAction opponentAction = opponent.GetAction(engine.Config, currentState);

        DynamicState nextState = currentState.Clone();
        engine.SetState(nextState);
        engine.Tick(nextState, opponentAction, myAction, timeDelta);

        return nextState;
    }

    public PlayerAction GetAction(StaticState config, DynamicState state)
    {
        float timeDelta = 1.0f / 60.0f;
        var openSet = new SortedSet<Node>(new NodeComparer());
        var closedSet = new Dictionary<float, Node>();
        Stopwatch stopwatch = new Stopwatch();

        Node startNode = new Node
        {
            nodeState = state,
            gCost = 0,
            hCost = CalculateHeuristic(state, config),
            fCost = 0
        };

        openSet.Add(startNode);
        stopwatch.Start();

        while (openSet.Count > 0 && stopwatch.Elapsed.TotalSeconds < 0.05)
        {
            Node current = openSet.Min;
            openSet.Remove(current);

            if (IsGoal(current, config))
            {
                return ReconstructPath(current);
            }

            closedSet[current.nodeState.plr2PaddleY] = current;

            foreach (PlayerAction myAction in Enum.GetValues(typeof(PlayerAction)))
            {
                DynamicState nextDynamicState = AdvanceState(myAction, current.nodeState, timeDelta);
                Node nextNode = new Node
                {
                    nodeState = nextDynamicState,
                    gCost = current.gCost + 1,
                    parent = current,
                    hCost = CalculateHeuristic(nextDynamicState, config)
                };
                nextNode.fCost = nextNode.gCost + nextNode.hCost;

                if (!closedSet.ContainsKey(nextNode.nodeState.plr2PaddleY) || nextNode.fCost < closedSet[nextNode.nodeState.plr2PaddleY].fCost)
                {
                    openSet.Add(nextNode);
                }
            }
        }

        if (stopwatch.Elapsed.TotalSeconds < 0.05)
        {
            // Console.WriteLine("IM HERE");
            if (closedSet.Count > 0)
            {
                Node mostPromisingNode = null;
                float lowestFCost = float.MaxValue;

                foreach (var kvp in closedSet)
                {
                    Node node = kvp.Value;
                    if (node.fCost <= lowestFCost)
                    {
                        mostPromisingNode = node;
                        lowestFCost = node.fCost;
                    }
                }

                if (mostPromisingNode != null)
                {
                    return ReconstructPath(mostPromisingNode);
                }
            }
        }
        return PlayerAction.NONE;
    }
}