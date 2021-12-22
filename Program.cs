using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var rebootSteps = new List<Step>();
        foreach (var line in File.ReadAllLines(inputFilePath)) {
            var reg = new Regex(@"[-\d]+").Matches(line).Select(m => int.Parse(m.Value)).ToArray();
            rebootSteps.Add(new Step(line.StartsWith("on"), new Cuboid(new Range(reg[0], reg[1]), new Range(reg[2], reg[3]), new Range(reg[4], reg[5]))));
        }

        var reactor = new Reactor(rebootSteps.ToArray(), new Cuboid(new Range(-50, 50), new Range(-50,50), new Range(-50, 50)));
        reactor.Reboot();

        return $"There are a total of {reactor.TotalNumberOfCubesTurnedOn} cubes turned on after the reboot";
    }
}

public record Reactor(Step[] RebootSteps, Cuboid ReactorGrid) {
    public long TotalNumberOfCubesTurnedOn { get; private set; }

    public void Reboot() {
        TotalNumberOfCubesTurnedOn = ComputeNumberbOfCubesTurnedOnAfterStep(RebootSteps.Length - 1, ReactorGrid);
    }

    /// <summary>
    /// Return how many cubes are turned on inside the cuboid <paramref name="cubToConsider"/> after applying reboot step number <paramref name="step"/>
    /// </summary>
    public long ComputeNumberbOfCubesTurnedOnAfterStep(int step, Cuboid cubToConsider) {
        if (cubToConsider.GetVolume() == 0) {
            return 0;
        }

        var rebootStep = RebootSteps[step];
        var currentCuboid = cubToConsider.GetIntersection(rebootStep.Cuboid);

        long nbOfCubesTurnedOnOutsideCurrentCuboid = 0;
        if (step > 0) {
            var nbCubesTurnedOnAtPreviousStep = ComputeNumberbOfCubesTurnedOnAfterStep(step - 1, cubToConsider);
            var nbCubesTurnedOnAtPreviousStepInsideCurrentCuboid = ComputeNumberbOfCubesTurnedOnAfterStep(step - 1, currentCuboid);
            nbOfCubesTurnedOnOutsideCurrentCuboid = nbCubesTurnedOnAtPreviousStep - nbCubesTurnedOnAtPreviousStepInsideCurrentCuboid;
        }
        return nbOfCubesTurnedOnOutsideCurrentCuboid + (rebootStep.TurnOn ? currentCuboid.GetVolume() : 0);
    }
}

public record Step(bool TurnOn, Cuboid Cuboid);

public record Cuboid(Range xRange, Range yRange, Range zRange) {
    public Cuboid GetIntersection(Cuboid anotherCuboid) =>
        new Cuboid(xRange.GetIntersection(anotherCuboid.xRange), yRange.GetIntersection(anotherCuboid.yRange), zRange.GetIntersection(anotherCuboid.zRange));
    public long GetVolume() => xRange.GetLength() * yRange.GetLength() * zRange.GetLength();
}
public record Range(long From, long To) {
    public Range GetIntersection(Range anotherRange) => new Range(Math.Max(anotherRange.From, From), Math.Min(anotherRange.To, To));
    public long GetLength() => From > To ? 0 : To - From + 1;
}

