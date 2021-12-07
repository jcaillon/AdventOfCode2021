Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var crabPosition = File.ReadAllText(inputFilePath).TrimEnd().Split(',').Select(s => int.Parse(s)).ToArray();

        var costForEachPosition = new List<int>();
        for (int i = crabPosition.Min(); i <= crabPosition.Max(); i++) {
            costForEachPosition.Add(crabPosition.Select(pos => Math.Abs(pos - i).CrabFuelConsumptionFromLength()).Sum());
        }

        return $"The minimum fuel consumption is for position {costForEachPosition.FindIndex(x => x == costForEachPosition.Min()) + crabPosition.Min()} with {costForEachPosition.Min()} fuel consumed";
    }

    public static int CrabFuelConsumptionFromLength(this int length) {
        var cost = 0;
        for (int i = 0; i <= length; i++) {
            cost += i;
        }
        return cost;
    }
}
