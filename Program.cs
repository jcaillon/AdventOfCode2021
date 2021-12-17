using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var input = File.ReadAllText(inputFilePath);
        var reg = new Regex(@"[-\d]+").Matches(input).Select(m => int.Parse(m.Value)).ToList();
        var targetArea = new Rectangle(reg[0], reg[2], reg[1] - reg[0], reg[3] - reg[2]);

        var oceanTrench = new OceanTrench();
        oceanTrench.FindAllTrajectories(targetArea);

        return $"The highest y position reached by a valid trajectory is {oceanTrench.Simulations.SelectMany(s => s.Trajectory).Select(p => p.Y).Max()}";
    }
}

class OceanTrench {
    public List<Simulation> Simulations { get; private set; } = new List<Simulation>();
    public void FindAllTrajectories(Rectangle targetArea) {
        // initial x velocity must be non negative and < targetArea.Left because we can't overshoot target area after 1st step
        // initial y velocity must be non negative (find highest curve) and velocity at y = 0 is equal to the opposite of the initial velocity
        // so initial y velocity can't be more than targetArea.Top or next step after reaching y = 0 would overshoot
        foreach (var initialVelocity in
            from x in Enumerable.Range(1, targetArea.Left)
            from y in Enumerable.Range(0, Math.Abs(targetArea.Top))
            select new Point(x, y)) {
            Simulations.Add(new Simulation(targetArea, new Point(0, 0), initialVelocity));
        }
        Simulations = Simulations.Where(s => s.TrajectoryInTargetArea).ToList();
    }
}

class Simulation {
    public Rectangle TargetArea { get; private set; }
    public List<Point> Trajectory { get; private set; }
    public Point InitialPosition { get; private set; }
    public Point InitialVelocity { get; private set; }
    public bool TrajectoryInTargetArea { get; private set; }

    public Simulation(Rectangle targetArea, Point initialPosition, Point initialVelocity) {
        TargetArea = targetArea;
        InitialPosition = initialPosition;
        InitialVelocity = initialVelocity;
        Trajectory = new List<Point>() { initialPosition };
        Simulate();
    }

    private void Simulate() {
        var currentPoint = InitialPosition;
        var currentVelocity = InitialVelocity;
        while(!TrajectoryInTargetArea && currentPoint.X <= TargetArea.Right && currentPoint.Y >= TargetArea.Bottom) {
            currentPoint.Offset(currentVelocity.X, currentVelocity.Y);
            currentVelocity.Offset(currentVelocity.X > 0 ? -1 : 0, -1);
            TrajectoryInTargetArea = TargetArea.Contains(currentPoint);
            Trajectory.Add(currentPoint);
        }
    }

}
