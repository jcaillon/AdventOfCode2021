using System.Diagnostics;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var inputList = File.ReadAllLines(inputFilePath);
        var cavern = new Cavern(inputList.Select(s => new Connexion(new Cave(s.Split('-')[0]), new Cave(s.Split('-')[1]))).ToList());

        return $"There are {cavern.Paths.Count} paths through this cave system that visit small caves at most once.";
    }
}

class Cavern {
    public List<Connexion> Connexions { get; private set; }
    public List<Path> Paths { get; private set; } = new List<Path>();

    public Cavern(List<Connexion> connexions) {
        Connexions = connexions;
        ComputePaths();
    }

    private void ComputePaths() {
        foreach (var startConnexion in Connexions.Where(c => c.Cave1.IsStart)) {
            var initialPath = new Path(new List<Cave>() { startConnexion.Cave1, startConnexion.Cave2 });
            var branches = new List<Path>() { initialPath };
            var brancheIndexesToExplore = new Queue<int>();
            brancheIndexesToExplore.Enqueue(0);
            while (brancheIndexesToExplore.Count > 0) {
                var currentBranchIndex = brancheIndexesToExplore.Dequeue();
                var currentBranch = branches[currentBranchIndex];
                var currentCave = currentBranch.Caves.Last();
                var connectedCaves = Connexions.Select(conn => conn.FindConnection(currentCave)).Where(cave => cave != null);
                var nbConnectedCave = 0;
                Cave? caveToAddToCurrentPath = null;
                foreach (var connectedCave in connectedCaves) {
                    var caveBeforeLastCave = currentBranch.GetBeforeLastCave();
                    //if (connectedCave == null || (caveBeforeLastCave?.Name.Equals(connectedCave.Name, StringComparison.CurrentCulture) ?? false))
                    if (connectedCave == null)
                        continue;
                    if (currentCave.IsBig && (caveBeforeLastCave?.IsBig ?? false) && (caveBeforeLastCave?.Name.Equals(connectedCave.Name) ?? false)) {
                        continue; // avoid infinite back and forth between 2 big caves.
                    }
                    nbConnectedCave++;
                    if (!connectedCave.IsBig && currentBranch.Contains(connectedCave)) {
                        // small cave already explored
                        continue;
                    }
                    int indexToExplore;
                    if (nbConnectedCave == 1) {
                        caveToAddToCurrentPath = connectedCave;
                        indexToExplore = currentBranchIndex;
                    } else {
                        indexToExplore = branches.Count;
                        branches.Add(new Path(currentBranch.Caves.Append(connectedCave).ToList()));
                    }
                    if (!connectedCave.IsEnd) {
                        brancheIndexesToExplore.Enqueue(indexToExplore);
                    }
                }
                if (caveToAddToCurrentPath != null)
                    currentBranch.AddCave(caveToAddToCurrentPath);
            }
            Paths.AddRange(branches.Where(path => path.Caves.Last().IsEnd).ToList());
        }
    }
}
class Path {
    public List<Cave> Caves { get; private set; }
    public Path(List<Cave> caves) => Caves = caves;
    public bool Contains(Cave cave) {
        return Caves.Exists(c => c.Name.Equals(cave.Name, StringComparison.CurrentCulture));
    }
    public void AddCave(Cave cave) => Caves.Add(cave);
    public Cave? GetBeforeLastCave() {
        return Caves.Count >= 2 ? Caves[Caves.Count - 2] : null;
    }
}

class Connexion {
    public Cave Cave1 { get; private set; }
    public Cave Cave2 { get; private set; }
    public Connexion(Cave cave1, Cave cave2) {
        Cave1 = cave1.IsStart ? cave1 : cave2;
        Cave2 = cave1.IsStart ? cave2 : cave1;
    }
    public Cave? FindConnection(Cave cave) {
        return cave.Name.Equals(Cave1.Name, StringComparison.CurrentCulture) ? Cave2 : cave.Name.Equals(Cave2.Name, StringComparison.CurrentCulture) ? Cave1 : null;
    }
}

class Cave {
    public string Name { get; private set; }
    public bool IsBig => char.IsUpper(Name[0]);
    public bool IsEnd => "end".Equals(Name);
    public bool IsStart => "start".Equals(Name);
    public Cave(string name) => Name = name;
}
