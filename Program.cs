using System.Diagnostics;

Console.WriteLine(Puzzle.Solve("input-test", 10));
Console.WriteLine(Puzzle.Solve("input-test", 100));
Console.WriteLine(Puzzle.Solve("input", 100));

static class Puzzle {
    public static string Solve(string inputFilePath, int nbSteps) {
        var inputList = File.ReadAllLines(inputFilePath);
        var cavern = new Cavern(inputList.SelectMany(c => c).Select(c => byte.Parse($"{c}")).ToList(), inputList[0].Length, inputList.Length);
        var totalFlashes = 0;
        for (int i = 0; i < nbSteps; i++) {
            cavern.NextStep();
            totalFlashes += cavern.TotalFlashNumber;
            if (nbSteps < 5) {
                Console.WriteLine($"Step {cavern.Step}");
                for (int j = 0; j < cavern.MapHeight; j++) {
                    Console.WriteLine(String.Concat(cavern.OctopusesEnergyLevel.Skip(j * cavern.MapWidth).Take(cavern.MapWidth)));
                }
            }
        }
        return $"After {nbSteps} steps, there has been a total of {totalFlashes} flashes.";
    }
}

class Cavern {
    public List<byte> OctopusesEnergyLevel { get; private set; }

    public int MapWidth { get; private set; }
    public int MapHeight { get; private set; }
    public int Step { get; private set; }
    public int TotalFlashNumber => OctopusesEnergyLevel.Count(energy => energy == 0);

    public Cavern(List<byte> octopusesEnergyLevel, int mapWidth, int mapHeight) {
        OctopusesEnergyLevel = octopusesEnergyLevel;
        MapWidth = mapWidth;
        MapHeight = mapHeight;
        Step = 0;
        Debug.Assert(MapHeight * MapWidth == OctopusesEnergyLevel.Count);
    }

    public void NextStep() {
        Step++;
        var indexesFlashing = new Queue<int>();
        for (int i = 0; i < OctopusesEnergyLevel.Count; i++) {
            if (++OctopusesEnergyLevel[i] > 9) {
                indexesFlashing.Enqueue(i);
            }
        }
        Debug.Assert(OctopusesEnergyLevel.All(e => e > 0));
        var adjacentDeltas = new int[] { -MapWidth - 1, -MapWidth, -MapWidth + 1, -1, 1, MapWidth - 1, MapWidth, MapWidth + 1 };
        var adjacentDeltasX = new int[] { -1, 0, +1, -1, +1, -1, 0, +1 };
        var adjacentDeltasY = new int[] { -1, -1, -1, 0, 0, +1, +1, +1 };
        while(indexesFlashing.Count > 0) {
            var currentIndex = indexesFlashing.Dequeue();
            var currentX = currentIndex % MapWidth;
            var currentY = currentIndex / MapWidth;
            var adjacentDeltaIndexesAllowed = new List<int>();
            for (int i = 0; i < adjacentDeltas.Length; i++) {
                if (currentX + adjacentDeltasX[i] >= 0 && currentX + adjacentDeltasX[i] < MapWidth
                    && currentY + adjacentDeltasY[i] >= 0 && currentY + adjacentDeltasY[i] < MapHeight) {
                    adjacentDeltaIndexesAllowed.Add(i);
                }
            }
            foreach (var adjacentDeltaIndexAllowed in adjacentDeltaIndexesAllowed) {
                var adjacentIndex = currentIndex + adjacentDeltas[adjacentDeltaIndexAllowed];
                if (IncreaseEnergyLevel(adjacentIndex) > 9) {
                    indexesFlashing.Enqueue(adjacentIndex);
                }
            }
        }
        OctopusesEnergyLevel = OctopusesEnergyLevel.Select(energy => energy > 9 ? (byte) 0 : energy).ToList();
    }

    private int IncreaseEnergyLevel(int index) {
        if (index >= 0 && index < OctopusesEnergyLevel.Count && OctopusesEnergyLevel[index] <= 9) {
            return ++OctopusesEnergyLevel[index];
        }
        return -1;
    }
}
