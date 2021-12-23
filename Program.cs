using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

Console.WriteLine(Puzzle.Solve("input-test"));
Console.WriteLine(Puzzle.Solve("input"));

static class Puzzle {
    public static string Solve(string inputFilePath) {
        var snailfishes = new List<Snailfish>();
        foreach (var line in File.ReadAllLines(inputFilePath)) {
            snailfishes.Add(new Snailfish(line));
        }
        Snailfish finalSum = snailfishes[0].Reduce();
        for (int i = 1; i < snailfishes.Count; i++) {
            finalSum = finalSum.Add(snailfishes[i].Reduce()).Reduce();
        }

        return $"The magnitude of the final sum is {finalSum.GetMagnitude()} for {finalSum}";
    }
}

public class Element { }
public class Pair : Element {
    public Element[] Elements { get; set; }
    public Pair? Parent { get; set; }
    public Pair(Pair? parent, string toParse) {
        Parent = parent;
        Elements = new Element[2];
        var strIndex = 1;
        if (!char.IsDigit(toParse[strIndex])) {
            var stackCount = 1;
            do {
                strIndex++;
                var curChar = toParse[strIndex];
                if (curChar == '[')
                    stackCount++;
                else if (curChar == ']')
                    stackCount--;
            } while (stackCount > 0);
        }
        Elements[0] = ParseElement(toParse.Substring(1, strIndex));
        Elements[1] = ParseElement(toParse.Substring(strIndex + 2, toParse.Length - strIndex - 3)); // [1,1]
    }
    public Pair(Pair? parent, Element[] elements) {
        Elements = elements;
        Parent = parent;
    }
    private Element ParseElement(string toParse) {
        if (char.IsDigit(toParse[0])) {
            return AddNewNumberAfter(byte.Parse(toParse), null);
        }
        return new Pair(this, toParse);
    }
    protected bool Split() {
        for (int i = 0; i < Elements.Length; i++) {
            if (Elements[i] is Number number) {
                if (number.CanSplit()) {
                    var split = number.Split();
                    var firstNumber = AddNewNumberAfter(split[0], number);
                    Elements[i] = new Pair(this, new Element[] { firstNumber, AddNewNumberAfter(split[1], firstNumber) });
                    RemoveNumber(number);
                    return true;
                }
            } else if (Elements[i] is Pair pair) {
                if (pair.Split())
                    return true;
            }
        }
        return false;
    }
    protected bool Explode() {
        for (int i = 0; i < Elements.Length; i++) {
            if (Elements[i] is Pair pair) {
                var nesting = pair.GetNesting();
                if (nesting >= 4 && pair.Elements[0] is Number left && pair.Elements[1] is Number right) {
                    var leftVal = left.Value;
                    var rightVal = right.Value;
                    var previousNumber = FindAdjacentNumberOf(left, false);
                    if (previousNumber != null)
                        previousNumber.Value += leftVal; // add left value to right most parent number (if any)
                    var nextNumber = FindAdjacentNumberOf(right, true);
                    if (nextNumber != null)
                        nextNumber.Value += rightVal; // add right value to leftmost parent number (if any)
                    Elements[i] = AddNewNumberAfter(0, right);
                    RemoveNumber(left);
                    RemoveNumber(right);
                    return true;
                } else {
                    if (pair.Explode())
                        return true;
                }
            }
        }
        return false;
    }
    private Number AddNewNumberAfter(byte value, Number? previousValue) {
        var newNumber = new Number(value);
        var snailFish = GetSnaifFish();
        if (previousValue == null) {
            snailFish!.Numbers.AddLast(newNumber);
        } else {
            snailFish!.Numbers.AddAfter(snailFish.Numbers.Find(previousValue)!, newNumber);
        }
        return newNumber;
    }
    private void RemoveNumber(Number val) {
        GetSnaifFish()!.Numbers.Remove(val);
    }
    private Number? FindAdjacentNumberOf(Number val, bool seekNext) {
        var snailFish = GetSnaifFish();
        var current = snailFish!.Numbers.Find(val);
        return seekNext ? current!.Next?.Value : current!.Previous?.Value;
    }
    private Snailfish? _snailFish;
    private Snailfish? GetSnaifFish() {
        if (_snailFish == null) {
            var snailFish = this;
            while (snailFish != null && !(snailFish is Snailfish)) {
                snailFish = snailFish.Parent;
            }
            _snailFish = snailFish as Snailfish;
        }
        return _snailFish;
    }
    public long GetMagnitude() {
        return 3 * (Elements[0] is Number left ? left.Value : ((Pair) Elements[0]).GetMagnitude()) +
            2 * (Elements[1] is Number right ? right.Value : ((Pair) Elements[1]).GetMagnitude());
    }
    public int GetNesting() => Parent == null ? 0 : Parent.GetNesting() + 1;
    public override string? ToString() => $"[{Elements[0]},{Elements[1]}]";
}
public class Number : Element {
    public byte Value { get; set; }
    public Number(byte value) => Value = value;
    public bool CanSplit() => Value >= 10;
    public byte[] Split() {
        var left = (byte) Math.Floor((decimal) Value / 2);
        return new byte[] { left, (byte) (Value - left) };
    }
    public override string? ToString() => $"{Value}";
}
public class Snailfish : Pair {
    public LinkedList<Number> Numbers { get; set; } = new LinkedList<Number>();
    public Snailfish(string toParse) : base(null, toParse) { }
    public Snailfish(Element[] elements) : base(null, elements) { }
    public Snailfish Reduce() {
        //Console.WriteLine($"Before: {this}");
        while (true) {
            if (!Explode() && !Split()) {
                break;
            }
            //Console.WriteLine($"        {this}");
        }
        return this;
    }
    public Snailfish Add(Snailfish snailfish) => new Snailfish($"[{this},{snailfish}]");
}

