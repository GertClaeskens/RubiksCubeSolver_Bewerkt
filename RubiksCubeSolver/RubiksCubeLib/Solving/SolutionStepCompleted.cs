using System;

namespace RubiksCubeLib.Solver
{
    public class SolutionStepCompletedEventArgs : EventArgs
    {
        public bool Finished { get; }
        public Algorithm Algorithm { get; }
        public int Milliseconds { get; }
        public string Step { get; }
        public SolutionStepType Type { get; }

        public SolutionStepCompletedEventArgs(string step, bool finished, Algorithm moves, int milliseconds, SolutionStepType type = SolutionStepType.Standard)
        {
            this.Step = step;
            this.Finished = finished;
            this.Algorithm = moves;
            this.Milliseconds = milliseconds;
            this.Type = type;
        }
    }
}