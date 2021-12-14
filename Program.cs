using System.Diagnostics;
using System.Drawing;
using System.Text;

Console.WriteLine(Puzzle.Solve("input-test", 10, true));
Console.WriteLine(Puzzle.Solve("input", 10, false));

static class Puzzle {
    public static string Solve(string inputFilePath, int stepNumber, bool displayResult) {
        var inputList = File.ReadAllLines(inputFilePath);
        var pairInsertionsRules = inputList.Where(s => s.Contains('-')).Select(s => { var sp = s.Split(" -> "); return new InsertionRule(sp[0], sp[1]); }).ToList();
        var polymerizationEquipement = new PolymerizationEquipement(inputList[0], pairInsertionsRules);

        if (displayResult)
            Console.WriteLine($"Template     : {polymerizationEquipement.PolymerTemplate}");

        for (int i = 0; i < stepNumber; i++) {
            polymerizationEquipement.ExecutePolymerInsertion();
            if (displayResult)
                Console.WriteLine($"After step {polymerizationEquipement.Step,-2}: {polymerizationEquipement.Polymer}");
        }

        var groupedByElements = polymerizationEquipement.Polymer.ToList().GroupBy(c => c).OrderBy(g => g.Count()).ToList();
        return $"The quantity of the most common element minus the quantity of the least common element is {groupedByElements.Last().Count() - groupedByElements.First().Count()}.";
    }
}

class PolymerizationEquipement {
    public string PolymerTemplate { get; }
    public string Polymer { get; private set; }
    public List<InsertionRule> PairInsertionRules { get; private set; }
    public int Step { get; private set; }

    public PolymerizationEquipement(string polymerTemplate, List<InsertionRule> pairInsertionRules) {
        PolymerTemplate = polymerTemplate;
        Polymer = polymerTemplate;
        PairInsertionRules = pairInsertionRules;
        Step = 0;
    }
    public void ExecutePolymerInsertion() {
        Step++;
        var sb = new StringBuilder();
        for (int i = 0; i < Polymer.Length - 1; i++) {
            var adjacentElements = Polymer.Substring(i, 2);
            foreach (var rule in PairInsertionRules) {
                adjacentElements = rule.ApplyRule(adjacentElements);
                if (adjacentElements.Length > 2)
                    break;
            }
            sb.Append(adjacentElements.Substring(0, 2));
        }
        sb.Append(Polymer.Last());
        Polymer = sb.ToString();
    }

}

class InsertionRule {
    public string AdjacentElements { get; private set; }
    public string InsertedElement { get; private set; }

    public InsertionRule(string adjacentElements, string insertedElement) {
        AdjacentElements = adjacentElements;
        InsertedElement = insertedElement;
    }
    public string ApplyRule(string adjacentElements) {
        return adjacentElements.Equals(AdjacentElements) ? $"{AdjacentElements[0]}{InsertedElement}{AdjacentElements[1]}" : adjacentElements;
    }
}
