namespace Prong
{
    class PongEnginePerf
    {
        private StaticState config;
        private DynamicState state;
        private PongEngine engine;

        public PongEnginePerf()
        {
            config = new StaticState();
            config.ClientSize_Width = 640;
            config.ClientSize_Height = 480;

            state = new DynamicState();

            engine = new PongEngine(config);
        }

        public void Run()
        {
            engine.Tick(state, PlayerAction.NONE, PlayerAction.NONE, 0.05f);
        }

        public void Perf(long iterations)
        {
            TimeIt.ExecuteAndReport("PongEngine", this.Run, iterations);
        }

        public void Perf(double maxTimeSecs)
        {
            TimeIt.ExecuteAndReport("PongEngine", this.Run, maxTimeSecs);
        }

        public static void RunPerf()
        {
            PongEnginePerf enginePerf = new PongEnginePerf();
            enginePerf.Perf(1000000);
            enginePerf.Perf(0.05);
        }
    }
}
