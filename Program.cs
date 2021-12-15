using System.Diagnostics;
using System.Drawing;
using System.Text;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var inputList = File.ReadAllLines(inputFilePath);
        var cavern = new Cavern(inputList.SelectMany(c => c).Select(c => byte.Parse($"{c}")).ToList(), inputList[0].Length, inputList.Length);

        return $"The lowest total risk of any path is {cavern.SumOfRiskLevelMap?.Last() ?? -1}.";
    }
}

class Cavern {
    public List<byte> RiskLevelMap { get; private set; }
    public List<int>? SumOfRiskLevelMap { get; private set; }

    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }

    public Cavern(List<byte> riskLevelMap, int mapWidth, int mapHeight) {
        RiskLevelMap = riskLevelMap;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Debug.Assert(MapHeight * MapWidth == RiskLevelMap.Count);
        ComputePaths();
    }

    private void ComputePaths() {
        SumOfRiskLevelMap = Enumerable.Repeat(-1, MapWidth * MapHeight).ToList();
        SumOfRiskLevelMap[0] = 0;
        var endingPosition = RiskLevelMap.Count - 1;
        var positionToExplore = new PriorityQueue<int, int>(); // will dequeue lowest priority first (has the best odds to be the best path)
        positionToExplore.Enqueue(0, 0);
        while (positionToExplore.Count > 0) {
            var currentPosition = positionToExplore.Dequeue();
            if (currentPosition == endingPosition)
                break;
            var adjacentPositions = GetAdjacentPositions(currentPosition);
            foreach (var adjacentPosition in adjacentPositions) {
                var newPositionRisk = SumOfRiskLevelMap[currentPosition] + RiskLevelMap[adjacentPosition];
                if (SumOfRiskLevelMap[adjacentPosition] > -1 && newPositionRisk > SumOfRiskLevelMap[adjacentPosition]) {
                    // did we already reach that position with lower risk?
                    continue;
                }
                SumOfRiskLevelMap[adjacentPosition] = newPositionRisk;
                positionToExplore.Enqueue(adjacentPosition, newPositionRisk);
            }
        }
    }

    /// <summary>
    /// Return a list of all possible adjacent positions of <paramref name="currentPosition"/>
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <returns></returns>
    private List<int> GetAdjacentPositions(int currentPosition) {
        var adjacentDeltas = new int[] { -MapWidth, -1, 1, MapWidth };
        var adjacentDeltasX = new int[] { 0, -1, +1, 0 };
        var adjacentDeltasY = new int[] { -1, 0, 0, +1 };
        var currentX = currentPosition % MapWidth;
        var currentY = currentPosition / MapWidth;
        var adjacentPositions = new List<int>();
        for (int i = 0; i < adjacentDeltas.Length; i++) {
            if (currentX + adjacentDeltasX[i] >= 0 && currentX + adjacentDeltasX[i] < MapWidth
                && currentY + adjacentDeltasY[i] >= 0 && currentY + adjacentDeltasY[i] < MapHeight) {
                adjacentPositions.Add(currentPosition + adjacentDeltas[i]);
            }
        }
        return adjacentPositions;
    }
}
