using System.Collections.Generic;
using System.Linq;

namespace RubiksCubeLib.Solver
{
  /// <summary>
  /// Represents a pattern table to search for equivalent patterns easily
  /// </summary>
  public abstract class PatternTable
  {
    /// <summary>
    /// Collection of comparison patterns with related algorithms
    /// </summary>
    public abstract Dictionary<Pattern, Algorithm> Patterns { get; }

      /// <summary>
      /// Finds all possible algorithms for this pattern
      /// </summary>
      /// <param name="p">Current rubik pattern</param>
      /// <param name="rotationLayer">Transformation rotation</param>
      /// <param name="filter"></param>
      /// <returns>Returns all possible solutions for this pattern</returns>
      public Dictionary<Pattern, Algorithm> FindMatches(Pattern p, CubeFlag rotationLayer, PatternFilter filter)
    {
      var transformedPatterns = this.Patterns.ToDictionary(kvp => kvp.Key.DeepClone(), a => a.Value); // clone
      var filteredPatterns = transformedPatterns.Where(kvp => filter.Filter(p, kvp.Key)).ToDictionary(pa => pa.Key, a => a.Value); // filter
      filteredPatterns = filteredPatterns.OrderByDescending(k => k.Key.Probability).ToDictionary(pa => pa.Key.DeepClone(), a => a.Value); // order by probability

      var matches = new Dictionary<Pattern, Algorithm>();
      // 4 possible standard transformations
      for (var i = 0; i < 4; i++)
      {
        // Get matches
        foreach (var kvp in filteredPatterns.Where(pa => p.IncludesAllPatternElements(pa.Key)))
        {
          matches.Add(kvp.Key, kvp.Value); // Add to matches
        }
        if (rotationLayer == CubeFlag.None) return matches;

        if (filter.OnlyAtBeginning)
        {
          transformedPatterns = filteredPatterns.Except(matches).ToDictionary(pa => pa.Key.Transform(rotationLayer), a => a.Value.Transform(rotationLayer));
          filteredPatterns = transformedPatterns;
        }
        else
        {
          transformedPatterns = transformedPatterns.ToDictionary(pa => pa.Key.Transform(rotationLayer), a => a.Value.Transform(rotationLayer));
          filteredPatterns = transformedPatterns.Where(kvp => filter.Filter(p, kvp.Key)).ToDictionary(pa => pa.Key, a => a.Value);
        }
      }
      return matches;
    }

      /// <summary>
      /// Searches for the best algorithm for the given pattern
      /// </summary>
      /// <param name="p">Current rubik pattern</param>
      /// <param name="rotationLayer">Transformation layer</param>
      /// <param name="filter"></param>
      /// <returns>Returns the best match</returns>
      public Algorithm FindBestMatch(Pattern p, CubeFlag rotationLayer, PatternFilter filter)
    {
      var matches = this.FindMatches(p, rotationLayer, filter);
      var bestAlgo = matches.OrderByDescending(item => item.Key.Items.Count).FirstOrDefault().Value;
      return bestAlgo;
    }
  }
}
