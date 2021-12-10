using System.Diagnostics;

//Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var inputList = File.ReadAllLines(inputFilePath);

        var nav = new NavigationSystem(inputList);
        var incomplete = nav.ParsedLines.Where(p => !string.IsNullOrEmpty(p.MissingChars)).OrderBy(p => p.MissingCharScore).ToList();
        foreach (var inc in incomplete) {
            Console.WriteLine($"{inc.MissingChars} -> {inc.MissingCharScore}");
        }
        return $"Middle score is {incomplete[incomplete.Count / 2].MissingCharScore}";
    }
}

class NavigationSystem {
    public List<ParsedLine> ParsedLines { get; private set; }
    public NavigationSystem(IEnumerable<string> navigationCode) {
        ParsedLines = navigationCode.Select(l => new ParsedLine(l)).ToList();
    }
}

class ParsedLine {
    private char [] legalStartingChar = new char[] { '(', '[', '{', '<' };
    private char[] legalEndingChar = new char[] { ')', ']', '}', '>' };
    private int[] charScore = new int[] { 3, 57, 1197, 25137 };
    private ulong[] missingCharScore = new ulong[] { 1, 2, 3, 4 };
    public string RawLine { get; }
    public List<string> Chunks { get; }
    public int FirstIllegalCharPosition { get; } = -1;
    public bool IsCorrupted => FirstIllegalCharPosition > -1;
    public bool IsIncomplete => FirstIllegalCharPosition == -1 && !string.IsNullOrEmpty(MissingChars);
    public string? MissingChars { get; }
    public int Score {
        get {
            var indexOfFirstIllegalChar = legalEndingChar.FindIndex(FirstIllegalChar);
            return indexOfFirstIllegalChar > -1 ? charScore[indexOfFirstIllegalChar] : 0;
        }
    }
    public ulong MissingCharScore {
        get {
            var total = (ulong) 0;
            if (!string.IsNullOrEmpty(MissingChars)) {
                foreach (var c in MissingChars) {
                    var idx = legalEndingChar.FindIndex(c);
                    var f = missingCharScore[legalEndingChar.FindIndex(c)];
                    total = total * 5 + missingCharScore[legalEndingChar.FindIndex(c)];
                }
            }
            return total;
        }
    }
    public char FirstIllegalChar => FirstIllegalCharPosition > -1 ? RawLine[FirstIllegalCharPosition] : (char) 0;
    public ParsedLine(string rawLine) {
        RawLine = rawLine;
        Chunks = new List<string>();
        MissingChars = null;
        var openedCharIndexStack = new Stack<int>();
        var chunkStartPos = 0;
        for (var currentPos = 0; currentPos < RawLine.Length; currentPos++) {
            var currentChar = RawLine[currentPos];
            var foundOpeningCharIndex = legalStartingChar.FindIndex(currentChar);
            if (foundOpeningCharIndex > -1) {
                openedCharIndexStack.Push(foundOpeningCharIndex);
            } else {
                var foundClosingCharIndex = legalEndingChar.FindIndex(currentChar);
                if (foundClosingCharIndex > -1) {
                    var lastOpeningCharIndex = openedCharIndexStack.Pop();
                    if (lastOpeningCharIndex != foundClosingCharIndex) {
                        FirstIllegalCharPosition = currentPos;
                        break;
                    } else if (openedCharIndexStack.Count == 0) {
                        Chunks.Add(rawLine.Substring(chunkStartPos, currentPos - chunkStartPos + 1));
                        chunkStartPos = currentPos + 1;
                    }
                } else {
                    // wrong char
                    FirstIllegalCharPosition = currentPos;
                    break;
                }
            }
        }
        if (FirstIllegalCharPosition == -1 && openedCharIndexStack.Count > 0) {
            MissingChars = string.Concat(openedCharIndexStack.Select(i => legalEndingChar[i]));
        }
    }
}

public static class ParsedLineExtensions {
    public static int FindIndex(this char[] chars, char charToFind) {
        for (int i = 0; i < chars.Length; i++) {
            if (chars[i] == charToFind) {
                return i;
            }
        }
        return -1;
    }
}
