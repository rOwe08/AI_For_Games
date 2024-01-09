using Prong;
using System;

class PlayerAIReactive : Player
{
    private const float MovementThreshold = 1.0f;

    public PlayerAction GetAction(StaticState config, DynamicState state)
    {
        float paddleCenter = state.plr1PaddleY + config.paddleHeight() / 2;
        float distanceToBall = state.ballY - paddleCenter;

        if (Math.Abs(distanceToBall) > MovementThreshold)
        {
            if (distanceToBall > 0) return PlayerAction.UP;
            if (distanceToBall < 0) return PlayerAction.DOWN;
        }

        return PlayerAction.NONE;
    }
}
