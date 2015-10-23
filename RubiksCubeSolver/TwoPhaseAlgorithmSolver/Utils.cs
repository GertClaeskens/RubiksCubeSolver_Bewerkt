namespace TwoPhaseAlgorithmSolver
{
  public static class Utils
  {
    public static int Factorial(int number)
    {
      var faculty = 1;
      for (var i = 1; i <= number; i++) faculty *= i;
      return faculty;
    }

    public static int BinomialCoefficient(int n, int k)
    {
      if (n == 0 && (n - k) == -1) return 0;
      return Factorial(n) / (Factorial(k) * Factorial(n - k));
    }

    public static int Decrement(int number, int start, int end)
    {
      number--;
      while (start <= number && end >= number)
      {
        number--;
      }
      return number;
    }
  }
}
