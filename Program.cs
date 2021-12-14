using System.Diagnostics;
using System.Drawing;
using System.Text;

Console.WriteLine(Puzzle.Solve("input-test", 40));
Console.WriteLine(Puzzle.Solve("input", 40));

static class Puzzle {
    public static string Solve(string inputFilePath, int stepNumber) {
        var inputList = File.ReadAllLines(inputFilePath);
        var pairInsertionsRules = inputList.Where(s => s.Contains('-')).Select(s => { var sp = s.Split(" -> "); return new InsertionRule(sp[0], sp[1]); }).ToList();
        var polymerizationEquipement = new PolymerizationEquipement(inputList[0], pairInsertionsRules);

        for (int i = 0; i < stepNumber; i++) {
            polymerizationEquipement.ExecutePolymerInsertion();
        }

        var countByElementSorted = polymerizationEquipement.GetCountByElement().Select(kpv => kpv.Value).OrderBy(c => c);

        return $"The quantity of the most common element minus the quantity of the least common element is {countByElementSorted.Last() - countByElementSorted.First()}.";
    }
}

class PolymerizationEquipement {
    public string PolymerTemplate { get; }
    public Dictionary<string, ulong> PolymerAdjacentElements { get; private set; }
    public List<InsertionRule> PairInsertionRules { get; private set; }
    public int Step { get; private set; }
    public PolymerizationEquipement(string polymerTemplate, List<InsertionRule> pairInsertionRules) {
        PolymerTemplate = polymerTemplate;
        PairInsertionRules = pairInsertionRules;
        Step = 0;
        // add the last element of the template to correctly count it
        PolymerAdjacentElements = new Dictionary<string, ulong> { { PolymerTemplate.Last().ToString() , 1 } };
        for (int i = 0; i < PolymerTemplate.Length - 1; i++) {
            var adjacentElements = PolymerTemplate.Substring(i, 2);
            PolymerAdjacentElements.Merge(adjacentElements, 1);
        }
    }
    public void ExecutePolymerInsertion() {
        Step++;
        var newPolymerAdjacentElements = new Dictionary<string, ulong> { { PolymerTemplate.Last().ToString(), 1 } };
        foreach (var kpv in PolymerAdjacentElements) {
            foreach (var rule in PairInsertionRules) {
                if (rule.IsMatching(kpv.Key)) {
                    rule.NewAdjacentElements.ForEach(newElems => newPolymerAdjacentElements.Merge(newElems, kpv.Value));
                }
            }
        }
        PolymerAdjacentElements = newPolymerAdjacentElements;
    }
    public Dictionary<char, ulong> GetCountByElement() {
        // count for the first element of adjacent elements or we would count twice
        var groupedByElements = PolymerAdjacentElements
            .Select(kpv => new { Element = kpv.Key.First(), Count = kpv.Value })
            .GroupBy(c => c.Element);
        var countByElement = new Dictionary<char, ulong>();
        foreach (var group in groupedByElements) {
            var total = (ulong) 0;
            foreach (var elem in group) {
                total += elem.Count;
            }
            countByElement.Add(group.Key, total);
        }
        return countByElement;
    }
}

class InsertionRule {
    public string AdjacentElements { get; private set; }
    public string InsertedElement { get; private set; }
    public List<string> NewAdjacentElements { get; private set; }

    public InsertionRule(string adjacentElements, string insertedElement) {
        AdjacentElements = adjacentElements;
        InsertedElement = insertedElement;
        NewAdjacentElements = new List<string>();
        NewAdjacentElements.Add($"{AdjacentElements[0]}{InsertedElement}");
        NewAdjacentElements.Add($"{InsertedElement}{AdjacentElements[1]}");
    }
    public bool IsMatching(string adjacentElements) => AdjacentElements.Equals(adjacentElements);
}

public static class Extensions {
    public static void Merge(this Dictionary<string, ulong> dic, string key, ulong value) {
        if (dic.ContainsKey(key)) {
            dic[key] += value;
        } else {
            dic.Add(key, value);
        }
    }
}
