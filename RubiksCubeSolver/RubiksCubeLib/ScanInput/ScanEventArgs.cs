using RubiksCubeLib.RubiksCube;
using System;

namespace RubiksCubeLib.ScanInput
{
  public class ScanEventArgs: EventArgs
  {
    public Rubik Rubik { get; set; }
  }
}
