using System.Diagnostics;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var inputList = File.ReadAllLines(inputFilePath);

        var nav = new NavigationSystem(inputList);

        return $"Total syntax error score is {nav.ParsedLines.Select(p => p.Score).Sum()}";
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
    public string RawLine { get; }
    public List<string> Chunks { get; }
    public int FirstIllegalCharPosition { get; } = -1;
    public bool IsCorrupted => FirstIllegalCharPosition > -1;
    public bool IsIncomplete { get; }
    public int Score {
        get {
            var indexOfFirstIllegalChar = legalEndingChar.FindIndex(FirstIllegalChar);
            return indexOfFirstIllegalChar > -1 ? charScore[indexOfFirstIllegalChar] : 0;
        }
    }
    public char FirstIllegalChar => FirstIllegalCharPosition > -1 ? RawLine[FirstIllegalCharPosition] : (char) 0;
    public ParsedLine(string rawLine) {
        RawLine = rawLine;
        Chunks = new List<string>();
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
        IsIncomplete = FirstIllegalCharPosition == 0 && openedCharIndexStack.Count > 0;
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
