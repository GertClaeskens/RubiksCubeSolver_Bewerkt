using System;
using System.Linq;

namespace RubiksCubeLib
{

  public class LayerMove : IMove
  {

    // *** CONSTRUCTORS ***

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="layer">Defines the layer to be moved</param>
    /// <param name="direction">Defines the direction (true == clockwise and false == counter-clockwise)</param>
    /// <param name="twice">Defines whether this layer will be turned twice or not</param>
    /// <exception cref="System.Exception">Thrown when layer contains more than one flag</exception>
    public LayerMove(CubeFlag layer, bool direction = true, bool twice = false)
    {
      if (CubeFlagService.CountFlags(layer) == 1)
      {
        this.Layer = layer;
        this.Direction = direction;
        this.Twice = twice;
      }
      else
        throw new Exception("Impossible movement");
    }



    // *** PROPERTIES ***

    /// <summary>
    /// Returns the name (the notation) of this LayerMove
    /// </summary>
    public string Name => $"{this.Layer} {(this.Twice ? "x2" : this.Direction ? "Clockwise" : "Counter-Clockwise")}";

      /// <summary>
    /// Describes the layer of this LayerMove
    /// </summary>
    public CubeFlag Layer { get; set; }

    /// <summary>
    /// Describes the direction of this LayerMove
    /// </summary>
    public bool Direction { get; set; }

    /// <summary>
    /// Describes whether this LayerMove will be executed twice
    /// </summary>
    public bool Twice { get; set; }

    /// <summary>
    /// Returns true if MultipleLayers are allowed
    /// </summary>
    public bool MultipleLayers => false;

      /// <summary>
    /// Gets the reverse move
    /// </summary>
    public IMove ReverseMove => new LayerMove(this.Layer, !this.Direction, this.Twice);

      // *** OPERATORS ***

    /// <summary>
    /// Combines two LayerMoves into a LayerMoveCollection
    /// </summary>
    public static LayerMoveCollection operator &(LayerMove first, LayerMove second)
    {
        var moves = new LayerMoveCollection { first, second };
        return moves;
    }



    // *** METHODS ***

    /// <summary>
    /// Parses a notation string into a LayerMove
    /// </summary>
    /// <param name="notation">Defines to string to be parsed</param>
    /// <returns></returns>
    public static LayerMove Parse(string notation)
    {
      var layer = notation[0].ToString();
      var rotationLayer = CubeFlagService.Parse(layer);

      var ccwChars = new[] { '\'', 'i' };
      var direction = !ccwChars.Any(notation.Contains);

      var twice = notation.Contains("2");
      return new LayerMove(rotationLayer, direction, twice);
    }

    /// <summary>
    /// Parses a notation string into a layer move
    /// </summary>
    /// <param name="notation">String to be parsed</param>
    /// <param name="move">The resulting layer move</param>
    /// <returns>True, if the string was successfully parsed into a layermove</returns>
    public static bool TryParse(string notation, out LayerMove move)
    {
      move = null;
      var layer = notation[0].ToString();
      var rotationLayer = CubeFlagService.Parse(layer);

      var ccwChars = new[] { '\'', 'i' };
      var direction = !ccwChars.Any(notation.Contains);

      var twice = notation.Contains("2");
      if (CubeFlagService.CountFlags(rotationLayer) == 1)
      {
        move = new LayerMove(rotationLayer, direction, twice);
        return true;
      }
      else return false;
    }


    /// <summary>
    /// Converts this LayerMove into a notation string
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      var move = string.Empty;

      switch (this.Layer)
      {
        case CubeFlag.TopLayer:
          move = "U";
          break;
        case CubeFlag.MiddleLayer:
          move = "E";
          break;
        case CubeFlag.BottomLayer:
          move = "D";
          break;
        case CubeFlag.FrontSlice:
          move = "F";
          break;
        case CubeFlag.MiddleSlice:
          move = "S";
          break;
        case CubeFlag.BackSlice:
          move = "B";
          break;
        case CubeFlag.LeftSlice:
          move = "L";
          break;
        case CubeFlag.MiddleSliceSides:
          move = "M";
          break;
        case CubeFlag.RightSlice:
          move = "R";
          break;
      }

      if (!this.Direction)
        move += "'";

      if (this.Twice)
        move += "2";

      return move;
    }

    /// <summary>
    /// True, if the item accomplishes the equality conditions
    /// </summary>
    /// <param name="obj">Layer move to be compared</param>
    public override bool Equals(object obj)
    {
        if (!(obj is LayerMove))
        {
            return false;
        }
        var move = (LayerMove)obj;
        return this.Direction == move.Direction && this.Layer == move.Layer && this.Twice == move.Twice;
    }

    public override int GetHashCode() => base.GetHashCode();

      /// <summary>
    /// Transforms the layer move
    /// </summary>
    /// <param name="rotationLayer">Transformation layer</param>
    /// <returns>Transformed layer move</returns>
    public IMove Transform(CubeFlag rotationLayer)
    {
      var switchDir = CubeFlagService.IsYFlag(rotationLayer) && this.Layer.HasFlag(CubeFlag.MiddleSlice) ||
        CubeFlagService.IsXFlag(rotationLayer) && this.Layer.HasFlag(CubeFlag.MiddleLayer) ||
        CubeFlagService.IsZFlag(rotationLayer) && this.Layer.HasFlag(CubeFlag.MiddleSliceSides);

      var newMove = new LayerMove(CubeFlagService.NextCubeFlag(this.Layer, rotationLayer), this.Direction ^ switchDir, this.Twice);
      return newMove;
    }
  }
}
