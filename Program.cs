using System.Diagnostics;
using System.Drawing;

Console.WriteLine(Puzzle.Solve("input-test", true, true));
Console.WriteLine(Puzzle.Solve("input", true, false));

static class Puzzle {
    public static string Solve(string inputFilePath, bool printFinalMap, bool printIntermediateMaps) {
        var inputList = File.ReadAllLines(inputFilePath);
        var dotLocations = inputList.Where(s => s.Contains(',')).Select(s => new Point(int.Parse(s.Split(',')[0]), int.Parse(s.Split(',')[1]))).ToList();
        var origami = new Origami(dotLocations, dotLocations.Select(p => p.X).Max() + 1, dotLocations.Select(p => p.Y).Max() + 1);

        if (printIntermediateMaps)
            origami.PrintToConsole();

        var foldInstructions = inputList.Where(s => s.Contains('=')).Select(s => s.Split(' ')[2]).ToList();
        foreach (var foldInstr in foldInstructions.Select(s => s.Split('='))) {
            if (foldInstr[0] == "y") {
                origami.FoldHorizontally(int.Parse(foldInstr[1]));
            } else {
                origami.FoldVertically(int.Parse(foldInstr[1]));
            }
            if (printIntermediateMaps)
                origami.PrintToConsole();
        }

        if (printFinalMap && !printIntermediateMaps)
            origami.PrintToConsole();

        return $"There are {origami.DotsMap.Count(dot => dot)} dots visible.";
    }
}

class Origami {
    public List<bool> DotsMap { get; private set; }
    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }

    public Origami(List<Point> dotLocations, int mapWidth, int mapHeight) {
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        DotsMap = Enumerable.Repeat(false, mapWidth * mapHeight).ToList();
        dotLocations.ForEach(p => DotsMap[p.X + p.Y * MapWidth] = true);
    }

    public void FoldHorizontally(int line) {
        var nbLinesToFold = MapHeight - line - 1;
        for (int foldedLine = 1; foldedLine <= nbLinesToFold; foldedLine++) {
            for (int i = 0; i < MapWidth; i++) {
                var currentIndex = i + (line + foldedLine) * MapWidth;
                var newIndex = i + (line - foldedLine) * MapWidth;
                DotsMap[newIndex] = DotsMap[newIndex] || DotsMap[currentIndex];
            }
        }
        MapHeight = line;
        DotsMap = DotsMap.SkipLast((nbLinesToFold + 1) * MapWidth).ToList();
        Debug.Assert(DotsMap.Count == MapWidth * MapHeight);
    }

    public void FoldVertically(int column) {
        var nbLinesToFold = MapWidth - column - 1;
        for (int foldedColumn = 1; foldedColumn <= nbLinesToFold; foldedColumn++) {
            for (int i = 0; i < MapHeight; i++) {
                var currentIndex = column + foldedColumn + i * MapWidth;
                var newIndex = column - foldedColumn + i * MapWidth;
                DotsMap[newIndex] = DotsMap[newIndex] || DotsMap[currentIndex];
            }
        }
        var currentDotsMap = DotsMap.ToList();
        DotsMap = new List<bool>();
        for (int i = 0; i < MapWidth * MapHeight; i++) {
            var x = i % MapWidth;
            if (x < column) {
                DotsMap.Add(currentDotsMap[i]);
            }
        }
        MapWidth = column;
        Debug.Assert(DotsMap.Count == MapWidth * MapHeight);
    }

    public void PrintToConsole() {
        for (int i = 0; i < MapHeight; i++) {
            Console.WriteLine(string.Concat(DotsMap.Skip(i * MapWidth).Take(MapWidth).Select(b => b ? '#' : '.')));
        }
        Console.WriteLine();
    }
}
