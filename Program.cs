using NUnit.Framework;

Console.WriteLine(Puzzle.Solve("input-test", 18));
Console.WriteLine(Puzzle.Solve("input-test", 80));
Console.WriteLine(Puzzle.SolveFast("input-test", 18));
Console.WriteLine(Puzzle.SolveFast("input-test", 80));
Console.WriteLine(Puzzle.SolveFast("input-test", 256));
Console.WriteLine(Puzzle.SolveFast("input", 256));

static class Puzzle {
    public static string Solve(string inputFilePath, int nbDays) {
        var lanternFishAgeList = File.ReadAllText(inputFilePath).TrimEnd().Split(',').Select(s => int.Parse(s)).ToList();
        var ocean = new Ocean(lanternFishAgeList);
        ocean.FastFoward(nbDays);
        return $"After {nbDays} days there are {ocean.Fishes.Count} lantern fishes in the ocean.";
    }

    public static string SolveFast(string inputFilePath, int nbDays) {
        // methode with low mem footprint, each value of spawnDayNumber represents the nb of fish that will have new children for each day of the week
        var NbOfFishGettingNewChildrenPerDayNumber = new ulong[7];
        for (int i = 0; i < 7; i++) {
            NbOfFishGettingNewChildrenPerDayNumber[i] = (ulong) File.ReadAllText(inputFilePath).TrimEnd().Split(',').Select(s => int.Parse(s)).Count(d => d == i);
        }
        byte dayNumber = 0;
        ulong babyFishes = 0;
        ulong teenageFishes = 0;
        for (int i = 0; i < nbDays; i++) {
            dayNumber++;
            ulong bornFishes = NbOfFishGettingNewChildrenPerDayNumber[dayNumber - 1];
            NbOfFishGettingNewChildrenPerDayNumber[dayNumber - 1] += teenageFishes;
            teenageFishes = babyFishes;
            babyFishes = bornFishes;
            if (dayNumber >= 7) {
                dayNumber = 0;
            }
        }
        return $"After {nbDays} days there are {NbOfFishGettingNewChildrenPerDayNumber.Sum(u => (decimal) u) + babyFishes + teenageFishes} lantern fishes in the ocean.";
    }

}

class Ocean {
    public List<LanternFish> ParentFishes { get; private set; }

    public List<LanternFish> Fishes {
        get {
            var fishList = new List<LanternFish>();
            var parentFishes = ParentFishes.ToList();
            while (parentFishes.Count > 0) {
                fishList.AddRange(parentFishes);
                parentFishes = parentFishes.SelectMany(f => f.Children).ToList();
            }
            return fishList;
        }
    }

    public int DayNumber { get; private set; } = 0;

    public event EventHandler<int>? NewDayHasPassed;

    public Ocean(List<int> lanternFishAgeList) {
        ParentFishes = lanternFishAgeList.Select(a => new LanternFish(this, 999, a)).ToList();
    }

    public void FastFoward(int dayCount) {
        for (int i = 0; i < dayCount; i++) {
            DayNumber++;
            NewDayHasPassed?.Invoke(this, DayNumber);
        }
    }

}

class LanternFish {
    public int Age { get; private set; }
    public int TimeBeforeNextChild { get; private set; } = 0;

    /// <summary>
    /// Amount of time for <see cref="LanternFish"/> to create a new child.
    /// </summary>
    public int GestationPeriod { get; private set; }

    /// <summary>
    /// Age at which a <see cref="LanternFish"/> can start reproducing.
    /// </summary>
    public int ReproductionAge { get; private set; }

    public List<LanternFish> Children { get; private set; } = new List<LanternFish>();

    public Ocean Home { get; private set; }

    public LanternFish(Ocean home, int age, int timeBeforeNextChild, int gestationPeriod = 7, int reproductionAge = 2) {
        Age = age;
        GestationPeriod = gestationPeriod;
        ReproductionAge = reproductionAge;
        Home = home;
        Home.NewDayHasPassed += HandleNewDay;
        TimeBeforeNextChild = timeBeforeNextChild;
    }

    private void HandleNewDay(object? sender, int newDayNumber) {
        Age++;
        if (Age >= ReproductionAge) {
            if (TimeBeforeNextChild == 0) {
                TimeBeforeNextChild = GestationPeriod;
                if (Age > ReproductionAge) {
                    Children.Add(new LanternFish(Home, 0, 0, GestationPeriod, ReproductionAge));
                }
            }
            TimeBeforeNextChild--;
        }
    }
}

namespace Test {

    [TestFixture]
    public class TestPuzzle {

        [Test]
        public void SolveIsCorrect() {
            Assert.AreEqual(Puzzle.Solve("input-test", 18), "After 18 days there are 26 lantern fishes in the ocean.");
            Assert.AreEqual(Puzzle.Solve("input-test", 80), "After 18 days there are 5934 lantern fishes in the ocean.");
            Assert.AreEqual(Puzzle.Solve("input-test", 256), "After 18 days there are 26984457539 lantern fishes in the ocean.");
        }
    }
}
