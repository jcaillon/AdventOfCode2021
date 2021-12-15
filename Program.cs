using System.Diagnostics;
using System.Drawing;
using System.Text;

Console.WriteLine(Puzzle.Solve("input-test", 1));
Console.WriteLine(Puzzle.Solve("input", 1));
Console.WriteLine(Puzzle.Solve("input-test", 5));
Console.WriteLine(Puzzle.Solve("input", 5));

static class Puzzle {
    public static string Solve(string inputFilePath, int scale) {
        var inputList = File.ReadAllLines(inputFilePath);
        var cavern = new Cavern(inputList, inputList[0].Length, inputList.Length, scale);
        return $"The lowest total risk of any path for scale {scale} is {cavern.SumOfRiskLevelMap[new Point(cavern.MapWidth - 1, cavern.MapHeight - 1)]}.";
    }
}

class Cavern {
    public Dictionary<Point, int> RiskLevelMap { get; private set; }
    public Dictionary<Point, int> SumOfRiskLevelMap { get; private set; }

    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }

    public Cavern(string[] riskLevelMap, int mapWidth, int mapHeight, int scale) {
        RiskLevelMap = new Dictionary<Point, int>(
            from y in Enumerable.Range(0, mapHeight)
            from x in Enumerable.Range(0, mapWidth)
            select new KeyValuePair<Point, int>(new Point(x, y), int.Parse($"{riskLevelMap[y][x]}"))
        );
        if (scale > 1) {
            var newRiskLevelMap = new Dictionary<Point, int>();
            foreach (var kpv in RiskLevelMap) {
                for (int sv = 0; sv < scale; sv++) {
                    for (int sh = 0; sh < scale; sh++) {
                        var risk = kpv.Value + sh + sv;
                        if (risk > 9)
                            risk = risk - 9;
                        newRiskLevelMap.Add(new Point(kpv.Key.X + sh * mapWidth, kpv.Key.Y + sv * mapHeight), risk);
                    }
                }
            }
            RiskLevelMap = newRiskLevelMap;
        }
        MapWidth = mapWidth * scale;
        MapHeight = mapHeight * scale;
        Debug.Assert(MapHeight * MapWidth == RiskLevelMap.Count);
        ComputeSumOfRiskLevelMap();
    }

    private void ComputeSumOfRiskLevelMap() {
        var endingPoint = new Point(MapWidth - 1, MapHeight - 1);
        var pointsToExplore = new PriorityQueue<Point, int>(); // will dequeue lowest priority first (has the best odds to be the best path)

        SumOfRiskLevelMap = new Dictionary<Point, int>();
        SumOfRiskLevelMap[new Point(0, 0)] = 0;
        pointsToExplore.Enqueue(new Point(0, 0), 0);

        // Go until we find the bottom right corner
        do {
            var currentPoint = pointsToExplore.Dequeue();
            if (currentPoint == endingPoint) {
                break;
            }
            foreach (var adjacentPoint in GetAdjacentPoints(currentPoint)) {
                if (RiskLevelMap.ContainsKey(adjacentPoint)) {
                    var totalRiskThroughP = SumOfRiskLevelMap[currentPoint] + RiskLevelMap[adjacentPoint];
                    if (totalRiskThroughP < SumOfRiskLevelMap.GetValueOrDefault(adjacentPoint, int.MaxValue)) {
                        SumOfRiskLevelMap[adjacentPoint] = totalRiskThroughP;
                        pointsToExplore.Enqueue(adjacentPoint, totalRiskThroughP);
                    }
                }
            }
        } while (true);
    }

    IEnumerable<Point> GetAdjacentPoints(Point point) =>
        new[] {
            new Point(point.X, point.Y+1),
            new Point(point.X, point.Y-1),
            new Point(point.X+1, point.Y),
            new Point(point.X-1, point.Y)
        };
}
