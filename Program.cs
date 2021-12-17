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
        oceanTrench.FindAllSimulationsLandingTo(targetArea);

        return $"There are a total of {oceanTrench.Simulations.Count()} initial velocity that satisfy the simulation goal";
    }
}

class OceanTrench {
    public List<Simulation> Simulations { get; private set; } = new List<Simulation>();
    public void FindAllSimulationsLandingTo(Rectangle targetArea) {
        var xVelMin = 1; // must be >0 to go somewhere
        var xVelMax = targetArea.Right; // can't be more because we would overshoot target area after 1st step
        var yVelMin = -Math.Abs(targetArea.Top); // can't be less because we would overshoot target area after 1st step
        var yVelMax = Math.Abs(targetArea.Top); // at y = 0 velocity is equal to the opposite of the initial velocity so initial velocity can't be more than that or next step after reaching y = 0 would overshoot
        for (int xVel = xVelMin; xVel <= xVelMax; xVel++) {
            for (int yVel = yVelMin; yVel <= yVelMax; yVel++) {
                Simulations.Add(new Simulation(targetArea, new Point(0, 0), new Point(xVel, yVel)));
            }
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
        while(!TrajectoryInTargetArea && currentPoint.X <= TargetArea.Right && currentPoint.Y >= TargetArea.Top) {
            currentPoint.Offset(currentVelocity.X, currentVelocity.Y);
            currentVelocity.Offset(currentVelocity.X > 0 ? -1 : 0, -1);
            TrajectoryInTargetArea = currentPoint.X >= TargetArea.Left && currentPoint.X <= TargetArea.Right && currentPoint.Y >= TargetArea.Top && currentPoint.Y <= TargetArea.Bottom;
            Trajectory.Add(currentPoint);
        }
    }

}
