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

        return $"The sum all of output is {entryList.Select(entry => entry.CorrectedOutput).Sum()}";
    }
}

class Entry {
    public List<Signal> MixedSignals { get; set; }

    public List<Signal> OutputMixedSignals { get; set; }

    public int CorrectedOutput => int.Parse(string.Concat(OutputMixedSignals.Select(s => ConvertOutput(s).ToString())));
    private byte? ConvertOutput(Signal OutputMixedSignal) {
        var correctedSegments = SegmentMapping.ConvertToStandardSegments(OutputMixedSignal.Segments);
        return correctedSegments != null && StandardSegmentsDisplay.ContainsKey(correctedSegments) ? StandardSegmentsDisplay[correctedSegments] : null;
    }
    public Entry(IEnumerable<string> mixedSignals, IEnumerable<string> output) {
        MixedSignals = mixedSignals.Select(s => new Signal(string.Concat(s.OrderBy(c => c)))).ToList();
        OutputMixedSignals = output.Select(s => new Signal(string.Concat(s.OrderBy(c => c)))).ToList();
        SegmentMapping = new SegmentMapping(MixedSignals);
        
    }

    public SegmentMapping SegmentMapping { get; set; }

    private static Dictionary<string, byte>? _standardSegmentsDisplay;
    public static Dictionary<string, byte> StandardSegmentsDisplay {
        get {
            if (_standardSegmentsDisplay == null) {
                _standardSegmentsDisplay = new Dictionary<string, byte>() {
                    { "abcefg", 0 },
                    { "cf", 1 },
                    { "acdeg", 2 },
                    { "acdfg", 3 },
                    { "bcdf", 4 },
                    { "abdfg", 5 },
                    { "abdefg", 6 },
                    { "acf", 7 },
                    { "abcdefg", 8 },
                    { "abcdfg", 9 }
                };
            }
            return _standardSegmentsDisplay;
        }
    }
}

class SegmentMapping {
    public Dictionary<char, char> Map { get; set; }

    public string ConvertToStandardSegments(string mixedSegments) => string.Concat(mixedSegments.Select(c => Map[c]).OrderBy(c => c));

    public SegmentMapping(List<Signal> mixedSignals) {
        Map = FindCorrectMapping(mixedSignals);
    }

    private static Dictionary<char, char> FindCorrectMapping(List<Signal> mixedSignals) {
        var RevMap = new Dictionary<char, char>();
        // can guess the mapping for a
        var segmForOne = mixedSignals.Find(s => s.Length == 2)?.Segments;
        var segmForSeven = mixedSignals.Find(s => s.Length == 3)?.Segments;
        Debug.Assert(segmForOne != null && segmForSeven != null);
        var valForA = segmForSeven.Replace($"{segmForOne[0]}", "").Replace($"{segmForOne[1]}", "")[0];
        RevMap.Add('a', valForA);
        var concatSegments = string.Concat(mixedSignals.Select(s => s.Segments));
        // occurences of a letter gives us some info
        // a = 8
        // b = 6
        // c = 8
        // d = 7
        // e = 4
        // f = 9
        // g = 7
        concatSegments = concatSegments.Replace(valForA, 'x');
        var nbOccurencePerLetter = new List<int>();
        for (int i = 0; i < 7; i++) {
            var l = Convert.ToChar(97 + i);
            nbOccurencePerLetter.Add(concatSegments.Count(c => c == Convert.ToChar(97 + i))); // a = 97
        }
        RevMap.Add('b', (char) (97 + nbOccurencePerLetter.FindIndex(occurrence => occurrence == 6)));
        RevMap.Add('c', (char) (97 + nbOccurencePerLetter.FindIndex(occurrence => occurrence == 8)));
        RevMap.Add('e', (char) (97 + nbOccurencePerLetter.FindIndex(occurrence => occurrence == 4)));
        RevMap.Add('f', (char) (97 + nbOccurencePerLetter.FindIndex(occurrence => occurrence == 9)));
        var segmForFour = mixedSignals.Find(s => s.Length == 4)?.Segments;
        Debug.Assert(segmForFour != null);
        var valForD = segmForFour.Replace($"{RevMap['b']}", "").Replace($"{RevMap['c']}", "").Replace($"{RevMap['f']}", "")[0];
        RevMap.Add('d', valForD);
        var guess1 = Convert.ToChar(97 + nbOccurencePerLetter.FindIndex(occurrence => occurrence == 7));
        var valForG = guess1 == valForD ? Convert.ToChar(97 + nbOccurencePerLetter.FindLastIndex(occurrence => occurrence == 7)) : guess1;
        RevMap.Add('g', valForG);

        var map = new Dictionary<char, char>();
        foreach (var kpv in RevMap) {
            map.Add(kpv.Value, kpv.Key);
        }
        return map;
    }
}

class Signal {
    public string Segments { get; }
    public int Length => Segments.Length;
    public Signal(string segmentsOn) {
        Segments = segmentsOn;
    }
}


