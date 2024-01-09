﻿using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Prong
{
    class Program : GameWindow
    {
        private StaticState config;
        private DynamicState state;
        private PongEngine engine;

        //private Player player1 = new PlayerHuman(Key.W, Key.S);
        private Player player1;
        //private Player player2 = new PlayerHuman(Key.Up, Key.Down);
        //private Player player2 = new PlayerAIRandom();
        //private Player player2 = new PlayerAIReactive();
        private Player player2;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            config = new StaticState();
            config.ClientSize_Height = ClientSize.Height;
            config.ClientSize_Width = ClientSize.Width;
            state = new DynamicState();
            engine = new PongEngine(config);

            player1 = new PlayerAIReactive();
            player2 = new PlayerAIAstar(engine, player1);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            float timeDelta = (float)e.Time;

            PlayerAction plr1 = player1.GetAction(config, state);
            PlayerAction plr2 = player2.GetAction(config, state);

            TickResult result = engine.Tick(state, plr1, plr2, timeDelta);

            if (result == TickResult.PLAYER_1_SCORED)
            {
                Console.WriteLine($"Player score 1: {state.plr1Score}");
            }
            else
            if (result == TickResult.PLAYER_2_SCORED)
            {
                Console.WriteLine($"Player score 2: {state.plr2Score}");
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);

            Matrix4 projection = Matrix4.CreateOrthographic(ClientSize.Width, ClientSize.Height, 0.0f, 1.0f);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            DrawRectangle(state.ballX, state.ballY, config.gridCellSize, config.gridCellSize, 1.0f, 1.0f, 0.0f);
            DrawRectangle(engine.plr1PaddleBounceX(), state.plr1PaddleY, config.paddleWidth(), config.paddleHeight(), 1.0f, 0.0f, 0.0f);
            DrawRectangle(engine.plr2PaddleBounceX(), state.plr2PaddleY, config.paddleWidth(), config.paddleHeight(), 0.0f, 0.0f, 1.0f);

            SwapBuffers();
        }

        void DrawRectangle(float x, float y, int width, int height, float r, float g, float b)
        {
            GL.Color3(r, g, b);

            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(-0.5f * width + x, -0.5f * height + y);
            GL.Vertex2(0.5f * width + x, -0.5f * height + y);
            GL.Vertex2(0.5f * width + x, 0.5f * height + y);
            GL.Vertex2(-0.5f * width + x, 0.5f * height + y);
            GL.End();
        }

        static void Main()
        {
            new Program().Run(60.0, 60.0);
            //PongEnginePerf.RunPerf();
            //DynamicStatePerf.RunPerf();
            //System.Console.ReadLine();
        }
    }
}