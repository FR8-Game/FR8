using System;
using UnityEngine;

namespace FR8.Utility
{
    public static class Search
    {
        public static int FuzzySearch(Func<int, string> source, int length, string search)
        {
            int getScore(string a, string b)
            {
                var matrix = new int[a.Length + 1, b.Length + 1];

                for (var i = 0; i < matrix.GetLength(0); i++)
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    if (i == 0 || j == 0)
                    {
                        matrix[i, j] = i > j ? i : j;
                        continue;
                    }

                    var ac = a[i - 1];
                    var bc = b[i - 1];

                    var s1 = matrix[i - 1, j] + 1;
                    var s2 = matrix[i, j - 1] + 1;
                    var s3 = matrix[i - 1, j - 1] + (ac == bc ? 0 : 1);

                    matrix[i, j] = Mathf.Min(s1, s2, s3);
                }

                var score = 0;
                var l = Mathf.Max(matrix.GetLength(0), matrix.GetLength(1));
                for (var i = 1; i < l; i++)
                for (var j = 1; j < l; j++)
                {
                    score += matrix[Mathf.Min(i, matrix.GetLength(0) - 1), Mathf.Min(j, matrix.GetLength(1) - 1)];
                }
                return score;
            }

            var bestScore = getScore(search, source(0));
            var best = 0;
            for (var i = 1; i < length; i++)
            {
                var score = getScore(search, source(i));
                if (score >= bestScore) continue;
                
                bestScore = score;
                best = i;
            }
            return best;
        }
    }
}