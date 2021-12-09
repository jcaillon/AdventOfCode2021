using System.Diagnostics;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var inputList = File.ReadAllLines(inputFilePath);
        var cave = new Cave(inputList.SelectMany(c => c).Select(c => byte.Parse($"{c}")).ToList(), inputList[0].Length, inputList.Length);

        return $"The sum of all risk levels is {cave.RiskLevelMap.Sum()}";
    }
}

class Cave {

    public List<byte> HeightMap { get; private set; }

    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }

    public List<int> RiskLevelMap { get; private set; }

    public Cave(List<byte> heightMap, int mapWidth, int mapHeight) {
        HeightMap = heightMap;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Debug.Assert(MapHeight * MapWidth == HeightMap.Count);
        RiskLevelMap = Enumerable.Repeat(0, HeightMap.Count).ToList();
        ComputeRiskMap();
    }

    private void ComputeRiskMap() {
        for (int y = 0; y < MapHeight; y++) {
            for (int x = 0; x < MapWidth; x++) {
                var i = y * MapWidth + x;
                var current = HeightMap[i];
                if ((y == 0 || current < HeightMap[i - MapWidth]) &&
                    (y == MapHeight - 1 || current < HeightMap[i + MapWidth]) &&
                    (x == 0 || current < HeightMap[i - 1]) &&
                    (x == MapWidth - 1 || current < HeightMap[i + 1])) {
                    RiskLevelMap[i] = current + 1;
                }
            }
        }
    }
}
