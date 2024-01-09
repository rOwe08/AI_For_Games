﻿namespace Prong
{
    public class StaticState
    {
        public int ClientSize_Width = 0;
        public int ClientSize_Height = 0;
        public int gridCellSize = 10;
        public int paddleWidthCells = 2;
        public int paddleHeightCells = 10;
        public float ballSpeed = 800;
        public float paddle1Speed = 500;
        public float paddle2Speed = 500;

        public int paddleWidth()
        {
            return gridCellSize * paddleWidthCells;
        }

        public int paddleHeight()
        {
            return gridCellSize * paddleHeightCells;
        }
    }
}
