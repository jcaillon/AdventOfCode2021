using System.Diagnostics;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var inputList = File.ReadAllLines(inputFilePath);
        var cave = new Cave(inputList.SelectMany(c => c).Select(c => byte.Parse($"{c}")).ToList(), inputList[0].Length, inputList.Length);
        var multiply = 1;
        foreach (var basinSize in cave.Basins.Select(b => b.Indexes.Count).OrderBy(l => l).TakeLast(3)) {
            multiply *= basinSize;
        }
        return $"The multiplication of the 3 largest basins is {multiply}";
    }
}

class Cave {
    public List<byte> HeightMap { get; private set; }

    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }
    public List<Basin> Basins { get; private set; }

    public Cave(List<byte> heightMap, int mapWidth, int mapHeight) {
        HeightMap = heightMap;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Debug.Assert(MapHeight * MapWidth == HeightMap.Count);
        Basins = new List<Basin>();
        ComputeBasins();
    }

    private void ComputeBasins() {
        for (int yy = 0; yy < MapHeight; yy++) {
            for (int xx = 0; xx < MapWidth; xx++) {
                var ii = yy * MapWidth + xx;
                if (Basins.All(b => !b.Indexes.Contains(ii)) && HeightMap[ii] < 9) {
                    // ii is the position of a new Basin, we can explore in all directions from here.
                    // Exploration class to avoid a bad recursive pattern.
                    var exploration = new Exploration();
                    exploration.AddNewIndexToExplore(ii);
                    do {
                        var indexToExplore = exploration.GetNext();
                        var xToExplore = indexToExplore % MapWidth;
                        var yToExplore = indexToExplore / MapWidth;
                        if (yToExplore > 0 && HeightMap[indexToExplore - MapWidth] < 9)
                            exploration.AddNewIndexToExplore(indexToExplore - MapWidth);
                        if (xToExplore > 0 && HeightMap[indexToExplore - 1] < 9)
                            exploration.AddNewIndexToExplore(indexToExplore - 1);
                        if (yToExplore < MapHeight - 1 && HeightMap[indexToExplore + MapWidth] < 9)
                            exploration.AddNewIndexToExplore(indexToExplore + MapWidth);
                        if (xToExplore < MapWidth - 1 && HeightMap[indexToExplore + 1] < 9)
                            exploration.AddNewIndexToExplore(indexToExplore + 1);
                    } while (exploration.HasNewIndexToExplore);
                    Basins.Add(new Basin(exploration.IndexesExplored));
                }
            }
        }
    }

    class Exploration {
        public IEnumerable<int> IndexesExplored => UniqueIndexesExplored;
        HashSet<int> UniqueIndexesExplored { get; } = new HashSet<int>();
        Stack<int> IndexesLeftToExplore { get; } = new Stack<int>();
        public int GetNext() {
            return IndexesLeftToExplore.Pop();
        }
        public bool HasNewIndexToExplore => IndexesLeftToExplore.Count > 0;
        public void AddNewIndexToExplore(int index) {
            if (!UniqueIndexesExplored.Contains(index)) {
                UniqueIndexesExplored.Add(index);
                IndexesLeftToExplore.Push(index);
            }
        }
    }
}

class Basin {
    public List<int> Indexes { get; } = new List<int>();
    public Basin(IEnumerable<int> indexes) => Indexes.AddRange(indexes);
}
