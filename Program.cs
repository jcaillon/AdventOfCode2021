using System;
using DotUtilities;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace AdventOfCode
{
    class Program
    {
        static void Main(string[] args) {
            var depths = File.ReadAllLines("input").Select(s => int.Parse(s)).ToList();
            const int slidingWindow = 3;
            var depthSliding = depths
                .Select((depth, index) => depths.Skip(index).Take(slidingWindow).Sum())
                .SkipLast(slidingWindow - 1)
                .ToArray();
            var depthDifference = depthSliding.Select((depth, index) => index > 0 ? depth - depthSliding[index - 1] : 0);
            Console.WriteLine(depthDifference.Count(diff => diff > 0));
        }
    }
}
