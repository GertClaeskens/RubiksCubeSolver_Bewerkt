using System;

namespace RubiksCubeLib.Solver
{
    public class SolutionErrorEventArgs : EventArgs
    {
        public string Step { get; }
        public string Message { get; }

        public SolutionErrorEventArgs(string step, string message)
        {
            this.Step = step;
            this.Message = message;
        }
    }
}
