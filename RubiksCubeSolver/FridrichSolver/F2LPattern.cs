using RubiksCubeLib;
using RubiksCubeLib.Solver;

using System.Collections.Generic;

namespace FridrichSolver
{
  public class F2LPattern : PatternTable
  {
    private Dictionary<Pattern, Algorithm> patterns;

    public override Dictionary<Pattern, Algorithm> Patterns => this.patterns;

      CubeFlag CornerTargetPos { get; }
    public CubeFlag EdgeTargetPos { get; set; }

    public F2LPattern(CubeFlag edgeTargetPos, CubeFlag cornerTargetPos, CubeFlag rightSlice, CubeFlag frontSlice)
    {
      this.CornerTargetPos = cornerTargetPos;
      this.EdgeTargetPos = edgeTargetPos;
        this.InitPatterns(rightSlice, frontSlice);
    }

    private void InitPatterns(CubeFlag r, CubeFlag f)
    {
      var l = CubeFlagService.GetOppositeFlag(r);
      var b = CubeFlagService.GetOppositeFlag(f);
      var rIsX = CubeFlagService.IsXFlag(r);

      // edge orientation changes depending on the target slot
      var correct = (r == CubeFlag.BackSlice && f == CubeFlag.RightSlice) || (f == CubeFlag.LeftSlice && r == CubeFlag.FrontSlice)
        ? Orientation.Correct : Orientation.Clockwise;
      var clockwise = correct == Orientation.Correct ? Orientation.Clockwise : Orientation.Correct;

        this.patterns = new Dictionary<Pattern, Algorithm> {
        #region Corner correct oriented at targetposition 
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | f | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice)), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("U {0} U {0}' U' {1}' U' {1}",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | r | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice)), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("U' {1}' U' {1} U {0} U {0}'",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0}2 U2 {1} {0}2 {1}' U2 {0}' U {0}'",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        #endregion

        #region Corner clockwise oriented at target position
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | f | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice)), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U {0} U' {0}' U {0}",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | r | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice)), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0} U {0}' U' {0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0} U2 {0} U {0}' U {0} U2 {0}2",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0} U {0}' U' {0} U' {0}' U2 {1}' U' {1}",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        #endregion

        #region Corner counter-clockwise oriented at target position
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | f | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice)), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U' {0} U {0}' U' {0}",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | r | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice)), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0} U' {0}' U {0} U' {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U' {0} U2 {0}' U {0} U' {0}' U' {0}",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("{1}' U' {1} U {1}' U {1} U2 {0} U {0}'",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        #endregion

        #region Corner correct oriented in top layer
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0} U {0}' U' {0} U {0}' U' {0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0} U' {0}' U {1}' U {1}",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | b), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("U {0} U2 {0}' U {0} U' {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | b), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U {0} U2 {0}' U' {0}",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | l), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0} U' {0}' U2 {0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | l), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("U' {0}' U2 {0} U' {0}' U {0}",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | f), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U2 {0} U {0}' U' {0}",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | f), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U' {1}' U {1} {0} {1}' U {1}",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | r), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0} U2 {0}' U' {0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | r), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Correct, this.CornerTargetPos)
            }),
          new Algorithm("{0} U {1} U' {1}' {0}' {1} U' {1}'",CubeFlagService.ToNotationString(f), CubeFlagService.ToNotationString(r))
        },
        #endregion

        #region Corner counter-clockwise oriented in top layer
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("U' {0} U' {0}' U2 {0} U' {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("U' {0} U {0}' U {1}' U' {1}",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | r), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("U {0} U' {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | r), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0} U2 {0}2 U' {0}2 U' {0}'",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | b), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("U' {0} U {0}' U2 {0} U' {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | b), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("U' {0} U' {0}' U {1}' U' {1}",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | l), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U' {0}",CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | l), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("U' {0} U2 {0}' U2 {0} U' {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | f), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("d {0}' U {0} U' {0}' U' {0}",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | f), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.CounterClockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U {0} U2 {1} U {1}'",CubeFlagService.ToNotationString(f), CubeFlagService.ToNotationString(r))
        },
        #endregion

        #region Corner clockwise oriented in top layer
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("U {0} U {0}' U2 {0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(this.EdgeTargetPos), Orientation.Clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("U2 {1}' U {1} U {0} U {0}'",CubeFlagService.ToNotationString(r), CubeFlagService.ToNotationString(f))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | r), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("U' {0} U' {0}' U {0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | r), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("{1} U' {1}' U2 {0}' U' {0}",CubeFlagService.ToNotationString(f), CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | b), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("d {0}' U2 {0} U2 {0}' U {0}",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | b), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | l), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("d {0}' U' {0} U2 {0}' U {0}",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (!rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | l), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("d {0}' U {0} d' {0} U {0}'",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | f), clockwise, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("{0}' U2 {0}2 U {0}2 U {0}",CubeFlagService.ToNotationString(r))
        },
        {
          new Pattern(new List<PatternItem> {
              new PatternItem(new CubePosition(CubeFlag.TopLayer | (rIsX ? CubeFlag.MiddleSliceSides : CubeFlag.MiddleSlice) | f), correct, this.EdgeTargetPos),
              new PatternItem(new CubePosition(this.CornerTargetPos &~ CubeFlag.BottomLayer | CubeFlag.TopLayer), Orientation.Clockwise, this.CornerTargetPos)
            }),
          new Algorithm("U' {0}' U {0}",CubeFlagService.ToNotationString(f))
        }
        #endregion
      };
    }
  }
}
