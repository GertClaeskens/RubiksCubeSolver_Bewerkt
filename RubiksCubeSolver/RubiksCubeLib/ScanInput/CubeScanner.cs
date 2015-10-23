using RubiksCubeLib.RubiksCube;
using System;

namespace RubiksCubeLib.ScanInput
{
  public abstract class CubeScanner
  {
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Rubik Scan();
    public abstract void StartAsync();
    public EventHandler<ScanEventArgs> ScanAsyncFinished;
  }
}
