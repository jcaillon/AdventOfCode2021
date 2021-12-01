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
            var depthDifference = depths.Select((depth, index) => index > 0 ? depth - depths[index - 1] : 0);
            Console.WriteLine(depthDifference.Count(diff => diff > 0));
        }
    }
}
