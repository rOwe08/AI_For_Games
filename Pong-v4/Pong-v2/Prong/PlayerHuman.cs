﻿using OpenTK.Input;

namespace Prong
{
    class PlayerHuman : Player
    {
        public Key Up { get; }
        public Key Down { get; }

        public PlayerHuman(OpenTK.Input.Key up, OpenTK.Input.Key down)
        {
            this.Up = up;
            this.Down = down;
        }

        public PlayerAction GetAction(StaticState config, DynamicState state)
        {
            if (Keyboard.GetState().IsKeyDown(Up)) return PlayerAction.UP;
            if (Keyboard.GetState().IsKeyDown(Down)) return PlayerAction.DOWN;
            return PlayerAction.NONE;
        }
    }
}
