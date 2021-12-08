using System.Diagnostics;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var entryList = File.ReadAllLines(inputFilePath)
            .Select(s => {
                var split = s.Split('|');
                return new Entry(split[0].Trim().Split(' '), split[1].Trim().Split(' '));
            })
            .ToList();

        return $"There is a total of {entryList.SelectMany(entry => entry.OutputMixedSignals).Count(signal => signal.Length < 5 || signal.Length > 6)} 1, 4, 7 and 8";
    }
}

class Entry {
    public List<Signal> MixedSignals { get; set; }

    public List<Signal> OutputMixedSignals { get; set; }

    public Entry(IEnumerable<string> mixedSignals, IEnumerable<string> output) {
        MixedSignals = mixedSignals.Select(s => new Signal(string.Concat(s.OrderBy(c => c)))).ToList();
        OutputMixedSignals = output.Select(s => new Signal(string.Concat(s.OrderBy(c => c)))).ToList();
    }

}


class Signal {
    public string Segments { get; set; }
    public int Length => Segments.Length;
    public Signal(string segmentsOn) {
        Segments = segmentsOn;
    }
}


