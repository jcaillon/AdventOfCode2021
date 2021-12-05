using System.Drawing;

var input = File.ReadAllLines("input");

var hydroVents = input.Select(s => {
    var points = s.Split(" -> ").Select(s => s.Split(',').Select(s => int.Parse(s)).ToArray()).ToArray();
    return new HydroVent(new Point(points[0][0], points[0][1]), new Point(points[1][0], points[1][1]));
}).ToList();

// Part1:
//hydroVents = hydroVents.Where(h => h.Start.X == h.End.X || h.Start.Y == h.End.Y).ToList();

var oceanFloor = OceanFloor.Create(hydroVents);

// Draw small maps
if (oceanFloor.FloorSize.Width < 20)
    for (int i = 0; i < oceanFloor.FloorSize.Height; i++) {
        Console.WriteLine(string.Join(' ', oceanFloor.DangerMap.Skip(i * oceanFloor.FloorSize.Width).Take(oceanFloor.FloorSize.Width)));
    }

Console.WriteLine($"Number point with 2 overlapping lines = {oceanFloor.DangerMap.Count(d => d >= 2)}");

class OceanFloor {
    public List<int> DangerMap { get; private set; }

    public List<HydroVent> HydroVents { get; private set; }

    public Size FloorSize { get; private set; }

    OceanFloor(List<HydroVent> hydroVents, Size? floorSize = null) {
        HydroVents = hydroVents;
        FloorSize = floorSize ?? new Size(
            HydroVents.Select(h => Math.Max(h.Start.X, h.End.X)).Max() + 1,
            HydroVents.Select(h => Math.Max(h.Start.Y, h.End.Y)).Max() + 1
        );
        DangerMap = Enumerable.Repeat(0, FloorSize.Width * FloorSize.Height).ToList();
    }

    public static OceanFloor Create(List<HydroVent> hydroVents, Size? floorSize = null) {
        var oceanFloor = new OceanFloor(hydroVents, floorSize);
        oceanFloor.ComputeDangerMap();
        return oceanFloor;
    }

    private void ComputeDangerMap() {
        foreach (var hydroVent in HydroVents) {
            var xLength = hydroVent.End.X - hydroVent.Start.X;
            var yLength = hydroVent.End.Y - hydroVent.Start.Y;
            for (int i = 0; i <= Math.Max(Math.Abs(xLength), Math.Abs(yLength)); i++) {
                var xIncrement = xLength == 0 ? 0 : Math.Sign(xLength) * i;
                var yIncrement = yLength == 0 ? 0 : Math.Sign(yLength) * i;
                var index = hydroVent.Start.X + xIncrement + (hydroVent.Start.Y + yIncrement) * FloorSize.Width;
                DangerMap[index]++;
            }
        }
    }
}

class HydroVent {
    public Point Start { get; set; }

    public Point End { get; set; }

    public HydroVent(Point start, Point end) {
        Start = start;
        End = end;
    }
}
